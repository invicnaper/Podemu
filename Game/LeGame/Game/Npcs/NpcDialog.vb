Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Namespace Game
    Public Class NpcDialog

        Private Dialog As QuestionNPC
        Private Response As ResponseNPC

        Public Shared ListOfQuestion As New List(Of QuestionNPC)
        Public Shared ListOfResponse As New List(Of ResponseNPC)

        Public Shared Sub LoadQuestion()

            MyConsole.StartLoading("Loading question from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM npc_question"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewQuestion As New QuestionNPC

                    NewQuestion.NpcID = Result("npcid")
                    NewQuestion.QuestionID = Result("questionid")
                    NewQuestion.ResponsesPossible = Result("responses")

                    ListOfQuestion.Add(NewQuestion)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfQuestion.Count & "@' question loaded from database")
        End Sub

        Public Shared Sub LoadResponse()

            MyConsole.StartLoading("Loading response from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM npc_response"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewResponse As New ResponseNPC

                    NewResponse.ResponseID = Result("responseid")
                    NewResponse.Type = Result("type")
                    NewResponse.Args = Result("args")

                    ListOfResponse.Add(NewResponse)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfResponse.Count & "@' response loaded from database")
        End Sub

#Region "Functions"

        Public Shared Function QuestionExist(ByVal PnjID As Integer) As Boolean
            For Each MyQuestion As QuestionNPC In ListOfQuestion
                If MyQuestion.NpcID = PnjID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function _Get(ByVal PnjID As Integer) As QuestionNPC
            For Each MyQuestion As QuestionNPC In ListOfQuestion
                If MyQuestion.NpcID = PnjID Then
                    Return MyQuestion
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function ResponseExist(ByVal ResponseID As Integer) As Boolean
            For Each MyResponse As ResponseNPC In ListOfResponse
                If ResponseID = MyResponse.ResponseID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function _Get_R(ByVal ResponseID As Integer) As ResponseNPC
            For Each MyResponse As ResponseNPC In ListOfResponse
                If ResponseID = MyResponse.ResponseID Then
                    Return MyResponse
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function FormatPacket(ByVal Packet As String) As Integer
            Dim ThePacket() As String = Packet.Split("|")
            Return ThePacket(1)
        End Function

#End Region

        Public Shared Sub Launch(ByVal PnjId As Integer, ByVal Client As GameClient)
            Try
                If QuestionExist(PnjId) Then
                    Dim MyQuestion As QuestionNPC = _Get(PnjId)
                    Client.Send("DCK" & PnjId)
                    Client.Send("DQ" & MyQuestion.QuestionID & "|" + MyQuestion.ResponsesPossible)
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

        Public Shared Sub Reply(ByVal QuestionIDBrute As String, ByVal Client As GameClient)
            Try
                Dim QuestionID As String = FormatPacket(QuestionIDBrute)
                If ResponseExist(QuestionID) Then
                    Dim MyResponse As ResponseNPC = _Get_R(QuestionID)
                    'TODO World.EffectAction.ApplyEffect(Client, False, , MyResponse)
                    Client.Send("DV")
                End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

    End Class
End Namespace