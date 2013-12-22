Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class VipManager

        Public Vip As Boolean = 0

        Public Shared Function LoadVip() As String

            Dim Data1 As String = 0

            SyncLock Sql.AccountsSync
                Dim SQLText As String = "SELECT * FROM player_accounts"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

                Dim Result = SQLCommand.ExecuteReader
                If Result.Read Then
                    Data1 = Result("vip")
                Else
                    Data1 = 0
                End If

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

            End SyncLock
            Return Data1
        End Function
    End Class
End Namespace
