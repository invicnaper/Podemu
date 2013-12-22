Namespace World
    Public Class LoginKeys

        Public Shared KeyList As New List(Of Game.LoginKey)

        Public Shared Function TicketExist(ByVal Ticket As String) As Boolean

            For Each Key As Game.LoginKey In KeyList.ToArray
                If Not Key Is Nothing Then
                    If Key.Key = Ticket Then Return True
                End If
            Next

            Return False

        End Function

        Public Shared Function GetKey(ByVal Key As String) As Game.LoginKey

            For Each ActualGameKey As Game.LoginKey In KeyList.ToArray
                If Not ActualGameKey Is Nothing Then
                    If (ActualGameKey.Key = Key) Then
                        Return ActualGameKey
                    End If
                End If
            Next

            Return Nothing

        End Function

    End Class
End Namespace