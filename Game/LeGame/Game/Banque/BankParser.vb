Imports Podemu.Utils
Imports Podemu.Game

Public Class BankParser

    Public Shared ListOfItems As New List(Of Item)
    Public Kamas As Integer


    Public Shared Sub BeginTrade(ByVal Client As GameClient)

        'Client.Character.State.IsInBank = True
        Client.Send("ECK5|")
        BankHandler.LoadBank(Client)
        BankHandler.LoadBankKamas(Client)
        SendContent(Client)

    End Sub

    Public Sub EndTrade(ByVal Client As GameClient)
        Client.Character.State.EndTrade()
        Client.Character.State.IsInBank = False
        Client.Send("EV")
    End Sub

    Public Shared Function GetItemsString() As String
        Return String.Join(";", ListOfItems.Select(Function(i) "O" & i.ToString()))
    End Function

    Public Shared Function StringItems() As String
        Return String.Join(";", ListOfItems)
    End Function

    Public Shared Sub SendContent(ByVal Client As GameClient)

        Client.Send("EL" & GetItemsString() & ";G" & BankHandler.LoadBankKamas(Client))

    End Sub

    Public Shared Sub AddKamasInBanque(ByVal Client As GameClient, ByVal Kamas As Integer)

        If (Kamas < 0 AndAlso BankHandler.LoadBankKamas(Client) < Kamas) OrElse (Kamas > 0 AndAlso Client.Character.Player.Kamas < Kamas) Then
            Client.Send("EsE")
        End If

        BankHandler.AddBanqueKamas(Kamas, Client)
        Client.Character.Player.Kamas -= Kamas
        Client.Character.SendAccountStats()
        Client.Send("EsKG", BankHandler.LoadBankKamas(Client))
    End Sub

    Public Shared Sub SuprKamasInBanque(ByVal Client As GameClient, ByVal Kamas As Integer)

        If (Kamas < 0 AndAlso BankHandler.LoadBankKamas(Client) < Kamas) OrElse (Kamas > 0 AndAlso Client.Character.Player.Kamas < Kamas) Then
            Client.Send("EsE")
        End If

        BankHandler.SuprBanqueKamas(Kamas, Client)
        Dim KamasSplit As String = Kamas.ToString.Remove(0, 1)
        Client.Character.Player.Kamas += KamasSplit
        Client.Character.SendAccountStats()
        Client.Send("EsKG", BankHandler.LoadBankKamas(Client))
    End Sub

    Public Shared Sub ExchangeMoveItem(ByVal ExtraData As String, ByVal Client As GameClient)

        Dim Add As Boolean = (ExtraData.Substring(0, 1) = "+")
        Dim Data() As String = ExtraData.Substring(1).Split("|")
        Dim ItemId As Integer = Data(0)
        Dim Quantity As Integer = Data(1)
        Dim Price As Integer = 0
        If Data.Length > 2 Then
            Price = Data(2)
        End If
        If Add Then
            AddItem(Client, ItemId, Quantity)
        Else
            RemoveItem(Client, ItemId, Quantity)
        End If
    End Sub

    Public Shared Sub AddItem(ByVal client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

        Dim Item = client.Character.Items.GetItemFromID(ItemId)

        If Item IsNot Nothing AndAlso Quantity <= Item.Quantity Then

            client.Character.Items.RemoveItem(client, Item, Quantity)

            For Each AlreadyItem As Item In ListOfItems
                If AlreadyItem.IsSuperposable(Item) Then
                    AlreadyItem.Quantity += Quantity
                    client.Send("EsKO+", ItemString(AlreadyItem))
                    StringItems()
                    BankHandler.SendListOfItems(client, ListOfItems)
                    Exit Sub
                End If
            Next

            Dim NewItem = Item.Copy()
            NewItem.Quantity = Quantity
            ListOfItems.Add(NewItem)
            client.Send("EsKO+", ItemString(NewItem))
            StringItems()
            BankHandler.SendListOfItems(client, ListOfItems)
            Else
                client.Send("EsE")
            End If

    End Sub

    Public Shared Sub RemoveItem(ByVal client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

        Dim Item = GetItem(ItemId)

        If Item IsNot Nothing AndAlso Item.Quantity >= Quantity Then

            Dim Pods = Item.GetTemplate.Pods * Quantity

            If client.Character.Items.Pods + Pods > client.Character.Player.MaxPods Then
                client.SendMessage("Pas assez de place")
                Exit Sub
            End If

            If Item.Quantity = Quantity Then

                ListOfItems.Remove(Item)
                client.Send("EsKO-", ItemString(Item))

                client.Character.Items.AddItem(client, Item)

            Else

                Item.Quantity -= Quantity
                client.Send("EsKO+", ItemString(Item))

                Dim NewItem = Item.Copy()
                NewItem.Quantity = Quantity
                client.Character.Items.AddItem(client, NewItem)

            End If

            StringItems()
            BankHandler.SendListOfItems(client, ListOfItems)

        Else
            client.Send("EsE")
        End If

    End Sub

    'Id, uniqueId, quantity, position, effet, Price
    Public Shared Function ItemString(ByVal Item As Item) As String

        Dim ePosition As String = If(Item.Position = -1, "", Basic.DeciToHex(Item.Position))

        Return String.Concat(Item.UniqueID, "|", Basic.DeciToHex(Item.Quantity), "|", _
        Item.TemplateID, "|", Item.EffectsInfos())

    End Function

    Public Shared Function GetItem(ByVal Id As Integer) As Item
        Return ListOfItems.FirstOrDefault(Function(i) i.UniqueID = Id)
    End Function

    Private Sub Merge(ByVal Items As List(Of Item))

        Dim ToRemove As New Dictionary(Of Integer, Item)

        For Each iItem As Item In Items

            If Not ToRemove.ContainsKey(iItem.UniqueID) Then
                For Each OtherItem In Items.Where(Function(i) i.UniqueID <> iItem.UniqueID)

                    If Not ToRemove.ContainsKey(iItem.UniqueID) AndAlso iItem.IsSuperposable(OtherItem) Then
                        iItem.Quantity += OtherItem.Quantity
                        ToRemove.Add(OtherItem.UniqueID, OtherItem)
                    End If

                Next
            End If
        Next

        For Each Item In ToRemove
            Items.Remove(Item.Value)
        Next

    End Sub

End Class
