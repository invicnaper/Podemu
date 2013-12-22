Imports Podemu.Utils
Imports Podemu.Game
Imports System.Text
Imports Podemu.World
Imports MySql.Data.MySqlClient
Imports Podemu.World.Players
Imports System.Threading
Namespace World

    Public Class connecting
        Shared Cells As String
        Shared MapID As Object
        Shared Packet As Object
        Private Shared _existName As Boolean
        Shared CreateGuild As Integer
        Shared NewGuild As Object
        Shared Receiver As String
        Private Shared _parameters As String

        Private Shared Property GetPlayers As List(Of GameClient)
        Public Shared Sub podconnected()
            
        End Sub
    End Class
End Namespace
