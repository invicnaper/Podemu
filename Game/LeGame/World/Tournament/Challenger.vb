Namespace World

    Public Class Challenger

        Public Client As Game.GameClient

        Public BaseMap As Map

        Public State As ChallengerState = ChallengerState.WAITING

        Public Opponent As Challenger

        Public Round As Integer = 0

        Public Victory As Integer = 0


        Sub New(ByVal _client As Game.GameClient, ByVal _map As Map)
            Client = _client
            BaseMap = _map
        End Sub


    End Class

End Namespace
