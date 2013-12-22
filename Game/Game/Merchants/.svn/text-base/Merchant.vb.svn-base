
Namespace Game
    Public Class Merchant : Inherits Character

        Public ListOfTradder As New List(Of GameClient)

        Public Sub BeginTrade(ByVal Client As GameClient)
            ListOfTradder.Add(Client)
            Client.Character.State.BeginTrade(Trading.Merchant, Me)
            Client.Send("ECK4|" & ID)
            MerchantBag.SendItemsList(Client)
        End Sub

        Public Sub EndTrade(ByVal Client As GameClient)
            ListOfTradder.Remove(Client)
            Client.Character.State.EndTrade()
            Client.Send("EV")
        End Sub

        Public Sub EndTradeWithAll()
            For Each Client As GameClient In ListOfTradder
                Client.Character.State.EndTrade()
                Client.Send("EV")
            Next
        End Sub

        Public Sub RefreshItemsList()
            For Each Client As GameClient In ListOfTradder
                MerchantBag.SendItemsList(Client)
            Next
        End Sub

        Public Sub Buy(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

            Dim Item = MerchantBag.GetItem(ItemId)

            If Item IsNot Nothing Then

                Dim Price As Integer = Item.Price * Quantity

                If Client.Character.Player.Kamas >= Price And Item.Quantity >= Quantity Then

                    Dim NewItem As Item = Item.Copy()
                    NewItem.Quantity = Quantity
                    NewItem.Position = -1
                    MerchantBag.RemoveItem(Item, Quantity)

                    Client.Character.Player.Kamas -= Price
                    Player.Kamas += Price
                    Client.Character.Items.AddItem(Client, NewItem)
                    Client.Send("EBK")

                    If MerchantBag.Count = 0 Then
                        Dim Map = GetMap
                        EndTradeWithAll()
                        Map.RemoveEntity(Me.ID)
                        Map.MerchantList.Remove(Me)
                        CharactersManager.CharactersList(Me.Name) = CharactersManager.ToCharacter(Me)
                        Me.State.IsEmptyMerchant = True
                        Exit Sub
                    End If

                    RefreshItemsList()

                Else
                    Client.Send("OBE")
                End If
            Else
                Client.Send("OBE")
            End If

        End Sub

    End Class
End Namespace