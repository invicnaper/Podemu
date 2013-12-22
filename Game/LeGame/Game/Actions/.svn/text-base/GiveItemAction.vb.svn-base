Namespace Game.Actions
    Public Class GiveItemAction : Inherits BaseAction


        Sub New()
            MyBase.New("itemid")
        End Sub

        Public Overrides Sub Process(ByVal User As GameClient, ByVal TargetCell As String, ByVal Params As System.Collections.Generic.Dictionary(Of String, String))

            Dim ItemTemplate = ItemsHandler.GetItemTemplate(Params("itemid"))

            Dim Quantity = If(Params.ContainsKey("quantity"), CInt(Params("quantity")), 1)

            Dim Item = ItemTemplate.GenerateItem(Quantity)

            User.Character.Items.AddItem(User, Item)

            If Params.ContainsKey("mess") Then
                User.SendNormalMessage(Params("mess"))
            End If

        End Sub



    End Class
End Namespace