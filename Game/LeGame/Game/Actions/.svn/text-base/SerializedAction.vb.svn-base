Namespace Game.Actions
    Public Class SerializedAction

        Public Id As ActionEnum
        Public Action As BaseAction
        Public Params As New Dictionary(Of String, String)

        Public Sub Process(ByVal User As GameClient, ByVal TargetCell As String)
            Action.Process(User, TargetCell, Params)
        End Sub

    End Class
End Namespace