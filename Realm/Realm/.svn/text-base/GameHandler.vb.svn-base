Imports MySql.Data.MySqlClient
Imports Vemu_rs.Utils

Namespace Realm
    Public Class GameHandler

        Public Shared GameList As New List(Of GameServer)

        Public Shared Function IsAccountConnected(ByVal Account As String) As Boolean

            For Each Server As GameServer In GameList
                If Server.Accounts.Contains(Account) Then Return True
            Next
            Return False

        End Function

        Public Shared Function GetServer(ByVal Id As Integer) As GameServer

            For Each Server As GameServer In GameList
                If Server.Id = Id Then Return Server
            Next
            Return Nothing

        End Function

        Public Shared Function ValidServer() As Boolean

            For Each Server As GameServer In GameList
                If Server.Activaded Then Return True
            Next
            Return False

        End Function

        Public Shared Sub LoadGameServers()

            Dim SQLText As String = "SELECT * FROM servers_list"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim Id As Integer = Result("id")
                Dim Ip As String = Result("ip")
                Dim Port As Integer = Result("port")
                Dim SystemPort As Integer = Result("system_port")

                GameList.Add(New GameServer(Id, Ip, Port, SystemPort))

            End While

            Result.Close()

            Utils.MyConsole.Status("Successfully loaded @" & GameList.Count & "@ game servers")

        End Sub

    End Class
End Namespace