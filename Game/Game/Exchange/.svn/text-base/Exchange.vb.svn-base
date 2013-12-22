Namespace Game
    Public Class Exchange

        Public Player1 As Exchanger
 
        Public Player2 As Exchanger



        Public Sub New(ByVal Player1 As GameClient, ByVal Player2 As GameClient)

            Me.Player1 = New Exchanger(Player1, Me)
            Me.Player2 = New Exchanger(Player2, Me)

            Player1.Send("ERK" & Player1.Character.ID & "|" & Player2.Character.ID & "|1")
            Player2.Send("ERK" & Player1.Character.ID & "|" & Player2.Character.ID & "|1")

        End Sub

        Public Sub Begin()

            Player1.Begin()
            Player2.Begin()

        End Sub

        Public Sub Leave()

            Player1.Leave()
            Player2.Leave()

        End Sub

        Public Sub Finish()
            Player1.Finish()
            Player2.Finish()
        End Sub

        Public Sub Validate(ByVal Client As GameClient)

            If Client Is Player1.Client Then

                Player1.HasValidate = True

                Player1.Validate()
                Player2.Validate(Player1.Client.Character.ID)

                If Player2.HasValidate Then

                    DoActions()

                    Finish()

                    Leave()

                End If

            ElseIf Client Is Player2.Client Then

                Player2.HasValidate = True

                Player1.Validate(Player2.Client.Character.ID)
                Player2.Validate()

                If Player1.HasValidate Then

                    DoActions()

                    Finish()

                    Leave()

                End If

            End If
        End Sub

        Private Sub Unvalidate()

            Player1.Unvalidate()
            Player2.Unvalidate()

            Player1.Unvalidate(Player2.Client.Character.ID)
            Player2.Unvalidate(Player1.Client.Character.ID)

        End Sub

        Public Sub MoveKamas(ByVal Client As GameClient, ByVal Kamas As Long)

            If Kamas > 0 AndAlso Client.Character.Player.Kamas >= Kamas Then

                If Client Is Player1 Then
                    Player1.Kamas = Kamas


                    Unvalidate()

                    Player1.Client.Send("EmKG" & Kamas)
                    Player2.Client.Send("EMKG" & Kamas)

                ElseIf Client Is Player2 Then
                    Player2.Kamas = Kamas


                    Unvalidate()

                    Player1.Client.Send("EMKG" & Kamas)
                    Player2.Client.Send("EmKG" & Kamas)

                End If

            Else
                Client.Send("EME")
            End If

        End Sub

        Public Sub AddItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

            Dim Item As Item = Client.Character.Items.GetItemFromID(ItemId)

            If Item Is Nothing OrElse Item.Quantity < Quantity Then
                Client.Send("EME")
                Exit Sub
            End If

            If Client Is Player1.Client Then

                Dim AlreadyItem As ExchangeItem = Player1.Items.FirstOrDefault(Function(i) i.MyItem.IsSuperposable(Item))
                If AlreadyItem Is Nothing Then
                    AlreadyItem = New ExchangeItem(Item, Quantity)
                    Player1.Items.Add(AlreadyItem)
                Else
                    AlreadyItem.Quantity += Quantity
                    If AlreadyItem.Quantity > Item.Quantity Then AlreadyItem.Quantity = Item.Quantity
                End If

                Unvalidate()
                Player1.Client.Send("EMKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity)
                Player2.Client.Send("EmKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity & "|" & Item.GetTemplate.ID & "|" & Item.EffectsInfos)

            ElseIf Client Is Player2.Client Then

                Dim AlreadyItem As ExchangeItem = Player2.Items.FirstOrDefault(Function(i) i.MyItem.IsSuperposable(Item))
                If AlreadyItem Is Nothing Then
                    AlreadyItem = New ExchangeItem(Item, Quantity)
                    Player2.Items.Add(AlreadyItem)
                Else
                    AlreadyItem.Quantity += Quantity
                    If AlreadyItem.Quantity > Item.Quantity Then AlreadyItem.Quantity = Item.Quantity
                End If

                Unvalidate()
                Player1.Client.Send("EmKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity & "|" & Item.GetTemplate.ID & "|" & Item.EffectsInfos)
                Player2.Client.Send("EMKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity)

            End If

        End Sub

        Public Sub DelItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)

            Dim Item As Item = Client.Character.Items.GetItemFromID(ItemId)

            If Client Is Player1.Client Then
                Dim AlreadyItem As ExchangeItem = Player1.Items.FirstOrDefault(Function(i) i.MyItem.UniqueID = ItemId)
                If Not AlreadyItem Is Nothing Then
                    AlreadyItem.Quantity -= Quantity
                    Unvalidate()
                    If AlreadyItem.Quantity <= 0 Then
                        Player1.Items.Remove(AlreadyItem)
                        Player1.Client.Send("EMKO-" & Item.UniqueID)
                        Player2.Client.Send("EmKO-" & Item.UniqueID)
                    Else
                        Player1.Client.Send("EMKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity)
                        Player2.Client.Send("EmKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity & "|" & Item.GetTemplate.ID & "|" & Item.EffectsInfos)
                    End If
                End If

            ElseIf Client Is Player2.Client Then

                Dim AlreadyItem As ExchangeItem = Player2.Items.FirstOrDefault(Function(i) i.MyItem.UniqueID = ItemId)
                If Not AlreadyItem Is Nothing Then
                    AlreadyItem.Quantity -= Quantity
                    Unvalidate()
                    If AlreadyItem.Quantity <= 0 Then
                        Player2.Items.Remove(AlreadyItem)
                        Player2.Client.Send("EMKO-" & Item.UniqueID)
                        Player1.Client.Send("EmKO-" & Item.UniqueID)
                    Else
                        Player2.Client.Send("EMKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity)
                        Player1.Client.Send("EmKO+" & Item.UniqueID & "|" & AlreadyItem.Quantity & "|" & Item.GetTemplate.ID & "|" & Item.EffectsInfos)
                    End If
                End If

            End If

        End Sub

        Private Sub DoActions()

            Player1.Client.Character.Player.Kamas -= Player1.Kamas
            Player2.Client.Character.Player.Kamas -= Player2.Kamas
            Player1.Client.Character.Player.Kamas += Player2.Kamas
            Player2.Client.Character.Player.Kamas += Player1.Kamas

            Dim UniqueIds As New List(Of Integer)
            For Each Item As ExchangeItem In Player1.Items.ToArray
                If UniqueIds.Contains(Item.MyItem.UniqueID) Then
                    Player1.Items.Remove(Item)
                Else
                    UniqueIds.Add(Item.MyItem.UniqueID)
                End If
            Next

            For Each Item As ExchangeItem In Player1.Items
                If Item.Quantity > Item.MyItem.Quantity Then Item.Quantity = Item.MyItem.Quantity
                Player1.Client.Character.Items.DeleteItem(Player1.Client, Item.MyItem.UniqueID, Item.Quantity)
                Player2.Client.Character.Items.AddItem(Player2.Client, Item.Create)
            Next

            UniqueIds.Clear()
            For Each Item As ExchangeItem In Player2.Items.ToArray
                If UniqueIds.Contains(Item.MyItem.UniqueID) Then
                    Player2.Items.Remove(Item)
                Else
                    UniqueIds.Add(Item.MyItem.UniqueID)
                End If
            Next

            For Each Item As ExchangeItem In Player2.Items
                If Item.Quantity > Item.MyItem.Quantity Then Item.Quantity = Item.MyItem.Quantity
                Player2.Client.Character.Items.DeleteItem(Player2.Client, Item.MyItem.UniqueID, Item.Quantity)
                Player1.Client.Character.Items.AddItem(Player1.Client, Item.Create)
            Next

            Player1.Client.Character.SendAccountStats()
            Player2.Client.Character.SendAccountStats()

        End Sub





    End Class
End Namespace