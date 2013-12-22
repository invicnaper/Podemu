Imports MySql.Data.MySqlClient
Imports Podemu.Utils

Namespace Game
    Public Class GiftsHandler

        Private Shared GiftTemplates As New Dictionary(Of Integer, GiftTemplate)

        Public Shared Function GiftExist(ByVal ID As Integer) As Boolean
            Return GiftTemplates.ContainsKey(ID)
        End Function

        Public Shared Function GetTemplate(ByVal ID As Integer) As GiftTemplate
            Return GiftTemplates(ID)
        End Function

        Public Shared Sub LoadGifts()

            Dim SQLText As String = "SELECT * FROM gift_templates"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewTemplate As New GiftTemplate

                NewTemplate.ID = Result.GetInt32("id")
                NewTemplate.Name = Result.GetString("name")
                NewTemplate.Description = Result.GetString("description")
                NewTemplate.GfxUrl = Result.GetString("img_url")
                NewTemplate.Max = Result.GetBoolean("max")

                For Each Item As String In Result.GetString("items").Split(";")

                    If Not ItemsHandler.ItemExist(CInt(Item)) Then Continue For

                    Dim template As ItemTemplate = ItemsHandler.GetItemTemplate(CInt(Item))

                    NewTemplate.Items.Add(template.GenerateItem(1, If(NewTemplate.Max, 1, 0)))
                Next

                GiftTemplates.Add(NewTemplate.ID, NewTemplate)

            End While

            Result.Close()

        End Sub

      
    End Class
End Namespace