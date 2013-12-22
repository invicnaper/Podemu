Imports MySql.Data.MySqlClient
Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic

Namespace Game
    Public Class BanditManager

        Private Shared BanditsTemplates As New Dictionary(Of Integer, BanditTemplate)

        Public Shared Sub SetupBandits()

            MyConsole.StartLoading("Loading bandits from database...")

            SyncLock Sql.OthersSync

                BanditsTemplates.Clear()

                Try

                    Dim SQLText As String = "SELECT * FROM bandits"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewBandit As New BanditTemplate

                        NewBandit.ID = Result("ID")
                        Try
                            NewBandit.Name = Result("Name")
                            NewBandit.Skin = Result("Skin")


                            Dim XpNiveau1 As String = Result("XPNiveau")
                            NewBandit.XPNiveau = Split(XpNiveau1, ";")

                            Dim eSpells As String = Result("spells")


                            NewBandit.Spells.ClearSpells()

                            If eSpells <> "" Then

                                Dim SpellsSplit() As String = eSpells.Split(";")



                                For i As Integer = 0 To SpellsSplit.Length - 1
                                    NewBandit.Spells.AddSpell(SpellsSplit(i), 1, 25)
                                Next


                            End If
                            NewBandit.ItemID = Result("ItemsID")
                            'MyConsole.Write("ID : " & NewBandit.ID & " Name : " & NewBandit.Name & " Skin : " & NewBandit.Skin)

                            BanditsTemplates.Add(NewBandit.ItemID, NewBandit)
                        Catch
                            Utils.MyConsole.Err(NewBandit.ItemID, True)
                        End Try
                    End While

                    Result.Close()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't load items : " & ex.Message, True)
                    Exit Sub
                End Try

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & BanditsTemplates.Count & "@' Bandits loaded from database")


        End Sub

        Public Shared Function BanditExist(ByVal ID As Integer) As Boolean
            For i As Integer = 0 To BanditsTemplates.Count - 1
                If BanditsTemplates(i).ID = ID Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function GetBanditTemplate(ByVal ID As Integer) As BanditTemplate
            Return BanditsTemplates(ID)
        End Function




    End Class
End Namespace
