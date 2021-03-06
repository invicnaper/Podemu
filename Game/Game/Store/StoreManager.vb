﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class StoreManager

        Private Shared ItemList As New List(Of StoreItem)

        Public Shared Sub LoadStore()

            MyConsole.StartLoading("Loading store's items from database...")

            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM boutique"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewItem As New StoreItem

                    NewItem.StoreId = Result("id")
                    NewItem.ItemId = Result("itemId")
                    NewItem.Jet = Result("jet")
                    NewItem.Cost = Result("cost")

                    ItemList.Add(NewItem)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ItemList.Count & "@' store's items loaded from database")

        End Sub

        Public Shared Sub BuyItem(ByVal Client As GameClient, ByVal StoreId As Integer)

            For Each Item As StoreItem In ItemList
                If Item.StoreId = StoreId Then

                    If Client.Infos.Points >= Item.Cost Then

                        Client.Infos.Points -= Item.Cost
                        Item.AddToCharacter(Client)
                        Client.Character.Save()

                    Else
                        Dim Manquant As Integer = Item.Cost - Client.Infos.Points
                        Client.SendMessage("Il vous manque <b>" & Manquant & "</b> points. " & _
                                "<a href=""http://www.navosoft.net/Groufs "">(en acheter des points)</a>" & " visiter : " & Config.GetItem("LINK_STOR") & " Pour achter vos points")
                        Client.SendMessage("N'oubliez pas de vous reconnecter après un achat points !")
                    End If

                    Exit Sub
                End If
            Next

            Client.SendMessage("Ce numéro ne correspond à aucun objet de la boutique !" & " visiter : " & Config.GetItem("LINK_STOR") & " Pour les ID de boutique")



        End Sub

    End Class
End Namespace