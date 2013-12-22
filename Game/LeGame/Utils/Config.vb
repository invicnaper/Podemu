Namespace Utils
    Public Class Config

        Private Shared _Items As New Dictionary(Of String, String)
        Public Shared Censured As String()

        Public Shared Function GetItem(ByVal Item As String) As String
            If _Items.ContainsKey(Item) Then Return _Items(Item)
            MyConsole.Err("Can't find item '" & Item & "' in configuration file", True)
            Return ""
        End Function

        Public Shared Function GetItem(Of T)(ByVal Item As String) As T
            If _Items.ContainsKey(Item) Then Return Convert.ChangeType(_Items(Item), GetType(T))
            MyConsole.Err("Can't find item '" & Item & "' in configuration file", True)
            Return Nothing
        End Function

        Public Shared Sub LoadConfig()

            If Not IO.File.Exists("podemu.txt") Then
                MyConsole.Err("Can't find configuration file !", True)
            End If

            Try

                Dim Reader As New IO.StreamReader("podemu.txt")

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

            Catch ex As Exception

                MyConsole.Err("Can't read configuration file !", True)

            End Try

        End Sub
        Public Shared Sub ReLoadConfig()
            If Not IO.File.Exists("podemu.txt") Then
                MyConsole.Err("Can't find configuration file !", True)
            End If

            Try

                Dim Reader As New IO.StreamReader("podemu.txt")

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

            Catch ex As Exception

                MyConsole.Err("Can't read configuration file !", True)

            End Try

        End Sub

        Private Shared Sub UseConfig()

            ServerId = CInt(GetItem("GAME_ID"))
            MyConsole.ShowInfos = CBool(GetItem("SHOW_INFOS"))
            MyConsole.ShowNotices = CBool(GetItem("SHOW_NOTICES"))
            MyConsole.ShowWarnings = CBool(GetItem("SHOW_WARNINGS"))
            Censured = GetItem("CENSURED_WORDS").Split(";")
            Console.Title = "Vemu v" & MyConsole.GetVersion() & " ~ Serveur " & ServerId

        End Sub

    End Class
End Namespace