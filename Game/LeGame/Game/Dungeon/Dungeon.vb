Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class DungeonTemplate

        Public Id As Integer
        Public Rooms As New List(Of Map)(8)

        Public Sub ToNextRoom(ByVal Client As GameClient)
            Dim room = NextRoom(Client.Character.GetMap)
            Client.Character.MapId = room.Id
            Client.Character.MapCell = room.GetRandomCell(False)
        End Sub

        Public Function NextRoom(ByVal Room As Map) As Map

            Return Rooms(Rooms.IndexOf(Room) + 1)

        End Function

    End Class
End Namespace