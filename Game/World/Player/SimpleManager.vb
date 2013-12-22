Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Namespace World
    Public Class SimpleManager

#Region "List"
        Public Shared ListOfSimpleC As New List(Of SimpleCommand)
#End Region

#Region "Function"

        Public Shared Function Exist(ByVal Command As String) As Boolean
            For Each MyCommand As SimpleCommand In ListOfSimpleC
                If MyCommand.Name = Command Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Shared Function _Get(ByVal Command As String) As SimpleCommand
            For Each MySimpleCommand As SimpleCommand In ListOfSimpleC
                If MySimpleCommand.Name = Command Then
                    Return MySimpleCommand
                End If
            Next
            Return Nothing
        End Function

#End Region

#Region "SQL"

        Public Shared Sub LoadSimpleC()
            MyConsole.StartLoading("Loading SimpleCommands from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM simple_commands"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewSC As New SimpleCommand

                    NewSC.Name = Result("command_name")
                    NewSC.Args = Result("args")

                    ListOfSimpleC.Add(NewSC)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfSimpleC.Count & "@' SimpleCommands loaded from database")

        End Sub

#End Region

#Region "Manager"

        Public Shared Sub ExecCommand(ByVal Client As Game.GameClient, ByVal Command As String)
            If Exist(Command) Then
                Dim MySimpleCommand As SimpleCommand = _Get(Command)
                Dim MyArgs() As String = MySimpleCommand.Args.Split("|")
                Client.Character.TeleportTo(MyArgs(0), MyArgs(1))
            End If
        End Sub

#End Region

    End Class
End Namespace
