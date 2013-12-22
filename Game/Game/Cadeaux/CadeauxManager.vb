Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class CadeauxManager


        Public Shared Sub SetCadeauxByID(ByVal CharacterId As Integer, ByVal Cadeaux As String)
            SyncLock Sql.AccountsSync
                Dim UpdateString As String = "IDSCadeaux=@IDSCadeaux"
                Dim SQLText As String = "UPDATE player_accounts SET " & UpdateString & " WHERE id=@Tempo"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                Dim P As MySqlParameterCollection = SQLCommand.Parameters

                P.Add(New MySqlParameter("@IDSCadeaux", Cadeaux))
                P.Add(New MySqlParameter("@Tempo", CharacterId))

                SQLCommand.ExecuteNonQuery()

            End SyncLock
        End Sub
    End Class
End Namespace
