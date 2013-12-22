Imports Podemu.Utils.Basic
Imports Podemu.World

Namespace Game
    Public Class CharacterMobsSuiveurs

        Private MobsSuiveurs As New List(Of MobSuiveur)
        Private _contains As Boolean
        Private _additemMS As Item

        Private Property Contains(ByVal Id As Integer) As Boolean
            Get
                Return _contains
            End Get
            Set(ByVal value As Boolean)
                _contains = value
            End Set
        End Property

        Private Property AdditemMS(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Tours As Integer) As Item
            Get
                Return _additemMS
            End Get
            Set(ByVal value As Item)
                _additemMS = value
            End Set
        End Property

        Public Sub AddMobSuiveur(ByVal Client As GameClient, ByVal Mobid As Integer)
            If Mobid = Nothing Then Exit Sub
            Dim MST As MobSuiveurTemplate = MobsSuiveursManager.GetMSTByMonster(Mobid)
            Dim Monster As MonsterTemplate = MonstersHandler.GetTemplate(Mobid)
            Dim NewMS As New MobSuiveur
            NewMS.Skin = Monster.Skin
            NewMS.Taille = 100
            NewMS.Tours = MST.Tours
            NewMS.Mob = Monster
            If Not Contains(NewMS.Mob.Id) Then
                NewMS.Item = AdditemMS(Client, MST.ItemId, MST.Tours)
                MobsSuiveurs.Add(NewMS)
            End If
            Client.Character.GetMap.RefreshCharacter(Client)
        End Sub

        Public Sub RemoveMobsuiveur(ByVal Client As GameClient, ByVal Mobid As Integer)
            Dim Mob As MobSuiveur = Nothing
            For Each MS As MobSuiveur In MobsSuiveurs
                If MS.Mob.Id = Mobid Then
                    Mob = MS
                End If
            Next
            If Not Mob Is Nothing Then MobsSuiveurs.Remove(Mob)
            Client.Send("OR" & Mob.Item.UniqueID)
            Refreshitems(Client)
        End Sub


        Public Sub init(ByVal S As String)
            Dim s1 As String() = S.Split(",")
            For i As Integer = 0 To s1.Length - 1
                Dim MST As MobSuiveurTemplate = MobsSuiveursManager.GetMSTByMonster(s1(i))
                Dim Monster As MonsterTemplate = MonstersHandler.GetTemplate(s1(i))
                Dim NewMS As New MobSuiveur
                NewMS.Skin = Monster.Skin
                NewMS.Taille = 100
                NewMS.Tours = MST.Tours
                NewMS.Mob = Monster
                MobsSuiveurs.Add(NewMS)
            Next
        End Sub

        Public Function GetString() As String

            Dim S As String = ""
            If MobsSuiveurs.Count > 6 Then
                S += ":"
                For Each MS As MobSuiveur In MobsSuiveurs
                    S += MS.GetString & ":"
                Next

                S = S.Substring(0, S.Length - 1)
            ElseIf MobsSuiveurs.Count > 0 Then
                S += ","
                For Each MS As MobSuiveur In MobsSuiveurs
                    S += MS.GetString & ","
                Next

                S = S.Substring(0, S.Length - 1)
            End If
            Return S
        End Function

        Public Function GetSaveString(ByVal Character As Character) As String
            Dim S As String = ""
            If MobsSuiveurs.Count > 0 Then
                For Each MS As MobSuiveur In MobsSuiveurs
                    S += MS.Mob.Id & ","
                Next
                S = S.Substring(0, S.Length - 1)
            End If
            Return S
        End Function


        Public Sub EnleveItems(ByVal Character As Character)
            Dim Client As GameClient = Players.GetCharacter(Character.ID)
            If Not Client Is Nothing Then

                For Each MS As MobSuiveur In MobsSuiveurs
                    If Not MS.Item Is Nothing Then
                        Client.Send("OR" & MS.Item.UniqueID)
                    End If
                Next
            Else

            End If

        End Sub

        Public Sub RemetItems(ByVal Character As Character)
            'Dim Client As GameClient = Players.GetCharacter(Character.ID)
            'If Not Client Is Nothing Then
            '    For Each MS As MobSuiveur In MobsSuiveurs
            '        MS.Item = AdditemMS(Client, MS.Item.TemplateID, MS.Tours)
            '    Next
            'End If
        End Sub

        Public Sub Metitems(ByVal Client As GameClient)

            'If Not Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR1) Is Nothing Then
            '    Client.Character.Items.DeleteItem(Client, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR1).UniqueID, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR1).Quantity)
            'End If
            'If Not Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR2) Is Nothing Then
            '    Client.Character.Items.DeleteItem(Client, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR2).UniqueID, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR2).Quantity)
            'End If
            'If Not Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR3) Is Nothing Then
            '    Client.Character.Items.DeleteItem(Client, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR3).UniqueID, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR3).Quantity)
            'End If
            'If Not Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR4) Is Nothing Then
            '    Client.Character.Items.DeleteItem(Client, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR4).UniqueID, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR4).Quantity)
            'End If
            'If Not Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR5) Is Nothing Then
            '    Client.Character.Items.DeleteItem(Client, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR5).UniqueID, Client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.BAR5).Quantity)
            'End If


            For Each MS As MobSuiveur In MobsSuiveurs
                Dim MST As MobSuiveurTemplate = MobsSuiveursManager.GetMSTByMonster(MS.Mob.Id)
                MS.Item = AdditemMS(Client, MST.ItemId, MS.Tours)
            Next

            Client.Character.SendAccountStats()
        End Sub

        Public Sub Refreshitems(ByVal Client As GameClient)

            For Each MS As MobSuiveur In MobsSuiveurs
                If Not MS.Item Is Nothing Then
                    Client.Send("OR" & MS.Item.UniqueID)
                End If
            Next
            For Each MS As MobSuiveur In MobsSuiveurs
                MS.Item = AdditemMS(Client, MS.Item.TemplateID, MS.Tours)
            Next
            Client.Character.SendAccountStats()
            Client.Character.GetMap.RefreshCharacter(Client)
        End Sub

        Public Sub Afterfight(ByVal Client As GameClient)
            Dim ListeASuppr As New List(Of MobSuiveur)
            For Each MS As MobSuiveur In MobsSuiveurs
                MS.Tours -= 1
                If MS.Tours < 1 Then
                    ListeASuppr.Add(MS)
                End If
            Next
            For Each Ms As MobSuiveur In ListeASuppr
                RemoveMobsuiveur(Client, Ms.Mob.Id)
                Client.SendNormalMessage("Vous n'êtes plus suivi par " & Ms.Mob.Nom)
            Next
            Refreshitems(Client)
        End Sub

    End Class
End Namespace