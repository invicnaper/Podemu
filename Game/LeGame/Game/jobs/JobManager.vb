Imports Podemu.Utils
Imports System.Data
Imports MySql.Data.MySqlClient


Namespace Game
    Public Class JobManager
        Private Shared ListofJobs As New List(Of Job)
        Private Shared ListofRecipies As New List(Of CraftIngredient)
#Region "SQL"

        Public Shared Sub LoadJobs()
            Try
                MyConsole.StartLoading("Loading Jobs from database...")
                SyncLock Sql.OthersSync

                    Dim SQLText As String = "SELECT * FROM jobs_data"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewJob As New Job
                        NewJob.id = Result("id")
                        If Not Result("tools") = "" Then NewJob.tool = Result("tools")
                        For Each Split As String In Result("crafts").ToString.Split("|")
                            Dim craft As New Crafts
                            Dim type = Split.Split(";")
                            If type(0) <> "" Then
                                craft.type = type(0)

                                For Each itemID As String In Split.Substring(type(0).Length + 1).Split(",")

                                    craft.Listing.Add(itemID)
                                Next
                                NewJob.Crafts.Add(craft)
                            End If
                        Next

                        ListofJobs.Add(NewJob)

                    End While

                    Result.Close()

                End SyncLock

                MyConsole.StopLoading()
                MyConsole.Status("'@" & ListofJobs.Count & "@' Jobs loaded from database")
            Catch ex As Exception

            End Try

        End Sub



        Public Shared Sub LoadRecipies()
            Try
                MyConsole.StartLoading("Loading CraftsRecipies from database...")
                SyncLock Sql.OthersSync

                    Dim SQLText As String = "SELECT * FROM crafts"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read
                        Dim craft As New CraftIngredient
                        For Each Split As String In Result("craft").ToString.Split(";")
                            Dim data As String() = Split.Split("*")
                            If ItemsHandler.ItemExist(data(0)) Then
                                Dim Newingredient As Item = ItemsHandler.GetItemTemplate(data(0)).GenerateItem

                                Newingredient.Quantity = data(1)
                                craft.Ingredients.Add(Newingredient)
                            End If
                        Next
                        craft.ItemID = Result("id")

                        ListofRecipies.Add(craft)


                    End While

                    Result.Close()

                End SyncLock

                MyConsole.StopLoading()
                MyConsole.Status("'@" & ListofJobs.Count & "@' crafts loaded from database")
            Catch
            End Try

        End Sub

        Public Shared Function GetJob(ByVal tool As Integer) As Job
            For Each jobs As Job In ListofJobs
                If Not jobs.tool = "" Then
                    For Each tools As String In jobs.tool.Split(",")
                        If tools = tool Then
                            Return jobs
                        End If
                    Next
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function Istool(ByVal tool As Integer) As Boolean
            For Each jobs As Job In ListofJobs
                If Not jobs.tool = "" Then
                    For Each tools As String In jobs.tool.Split(",")
                        If tools = tool Then
                            Return True
                        End If
                    Next
                End If
            Next
            Return False
        End Function


        Public Shared Sub SendPlayerjob(ByVal client As GameClient)

            If client.Character.Items.IsObjectOnPos(1) Then 'objet metier
                If Istool(client.Character.Items.GetObjectOnPos(1).TemplateID) Then
                    Dim job As Job = GetJob(client.Character.Items.GetObjectOnPos(1).TemplateID)
                    Dim packet As String = "JS|"
                    Dim types As String() = GetJobtype(job.id).Split(",")
                    Dim count As Integer = 0
                    For Each Type As String In types
                        If count = 0 Then
                            packet &= job.id & ";" & Type & "~8~0~0~100,"
                            count = 1
                        Else
                            packet &= Type & "~8~0~0~100,"
                        End If
                    Next
                    client.Send(packet.Substring(0, packet.Length - 1))
                    client.Send("JX|" & job.id & ";100;0;0;50;")
                    client.Send("JO4|0|8")
                    client.Send("OT" & job.id)

                End If

            End If

        End Sub


        Public Shared Sub SendActionJobRecolte(ByVal cellid As String, ByVal client As GameClient)
            Dim CraftType As Integer = client.Character.State.CurrentGameAction
            client.Character.State.IsFauching = True
            client.Character.CurrentCraft = GetCrafts(CraftType)
            client.Send("GA" & client.Character.MapDir & ";501;" & client.Character.ID & ";" & cellid & ",5000")
            client.Send("GDF|" & cellid & ";2;0")
        End Sub

        Public Shared Sub EndActionJobRecolte(ByVal cellid As Integer, ByVal client As GameClient)
            If cellid = client.Character.MapCell Then
                Dim Ressourcerecolted As New Item
                For Each Itemtemplate As Integer In client.Character.CurrentCraft.Listing
                    Ressourcerecolted = ItemsHandler.GetItemTemplate(Itemtemplate).GenerateItem
                Next
                Ressourcerecolted.Quantity = CInt(Rnd() * 15 + 1) + 1
                client.Send("GDF|" & cellid & ";4;0")
                client.Send("IQ1|" & Ressourcerecolted.Quantity)
                client.Send("OQ" & Ressourcerecolted.UniqueID & "|" & Ressourcerecolted.Quantity)
                client.Character.Items.AddItem(client, Ressourcerecolted)
                client.Character.Items.RefreshItems(client)
                client.Character.State.IsFauching = False
            End If
        End Sub

        Public Shared Function GetJobtype(ByVal id As Integer) As String
            Dim types As String = ""
            For Each Job As Job In ListofJobs
                If Job.id = id Then
                    For Each Crafts As Crafts In Job.Crafts
                        types &= Crafts.type & ","
                    Next
                    Return types.Substring(0, types.Length - 1)
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function GetCrafts(ByVal type As Integer) As Crafts
            For Each jobs In ListofJobs
                For Each Crafts In jobs.Crafts

                    If Crafts.type = type Then
                        Return Crafts
                    End If

                Next
            Next
            Return Nothing
        End Function


        Public Shared Function GetRecoltsCraftslist() As List(Of Crafts)
            Dim RecoltsCraftsList As New List(Of Crafts)
            For Each jobs In ListofJobs

                For Each Crafts As Crafts In jobs.Crafts
                    If Crafts.Listing.Count < 2 Then
                        RecoltsCraftsList.Add(Crafts)
                    End If
                Next
            Next
            Return RecoltsCraftsList
        End Function


        Public Shared Function IsCorrectRecipie(ByVal ingredient As Dictionary(Of Item, Integer), ByVal itemid As Integer) As Boolean
            Dim requieredrecipies = ListofRecipies.Where(Function(e) e.ItemID = itemid).First.Ingredients
            If Not requieredrecipies.Count = ingredient.Count Then Return False
            For Each testingingredient In ingredient
                Dim testingredient = testingingredient
                If requieredrecipies.FindAll(Function(e) e.TemplateID = testingredient.Key.TemplateID And e.Quantity = testingredient.Value).Count = 0 Then Return False
            Next
            Return True
        End Function





        Public Shared ReadOnly Property JobsType(ByVal client As GameClient, ByVal currentAction As Integer) As TypeJobs
            Get
                If client.Character.State.IsFM Then
                    If currentAction = 181 Then Return TypeJobs.Concasseur
                    If (currentAction > 112 And currentAction < 121) Or (currentAction > 162 And currentAction < 170) Then Return TypeJobs.FM Else Return TypeJobs.Crafts
                End If
                If client.Character.State.IsFauching Then Return TypeJobs.Recolt
            End Get
        End Property

        Public Enum TypeJobs
            Crafts
            Recolt
            FM
            Concasseur
        End Enum




#End Region

    End Class
End Namespace