Imports Podemu.Utils
Namespace Game
    Public Class StoreItem

        Public Enum StoreJet

            Max = 1
            Min = 2
            Middle = 3
            None = 0

        End Enum

        Public StoreId As Integer = 0
        Public ItemId As Integer = 0
        Public Jet As StoreJet = StoreJet.None
        Public Cost As Integer = 0

        Private Function Create() As Item
            Return ItemsHandler.GetItemTemplate(ItemId).GenerateItem(Jet)
        End Function

        Public Sub AddToCharacter(ByVal Client As GameClient)

            Client.SendMessage("Tu viens d'acheter un(e) " & ItemsHandler.GetItemTemplate(ItemId).Name & " pour " & Cost & " " & Config.GetItem("POINTS_NAME") & " !")
            Client.Character.Items.AddItem(Client, Create())

        End Sub

    End Class
End Namespace