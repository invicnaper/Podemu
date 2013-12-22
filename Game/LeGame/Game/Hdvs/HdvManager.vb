Imports Podemu.Game
Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Imports System.Threading
Namespace Game

    Public Class HdvManager
        Public Shared ListHDV As New List(Of HDV)

        Public Shared Function GetHDVonmap(ByVal mapid As Integer) As HDV
            For Each HDV In ListHDV
                If HDV.Mapid = mapid Then
                    Return HDV
                End If
            Next
        End Function

#Region "SQL"

        Public Shared Sub LoadHDV()
            MyConsole.StartLoading("Loading HDVS from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM hdvs"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewHDV As New HDV
                    NewHDV.Categories = Result("categories")
                    NewHDV.Selltax = Result("sellTaxe")
                    NewHDV.Mapid = Result("map")
                    NewHDV.levelmax = Result("lvlMax")
                    If Not IsDBNull(Result("Itemlist")) Then
                        If Result("Itemlist") <> "" Then
                            Dim HDVitemdatas As String() = Result("Itemlist").ToString.Split("|")
                            For Each Hdvitemdata As String In HDVitemdatas
                                Dim data As String() = Hdvitemdata.Split(",")
                                Dim hdvitem As New ItemHDV
                                ' hdvitem.item = ItemsHandler.GetItemTemplate(data(0)).GenerateItem()
                                hdvitem.owner = data(1)
                                hdvitem.price = data(2)
                                NewHDV.Items.Add(hdvitem)
                            Next
                        End If

                    End If


                    ListHDV.Add(NewHDV)


                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListHDV.Count & "@' HDVs loaded from database")

        End Sub



        Public Shared Sub UpdateHDV(ByVal HDV As HDV)



            Dim ConMutex As New Threading.Mutex

            Try
                SyncLock Sql.Others
                    Dim CreateString As String = "itemlist=@itemlist where map=@map"

                    Dim SQLText As String = "UPDATE hdvs set " & CreateString & ""
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters
                    Dim Hdvitemsstring As String
                    For Each ItemHDV As ItemHDV In HDV.Items
                        Hdvitemsstring &= ItemHDV.item.TemplateID & "," & ItemHDV.owner & "," & ItemHDV.price & "|"
                    Next
                    P.Add(New MySqlParameter("@map", HDV.Mapid))
                    P.Add(New MySqlParameter("@itemlist", Hdvitemsstring.Substring(0, Hdvitemsstring.Length - 1)))


                    SQLCommand.ExecuteNonQuery()
                End SyncLock

            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try

        End Sub

        Public Shared Sub fullingHDV()
            For Each HDV As HDV In ListHDV

                HDV.Items = Getrandomitemhdv(HDV.Categories, HDV.levelmax)
                UpdateHDV(HDV)
            Next
        End Sub
        Public Shared Function Getrandomitemhdv(ByVal categorie As String, ByVal levelmax As Integer) As List(Of ItemHDV)
            Dim listofitemhdv As New List(Of ItemHDV)
            Dim types As String() = categorie.Split(",")
            While listofitemhdv.Count < 50
                For Each Type As String In types
                    For Each ItemTemplate As ItemTemplate In ItemsHandler.GetRandomeItemsTemplatebytype(Type, levelmax)
                        Dim item As New ItemHDV
                        item.item = ItemTemplate.GenerateItem()
                        item.owner = -1
                        item.price = Formulas.RandomepriceHDV(item.item)
                        listofitemhdv.Add(item)
                    Next
                Next
            End While

            Return listofitemhdv
        End Function

