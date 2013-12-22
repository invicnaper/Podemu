Imports System.Text

Namespace Utils
    Public Class MyConsole

        Public Shared ShowInfos, ShowNotices, ShowWarnings As Boolean
        Private Shared ConsoleLock As New Object

        Public Shared Function GetVersion() As String
            Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
        End Function

        Public Shared Sub DrawTitle()

            Console.Title = "PodEmu rev 0.3.0 - Realm "

            Write()
            Write("(  ____ )(  ___  )(  __  \ (  ____ \(       )|\     /|", ConsoleColor.Cyan)
            Write("| (    )|| (   ) || (  \  )| (    \/| () () || )   ( |", ConsoleColor.Cyan)
            Write("| (____)|| |   | || |   ) || (__    | || || || |   | |", ConsoleColor.Cyan)
            Write("|  _____)| |   | || |   | ||  __)   | |(_)| || |   | |", ConsoleColor.Cyan)
            Write("| (      | |   | || |   ) || (      | |   | || |   | |", ConsoleColor.Cyan)
            Write("| )      | (___) || (__/  )| (____/\| )   ( || (___) |-By Invic", ConsoleColor.Cyan)
            Write("|/       (_______)(______/ (_______/|/     \|(_______)-Merci Night", ConsoleColor.Cyan)
            Write("                                                     Mode realm", ConsoleColor.Cyan)
            DrawLine("_", ConsoleColor.Cyan)
            If MsgBox("Souhaitez-vous Fair un dons pour m'oncourager pour faire avancer pod emu?.Podemu est un emu gratuit avec plusieurs choses débug", 36, "Podemu - Dons") = MsgBoxResult.Yes Then
                Process.Start("http://www.inivc-projet.c.la")
            End If

            Write()

        End Sub

        Public Shared Sub Write()
            Console.WriteLine()
        End Sub
        Public Shared Sub Write(ByVal Message As String)

            Console.ForegroundColor = ConsoleColor.Gray
            Dim High As Boolean = False

            For Each c As Char In Message
                If High And c = "@" Then
                    Console.ForegroundColor = ConsoleColor.Gray
                    High = False
                ElseIf (Not High) And c = "@" Then
                    Console.ForegroundColor = ConsoleColor.White
                    High = True
                End If
                If (c <> "@") Then
                    Console.Write(c)
                End If
            Next

            Console.WriteLine()

        End Sub

        Public Shared Sub Write(ByVal Message As String, ByVal Color As ConsoleColor)
            Write(Message, Color, True)
        End Sub

        Public Shared Sub Write(ByVal Message As String, ByVal Color As ConsoleColor, ByVal Line As Boolean)
            Console.ForegroundColor = Color
            If Line Then
                Console.WriteLine(Message)
            Else
                Console.Write(Message)
            End If
        End Sub

        Public Shared Sub DrawLine(ByVal Character As Char, ByVal Color As ConsoleColor)

            Dim Bar As New StringBuilder("  ")

            For i As Integer = 0 To Console.BufferWidth - 5
                Bar.Append(Character)
            Next

            Write(Bar.ToString(), Color)

        End Sub

        Public Shared Sub Info(ByVal Message As String)
            SyncLock ConsoleLock
                If ShowInfos Then
                    Write("[Info]", ConsoleColor.White, False)
                    Write(": " & Message)
                End If
            End SyncLock
        End Sub

        Public Shared Sub Status(ByVal Message As String)
            SyncLock ConsoleLock
                Write("[Status]", ConsoleColor.Cyan, False)
                Write(": " & Message)
            End SyncLock
        End Sub

        Public Shared Sub Warn(ByVal Message As String)
            SyncLock ConsoleLock
                If ShowWarnings Then
                    Write("[Warning]", ConsoleColor.Yellow, False)
                    Write(": " & Message)
                End If
            End SyncLock
        End Sub

        Public Shared Sub Err(ByVal Message As String, Optional ByVal Fatal As Boolean = False)
            SyncLock ConsoleLock
                Write("[Error]", ConsoleColor.Red, False)
                Write(": " & Message)
                If Fatal Then
                    Console.ReadKey()
                    Environment.Exit(0)
                End If
            End SyncLock
        End Sub

        Public Shared Sub Notice(ByVal Message As String)
            SyncLock ConsoleLock
                If ShowNotices Then
                    Write("[Notice]", ConsoleColor.White, False)
                    Write(": " & Message)
                End If
            End SyncLock
        End Sub

    End Class
End Namespace