Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class MobsSuiveursManager

        Private Shared MobsSuiveursTemplates As New Dictionary(Of Integer, MobSuiveurTemplate)
        Private Shared _getMSTByMonster As MobSuiveurTemplate

        Shared Property GetMSTByMonster(ByVal Mobid As Integer) As MobSuiveurTemplate
            Get
                Return _getMSTByMonster
            End Get
            Set(ByVal value As MobSuiveurTemplate)
                _getMSTByMonster = value
            End Set
        End Property

        Public Shared Sub loadMobsSuiveurs()

            MyConsole.StartLoading("Loading MobsSuiveurs from database...")

            SyncLock Sql.OthersSync

                MobsSuiveursTemplates.Clear()

                Try

                    Dim SQLText As String = "SELECT * FROM MobsSuiveurs"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewMST As New MobSuiveurTemplate

                        NewMST.Mobid = Result("IdMob")
                        NewMST.ItemId = Result("IdObjet")
                        NewMST.Tours = Result("Tours")

                        MobsSuiveursTemplates.Add(NewMST.Mobid, NewMST)

                    End While

                    Result.Close()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't load items : " & ex.Message, True)
                    Exit Sub
                End Try

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & MobsSuiveursTemplates.Count & "@' MobsSuiveurs loaded from database")

        End Sub

    End Class
End Namespace