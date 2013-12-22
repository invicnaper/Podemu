Namespace Game
    Public Class GameState

        Public Enum WaitingPacket
            None
            Ticket
            Character
            Create
            Basic
        End Enum

        Public State As WaitingPacket = WaitingPacket.Ticket
        Public Created As Boolean = False

    End Class
End Namespace