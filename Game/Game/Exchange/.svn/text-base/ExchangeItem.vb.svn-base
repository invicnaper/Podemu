Namespace Game
    Public Class ExchangeItem

        Public MyItem As Item
        Public Quantity As Integer

        Public Sub New(ByVal Item As Item, ByVal Quantity As Integer)
            MyItem = Item
            Me.Quantity = Quantity
        End Sub

        Public Function Create() As Item

            Dim NewItem As Item = MyItem.Copy
            NewItem.Quantity = Quantity
            MyItem.Quantity -= Quantity
            Return NewItem

        End Function

    End Class
End Namespace