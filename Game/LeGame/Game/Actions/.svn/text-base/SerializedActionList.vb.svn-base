Namespace Game.Actions
    Public Class SerializedActionList

        Private ReadOnly ActionList As New List(Of SerializedAction)
        Private ReadOnly Message As String

        Sub New(ByVal Data As String)

            If Data.Length = 0 Then Throw New Exception("Data is null")

            Dim Actions = Data.Split("|")

            For Each Action In Actions
                If Action <> String.Empty Then
                    Try
                        If Action.Where(Function(c) c = ":").Count() <> 1 Then Throw New Exception("Can find the single : separator")

                        Dim pAction = Action.Split(":")
                        Dim NewAction As New SerializedAction

                        NewAction.Id = pAction(0)

                        If Not ActionHandler.Exists(NewAction.Id) Then Throw New Exception("Action doesn't exists")

                        NewAction.Action = ActionHandler.GetAction(NewAction.Id)

                        For Each param As String In pAction(1).Split(";")

                            If Not param.Contains("=") Then Throw New Exception("Can't find the = separator")

                            Dim sParam = param.Split("=")

                            NewAction.Params.Add(sParam(0).ToLower(), sParam(1))

                        Next

                        Dim CheckParam = ActionHandler.CheckParams(NewAction.Action, NewAction.Params)
                        If CheckParam <> "" Then Throw New Exception("Invalid parameters, action require parameter : " & CheckParam)

                        ActionList.Add(NewAction)
                    Catch ex As Exception
                        Throw New Exception("Error while adding the action : " & ex.Message & Environment.NewLine & "Data : " & Data)
                    End Try
                End If
            Next

        End Sub

        Public Sub Process(ByVal User As GameClient, ByVal TargetCell As String)
            For Each Action In ActionList
                Action.Process(User, TargetCell)
            Next
        End Sub

    End Class
End Namespace