#End Region

        Public Shared Sub Open(ByVal client As GameClient, ByVal forvente As Boolean)
            client.Send("EV")
            Dim hdv As HDV = GetHDVonmap(client.Character.MapId)
            If forvente Then
                client.Character.State.isTradingHDVSOLD = True
                client.Send("ECK10|" & hdv.Selltax & ",10,100;" & hdv.Categories & ";1.0;" & hdv.levelmax & ";20;-1;0")
                Dim packet As String = "EL1;"
                For Each ItemHDV As ItemHDV In hdv.Items
                    If ItemHDV.owner = client.Character.ID Then

                        packet &= ItemHDV.item.UniqueID & ";" & ItemHDV.item.Quantity & ";" & ItemHDV.item.TemplateID & ";" & ItemHDV.item.EffectsInfos & ";" & ItemHDV.price & ";350|"

                    End If
                Next
                client.Send(packet.Substring(0, packet.Length - 1))
            Else
                client.Character.State.isTradingHDVBUY = True
                client.Send("ECK11|" & hdv.Selltax & ",10,100;" & hdv.Categories & ";1.0;" & hdv.levelmax & ";20;-1;0")
            End If
        End Sub

        Public Shared Sub Sendtemplateid(ByVal type As Integer, ByVal client As GameClient, ByVal vente As Boolean)
            Dim hdv As HDV = GetHDVonmap(client.Character.MapId)
            Dim templatelist As String = ""
            If vente Then
                For Each ItemHDV As ItemHDV In hdv.Items
                    If (ItemHDV.item.GetTemplate.Type = type And ItemHDV.owner = client.Character.ID) Then
                        templatelist &= ItemHDV.item.TemplateID & ";"
                    End If

                Next
                client.Send("EHL" & type & "|" & templatelist.Substring(0, templatelist.Length - 1))
            Else
                For Each ItemHDV As ItemHDV In hdv.Items
                    If (ItemHDV.item.GetTemplate.Type = type And Not templatelist.Contains(ItemHDV.item.TemplateID)) Then
                        templatelist &= ItemHDV.item.TemplateID & ";"
                    End If

                Next
                client.Send("EHL" & type & "|" & templatelist.Substring(0, templatelist.Length - 1))
            End If


        End Sub



        Public Shared Sub senditeminfos(ByVal itemtemplate As Integer, ByVal client As GameClient)
            Dim hdv As HDV = GetHDVonmap(client.Character.MapId)
            Dim iteminfoslist As String = ""
            Dim prixtotal As Integer
            Dim elementtotal As Integer
            For Each ItemHDV As ItemHDV In hdv.Items
                If ItemHDV.item.TemplateID = itemtemplate Then
                    elementtotal += 1
                    prixtotal += ItemHDV.price
                    If ItemHDV.item.Quantity = 1 Then
                        iteminfoslist &= ItemHDV.item.UniqueID & ";" & ItemHDV.item.EffectsInfos & ";" & ItemHDV.price & ";;;|"
                    ElseIf ItemHDV.item.Quantity = 10 Then
                        iteminfoslist &= ItemHDV.item.UniqueID & ";" & ItemHDV.item.EffectsInfos & ";;" & ItemHDV.price & ";;|"
                    ElseIf ItemHDV.item.Quantity = 100 Then
                        iteminfoslist &= ItemHDV.item.UniqueID & ";" & ItemHDV.item.EffectsInfos & ";;;" & ItemHDV.price & ";|"
                    End If
                End If
            Next
            client.Send("EHP" & itemtemplate & "|" & CInt(prixtotal / elementtotal))
            client.Send("EHl" & itemtemplate & "|" & iteminfoslist.Substring(0, iteminfoslist.Length - 1))
        End Sub
        Public Shared Sub Buy(ByVal itemID As Integer, ByVal price As Integer, ByVal client As GameClient)
            If client.Character.Player.Kamas >= price Then
                Dim hdv As HDV = GetHDVonmap(client.Character.MapId)
                Dim newitem As Game.Item
                Dim olditem As Game.ItemHDV
                For Each itemhdv As ItemHDV In hdv.Items
                    If itemhdv.item.UniqueID = itemID Then
                        olditem = itemhdv
                    End If
                Next
                newitem = ItemsHandler.GetItemTemplate(olditem.item.TemplateID).GenerateItem()
                ' newitem.EffectList = olditem.item.EffectList
                client.Character.Items.AddItem(client, newitem)
                If Not olditem.owner = -1 Then
                    Dim owner = CharactersManager.GetCharacter(olditem.owner)
                    owner.Player.Kamas += price
                    owner.Save()
                End If
                hdv.Items.Remove(olditem)
                client.Character.Player.Kamas -= price
                client.Character.SendAccountStats(client)
                client.Character.Items.RefreshItems(client)
                client.Send("EHm-1")
                client.Send("Im068")
                client.Send("EV")

                client.Character.Save()
                UpdateHDV(hdv)
                Open(client, False)

            End If

        End Sub


        Public Shared Sub additem(ByVal itemid As Integer, ByVal quantity As Integer, ByVal price As Integer, ByVal client As GameClient, ByVal add As Boolean)
            Dim hdv As HDV = GetHDVonmap(client.Character.MapId)
            If add Then
                Dim Item As Item = client.Character.Items.GetItemFromID(itemid)
                If client.Character.Player.Kamas >= price * (hdv.Selltax / 100) Then
                    client.Send("EmK-" & Item.UniqueID)
                    Dim newitem As New Game.ItemHDV
                    newitem.item = ItemsHandler.GetItemTemplate(Item.TemplateID).GenerateItem
                    'newitem.item.EffectList = Item.EffectList
                    newitem.price = price
                    newitem.owner = client.Character.ID
                    client.Character.Items.RemoveItem(client, Item, 1)
                    hdv.Items.Add(newitem)
                    HdvManager.UpdateHDV(hdv)
                    client.Character.Items.RefreshItems(client)
                    client.Send("EV")
                    Open(client, True)
                Else
                    client.SendMessage("Vous n'avez pas assez de kamas")
                End If

            Else
                Dim newitem As Item
                Dim itemhdvtodel As Game.ItemHDV
                For Each Itemhdv As ItemHDV In hdv.Items
                    If Itemhdv.item.UniqueID = itemid Then
                        itemhdvtodel = Itemhdv
                    End If
                Next
                '  newitem = itemhdvtodel.item.GetTemplate.GenerateItem
                ' newitem.EffectList = itemhdvtodel.item.EffectList
                hdv.Items.Remove(itemhdvtodel)
                client.Character.Items.AddItem(client, newitem)
                HdvManager.UpdateHDV(hdv)
                client.Character.Items.RefreshItems(client)
                client.Send("EV")
                Open(client, True)
            End If

        End Sub
    End Class
End Namespace
