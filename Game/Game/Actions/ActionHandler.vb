Namespace Game.Actions


    Public Class ActionHandler

        Public Shared TemplateList As New Dictionary(Of Integer, BaseAction)()

        Public Shared Function Exists(ByVal Id As ActionEnum) As Boolean
            Return TemplateList.ContainsKey(Id)
        End Function

        Public Shared Function GetAction(ByVal Id As ActionEnum) As BaseAction
            If TemplateList.ContainsKey(Id) Then
                Return TemplateList(Id)
            End If
            Return Nothing
        End Function

        Public Shared Function CheckParams(ByVal Action As BaseAction, ByVal Params As Dictionary(Of String, String)) As String
            For Each param In Action.ExpectedParams
                If Not Params.ContainsKey(param) Then Return param
            Next
            Return ""
        End Function

        Public Shared Sub SetupActions()

            TemplateList.Add(1, New GiveItemAction)
            TemplateList.Add(2, New GiveLifeAction)

        End Sub

    End Class
End Namespace