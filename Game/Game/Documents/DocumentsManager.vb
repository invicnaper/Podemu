Imports MySql.Data.MySqlClient
Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic
Imports Vemu_gs.World

Namespace Game
    Public Class DocumentsManager

        Private Shared Documents As New Dictionary(Of Integer, Document)

        Public Shared Sub LoadDocuments()


            MyConsole.StartLoading("Loading Documents from database...")

            SyncLock Sql.OthersSync

                Documents.Clear()

                Try

                    Dim SQLText As String = "SELECT * FROM documents"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewDoc As New Document

                        NewDoc.id = Result("id")
                        NewDoc._Date = Result("date")

                        Documents.Add(NewDoc.id, NewDoc)

                    End While

                    Result.Close()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't load items : " & ex.Message, True)
                    Exit Sub
                End Try

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & Documents.Count & "@' Documents loaded from database")

        End Sub

        Public Shared Function GetDoc(ByVal docid As Integer) As Document
            Return Documents(docid)
        End Function
    End Class
End Namespace
