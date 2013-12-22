Namespace World
    Public Class TournamentManager


        Public Shared Tournaments As New List(Of Tournament)

        Shared Function GetOpenedTournament() As Tournament
            For Each Tournament As Tournament In Tournaments
                If Tournament.State = TournamentState.WAITING Then
                    Return Tournament
                End If
            Next
            Return Nothing
        End Function

        Shared Function GetTournament(ByVal player As Game.GameClient) As Tournament
            For Each Tournament As Tournament In Tournaments
                If Tournament.State = TournamentState.WAITING Then
                    For Each p As Game.GameClient In Tournament.RegisterClient
                        If p.Equals(player) Then
                            Return Tournament
                        End If
                    Next
                ElseIf Tournament.State = TournamentState.START Then
                    For Each p As Challenger In Tournament.Challengers
                        If p.Client.Equals(player) Then
                            Return Tournament
                        End If
                    Next
                End If
            Next
            Return Nothing
        End Function

    End Class
End Namespace