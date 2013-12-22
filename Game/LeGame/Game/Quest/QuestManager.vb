Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class QuestManager

        Private Shared QuestList As New List(Of Quest)
        Private Shared QuestStepList As New List(Of QuestStep)
        Private Shared QuestObjectifList As New List(Of QuestObjectif)



        Public Shared Sub LoadQuestObjectifs()
            MyConsole.StartLoading("Loading QuestObjectifs from database...")

            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM quest_objectifs"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewQO As New QuestObjectif

                    NewQO.id = Result("id")
                    NewQO.type = Result("type")
                    NewQO.arguments = Result("arguments")
                    NewQO.IsInvisble = Result("isInvisible")

                    QuestObjectifList.Add(NewQO)


                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & QuestObjectifList.Count & "@' QuestObjectifs loaded from database")
        End Sub

        Public Shared Sub LoadQuestSteps()
            MyConsole.StartLoading("Loading QuestSteps from database...")

            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM quest_steps"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewQS As New QuestStep

                    NewQS.id = Result("id")
                    Dim t As String = Result("objectifs")
                    Dim t2 As String() = t.Split("|")
                    For i As Integer = 0 To t2.Length - 1
                        Dim QObj As New QuestObjectif
                        QObj.Ordre = NewQS.Objectifs.Count
                        QObj = CloneObject.clone(GetObjectif(t2(i)))
                        NewQS.Objectifs.Add(QObj)
                    Next
                    If Not Result("gainobjet") Is Nothing Then
                        Dim d As String = Result("gainobjet")
                        Dim d2 As String() = d.Split(";")
                        For i As Integer = 0 To d2.Length - 1
                            NewQS.GainObjects.Add(d2(i))
                        Next
                    End If
                    If Not Result("gainkamas") Is Nothing Then
                        NewQS.GainKamas = Result("gainkamas")
                    Else
                        NewQS.GainKamas = 0
                    End If

                    If Not Result("gainxp") Is Nothing Then
                        NewQS.GainXp = Result("gainxp")
                    Else
                        NewQS.GainXp = 0
                    End If

                    NewQS.Dialogue = Result("dialogue")
                    QuestStepList.Add(NewQS)


                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & QuestStepList.Count & "@' QuestSteps loaded from database")
        End Sub

        Public Shared Sub LoadQuests()

            MyConsole.StartLoading("Loading Quests from database...")

            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM quests"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewQ As New Quest

                    NewQ.id = Result("id")
                    NewQ.Name = Result("name")
                    Dim t As String = Result("steps")
                    Dim t2 As String() = t.Split(";")
                    For i As Integer = 0 To t2.Length - 1
                        Dim Qstep As New QuestStep
                        Qstep = CloneObject.clone(GetQStep(t2(i)))
                        Qstep.Ordre = NewQ.Steps.Count
                        NewQ.Steps.Add(Qstep)
                    Next
                    If NewQ.Steps.Count > 0 Then
                        NewQ.NowStep = NewQ.GetStepByOrdre(0)
                    End If
                    NewQ.Conditions = Result("conditions")

                    QuestList.Add(NewQ)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & QuestList.Count & "@' Quests loaded from database")
        End Sub

        Public Shared Function GetObjectif(ByVal id As Integer) As QuestObjectif
            For Each QO As QuestObjectif In QuestObjectifList
                If QO.id = id Then
                    Return QO
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function GetQStep(ByVal id As Integer) As QuestStep
            For Each QS As QuestStep In QuestStepList
                If QS.id = id Then
                    Return QS
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function GetQuest(ByVal id As Integer) As Quest
            For Each Q As Quest In QuestList
                If Q.id = id Then
                    Return Q
                End If
            Next
            Return Nothing
        End Function

        Public Function Contains(ByVal Id As Integer) As Boolean
            For Each Q As Quest In QuestList
                If Q.id = Id Then
                    Return True
                End If
            Next
            Return False
        End Function


    End Class
End Namespace
