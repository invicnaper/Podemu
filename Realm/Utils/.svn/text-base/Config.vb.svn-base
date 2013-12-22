Namespace Utils
    Public Class Config

        Private Shared _Items As New Dictionary(Of String, String)

        Public Shared Function GetItem(ByVal Item As String) As String
            If _Items.ContainsKey(Item) Then Return _Items(Item)
            MyConsole.Err("Can't find item '" & Item & "' in configuration file", True)
            Return ""
        End Function

        Public Shared Function GetItem(Of T)(ByVal Item As String) As String
            If _Items.ContainsKey(Item) Then Return Convert.ChangeType(_Items(Item), GetType(T))
            MyConsole.Err("Can't find item '" & Item & "' in configuration file", True)
            Return ""
        End Function

        Public Shared Sub LoadConfig()

            If Not IO.File.Exists("realm.txt") Then
                MyConsole.Err("Can't find configuration file !", True)
            End If

            Try

                Dim Reader As New IO.StreamReader("realm.txt")

                While Not Reader.EndOfStream

                    Dim Line As String = Reader.ReadLine

                    If Line.Trim.StartsWith("#") Then Continue While
                    If Not Line.Contains("=") Then Continue While

                    Dim LigneInfos() As String = Line.Split("=".ToCharArray, 2)

                    Dim Item As String = LigneInfos(0).ToUpper.Trim
                    Dim Value As String = LigneInfos(1).Trim

                    If Item = "" Then Continue While

                    _Items.Add(Item, Value)

                End While

                UseConfig()

                MyConsole.Status("Configuration file loaded")

            Catch ex As Exception

                MyConsole.Err("Can't read configuration file !", True)

            End Try

        End Sub

        Private Shared Sub UseConfig()

            MyConsole.ShowInfos = GetItem("SHOW_INFOS")
            MyConsole.ShowNotices = GetItem("SHOW_NOTICES")
            MyConsole.ShowWarnings = GetItem("SHOW_WARNINGS")

        End Sub

    End Class
End Namespace