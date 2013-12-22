Imports Podemu.Utils

Namespace Game.Storages
    Public Class Storage

        Public ListOfItems As New List(Of Item)
        Public Kamas As Integer
        Public IsTrading As Boolean


        Public Sub BeginTrade(ByVal Client As GameClient)

            If Not IsTrading Then

                IsTrading = True
                Client.Character.State.BeginTrade(Trading.Storage, Me)
                Client.Send("ECK5|")
                SendContent(Client)
            Else
                Client.SendMessage("Emplacement actuellement utilisé")
            End If

        End Sub

        Public Sub EndTrade(ByVal Client As GameClient)
            Client.Character.State.EndTrade()
            Client.Send("EV")
            IsTrading = False
        End Sub

        Public Function GetItemsString() As String
            Return String.Join(";", ListOfItems.Select(Function(i) "O" & i.ToString()))
        End Function

        Public Sub SendContent(ByVal Client As GameClient)

            Client.Send("EL" & GetItemsString() & ";G" & Kamas)

        End Sub

        Public Sub SetKamas(ByVal client As GameClient, ByVal Kamas As Integer)

            If (Kamas < 0 AndAlso Me.Kamas < Kamas) OrElse (Kamas > 0 AndAlso client.Character.Player.Kamas < Kamas) Then
                client.Send("EsE")
            End If

            Me.Kamas += Kamas
            client.Character.Player.Kamas -= Kamas
            client.Character.SendAccountStats()
            client.Send("EsKG", Me.Kamas)

        End Sub

        Public Sub AddItem(ByVal client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

            Dim Item = client.Character.Items.GetItemFromID(ItemId)

            If Item IsNot Nothing AndAlso Quantity <= Item.Quantity Then

                client.Character.Items.RemoveItem(client, Item, Quantity)

                For Each AlreadyItem As Item In ListOfItems
                    If AlreadyItem.IsSuperposable(Item) Then
                        AlreadyItem.Quantity += Quantity
                        client.Send("EsKO+", ItemString(AlreadyItem))
                        Exit Sub
                    End If
                Next

                Dim NewItem = Item.Copy()
                NewItem.Quantity = Quantity
                ListOfItems.Add(NewItem)
                client.Send("EsKO+", ItemString(NewItem))
            Else
                client.Send("EsE")
            End If

        End Sub

        Public Sub RemoveItem(ByVal client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

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

            Else
                client.Send("EsE")
            End If

        End Sub

        'Id, uniqueId, quantity, position, effet, Price
        Public Function ItemString(ByVal Item As Item) As String

            Dim ePosition As String = If(Item.Position = -1, "", Basic.DeciToHex(Item.Position))

            Return String.Concat(Item.UniqueID, "|", Basic.DeciToHex(Item.Quantity), "|", _
            Item.TemplateID, "|", Item.EffectsInfos())

        End Function

        Private Function GetItem(ByVal Id As Integer) As Item
            Return ListOfItems.FirstOrDefault(Function(i) i.UniqueID = Id)
        End Function

        Private Sub Merge(ByVal Items As List(Of Item))

            Dim ToRemove As New Dictionary(Of Integer, Item)

            For Each Item In Items

                If Not ToRemove.ContainsKey(Item.UniqueID) Then
                    For Each OtherItem In Items.Where(Function(i) i.UniqueID <> Item.UniqueID)

                        If Not ToRemove.ContainsKey(Item.UniqueID) AndAlso Item.IsSuperposable(OtherItem) Then
                            Item.Quantity += OtherItem.Quantity
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
End Namespace