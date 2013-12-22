Imports MySql.Data.MySqlClient

Namespace Utils
    Public Class Sql

        Public Shared Accounts As New MySqlConnection
        Public Shared Others As New MySqlConnection

        Public Shared AccountsSync As New Object
        Public Shared OthersSync As New Object

        Public Shared Sub OpenConnexion()

            Try

                Dim ConnexionString As String = String.Concat("server=", Config.GetItem("SQL_IP"), ";" _
                                        , "uid=", Config.GetItem("SQL_USER"), ";" _
                                        , "pwd='", Config.GetItem("SQL_PASS"), "';" _
                                        , "database=", Config.GetItem("SQL_ACCOUNTS"), ";")

                Accounts.ConnectionString = ConnexionString
                Accounts.Open()

                ConnexionString = String.Concat("server=", Config.GetItem("SQL_IP"), ";" _
                        , "uid=", Config.GetItem("SQL_USER"), ";" _
                        , "pwd='", Config.GetItem("SQL_PASS"), "';" _
                        , "database=", Config.GetItem("SQL_OTHERS"), ";")

                Others.ConnectionString = ConnexionString
                Others.Open()

                MyConsole.Status("Connected to MySQL server")

            Catch ex As MySqlException
                MyConsole.Err("Can't connect to MySQL server", True)
            End Try

        End Sub

    End Class
End Namespace