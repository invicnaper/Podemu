﻿
Imports Podemu.Utils
Imports Podemu.Utils.Basic
Imports Podemu.World

Namespace Game
    Public Class Npc

        Public TemplateID As Integer
        Public ID As Integer
        Public CellID, Direction As Integer
        Public WithEvents Map As World.Map
        Friend PendingMap As Integer

        Public ReadOnly Property Template() As NpcTemplate
            Get
                Return NpcsHandler.GetTemplate(TemplateID)
            End Get
        End Property

        Private CachedGameInfo As New CachedPattern(AddressOf CachedGInfo)

        Private Function CachedGInfo() As String
            Dim MyTemplate As NpcTemplate = Template

            Dim Info As String = ";0;"
            Info &= ID & ";" & TemplateID & ";-4;" & MyTemplate.Skin & "^"
            Info &= MyTemplate.Size & ";" & MyTemplate.Sexe & ";"
            For i As Integer = 0 To 2
                Info &= IIf(MyTemplate.Color(i) = -1, "-1", DeciToHex(MyTemplate.Color(i))) & ";"
            Next
            Info &= MyTemplate.ItemsArt & ";"
            Info &= ";"
            Info &= MyTemplate.ArtWork

            Return Info
        End Function

        Public ReadOnly Property GameInfo() As String
            Get

                Return CellID & ";" & Direction & CachedGameInfo.Value

            End Get
        End Property

        Private ListOfTradder As New List(Of GameClient)

        Public Sub RefreshItemsList()
            Dim _template As NpcTemplate = Template
            _template.CachedSellingString.RefreshNow()
            For Each Client In ListOfTradder
                Client.Send("EL" & _template.CachedSellingString.Value)
            Next
        End Sub

        Public Sub SendItemsList(ByVal Client As GameClient)
            Client.Send("EL" & Template.CachedSellingString.Value)
        End Sub

        Public Sub BeginTrade(ByVal Client As GameClient)
            ListOfTradder.Add(Client)
            Client.Character.State.BeginTrade(Trading.Npc, Me)
            Client.Send("ECK0|" & Me.ID)
            SendItemsList(Client)

            SayOpenSentence(Client, 20)
        End Sub

        Public Sub EndTrade(ByVal Client As GameClient)
            ListOfTradder.Remove(Client)
            Client.Character.State.EndTrade()
            Client.Send("EV")
            SayCloseSentence(Client, 20)
        End Sub

        Public Sub Buy(ByVal Client As GameClient, ByVal ItemTemplateId As Integer, ByVal Quantity As Integer)

            Dim Item = Template.SellingList

            If Template.SellingList.Contains(ItemTemplateId) Then

                Dim ItemTemplate As ItemTemplate = ItemsHandler.GetItemTemplate(ItemTemplateId)
                Dim Price As Integer = ItemTemplate.Price * Quantity

                If Client.Character.Player.Kamas >= Price Then

                    Dim NewItem As Item = ItemTemplate.GenerateItem
                    NewItem.Quantity = Quantity
                    NewItem.Position = -1
                    Client.Character.Player.Kamas -= Price
                    RefreshItemsList()
                    Client.Character.Items.AddItem(Client, NewItem)
                    Client.Send("EBK")
                    SayBuySentence(Client, ItemTemplate.Name, Price, 25)
                Else
                    Client.Send("OBE")
                End If
            Else
                Client.Send("OBE")
            End If

        End Sub

        Public Sub Sell(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

            If Not Client.Character.Items.HasItemID(ItemId) Then
                Client.Send("OSE")
                Exit Sub
            End If

            Dim Item As Item = Client.Character.Items.GetItemFromID(ItemId)
            Dim ItemTemplate As ItemTemplate = ItemsHandler.GetItemTemplate(Item.TemplateID)

            If Quantity > Item.Quantity Then Quantity = Item.Quantity

            Dim Gain As Integer = Math.Floor(ItemTemplate.Price / 10) * Quantity

            Client.Character.Player.Kamas += Gain
            Client.Character.Items.DeleteItem(Client, ItemId, Quantity)
            Client.Send("ESK")

        End Sub

        Public Sub Follow(ByVal Client As GameClient)
            Client.Character.State.IsFollowed = True
            SayStartSentence(Client, 100)
            AddHandler Client.Character.OnMoved, AddressOf FollowingCharacter
        End Sub

        Private Sub FollowingCharacter(ByVal Character As Character, ByVal Cell As Integer)

            Dim cMap As World.Map = Character.GetMap

            If Map Is Nothing Or cMap Is Nothing Then
                Return
            End If

            If Map.Id <> cMap.Id Then
                Map.NpcList.Remove(Me)
                Map.RemoveEntity(Me.ID)
                CellID = Character.MapCell
                Direction = Character.MapDir
                cMap.NpcList.Add(Me)
                '    cMap.AddEntity(Me.GameInfo)
                Map = cMap
            End If

            Dim Pathfinding As New Pathfinding()
            Dim finalCell As Integer = 0

            For i As Integer = 0 To 7
                Dim ncell = Cells.NextCell(cMap, Cell, i)
                If cMap.IsCellReachable(ncell) Then
                    finalCell = ncell
                End If
            Next

            If finalCell = 0 Then Return

            Dim Path As String = Pathfinding.Pathing(Map, CellID, finalCell, True)

            If Path.Length <> 0 Then
                cMap.Send("GA0;1;", ID, ";", Utils.Cells.GetDirChar(Direction), Utils.Cells.GetCellChars(CellID), Path)
                CellID = finalCell
            End If
        End Sub

        Private Sub GoToCell(ByVal Client As GameClient)

            SayStartSentence(Client, 100)

            If Map Is Nothing Then
                Return
            End If

            Dim Pathfinding As New Pathfinding()

            Dim dir = If(Client.Character.MapDir Mod 2 = 0, Client.Character.MapDir + 1, Client.Character.MapDir)

            If Not Map.IsCellReachable(Cells.NextCell(Map, Client.Character.MapCell, dir)) Then
                SayImpossibleSentence(Client, 100)
                Return
            End If

            Dim finalCell = Cells.NextCell(Map, Client.Character.MapCell, dir)

            If finalCell = 0 Then Return

            Dim Path As String = Pathfinding.Pathing(Map, CellID, finalCell, True)

            If Path.Length <> 0 Then
                Map.Send("GA0;1;", ID, ";", Utils.Cells.GetDirChar(Direction), Utils.Cells.GetCellChars(CellID), Path)
                CellID = finalCell
            End If

        End Sub

        Private Sub PlayEmote(ByVal EmoteId As Integer)
            Map.Send("eU;" & ID & "|" & EmoteId & "|")
        End Sub

        Sub OnMessage(ByVal Client As GameClient, ByVal Message As String) Handles Map.OnSendedMessage

            If Client.Infos.GmLevel < 1 Then
                Return
            End If

            Select Case Message.Split("|")(0).ToUpper()

                Case "STAND UP"
                    Map.Send("eU;" & ID & "|" & 1 & "|" & 1)

                Case "SIT DOWN"
                    Map.Send("eU;" & ID & "|" & 1 & "|" & 0)

                Case "COME ON"
                    GoToCell(Client)

                Case "MUSIC"
                    PlayEmote(7)

                Case "FOLLOW ME"
                    Follow(Client)

                Case "UN POUR TOUS ET TOUS POUR UN"
                    PlayEmote(6)

                Case "STOP"
                    Say("D'accord je reste ici.")
                    PlayEmote(2)
                    RemoveHandler Client.Character.OnMoved, AddressOf FollowingCharacter

            End Select

        End Sub

        Public Function Percent(ByVal p As Integer) As Boolean
            If (Basic.Rand(0, 100) < p) Then Return True
            Return False
        End Function

        Public Sub Say(ByVal Message As String)
            Map.Send("cMK|" & ID & "|" & Template.Name & "|" & Message)
        End Sub

        Private Sub SayStartSentence(ByVal Client As GameClient, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.START_ACTION)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Say(Message)
            End If
        End Sub

        Private Sub SayImpossibleSentence(ByVal Client As GameClient, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.IMPOSSIBLE)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Say(Message)
            End If
        End Sub

        Private Sub SaySellSentence(ByVal Client As GameClient, ByVal ItemName As String, ByVal Price As Integer, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.SELL)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Message = Replace(Message, "{1}", ItemName)
                Message = Replace(Message, "{2}", Price)
                Say(Message)
            End If
        End Sub

        Private Sub SayBuySentence(ByVal Client As GameClient, ByVal ItemName As String, ByVal Price As Integer, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.BUY)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Message = Replace(Message, "{1}", ItemName)
                Message = Replace(Message, "{2}", Price)
                Say(Message)
            End If
        End Sub

        Private Sub SayOpenSentence(ByVal Client As GameClient, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.OPEN)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Say(Message)
            End If
        End Sub

        Private Sub SayCloseSentence(ByVal Client As GameClient, ByVal Perc As Integer)
            If (Percent(Perc)) Then
                Dim Message = SentencesHandler.GetRandomOf(SentenceType.CLOSE)
                Message = Replace(Message, "{0}", Client.Character.Name)
                Say(Message)
            End If
        End Sub

    End Class
End Namespace