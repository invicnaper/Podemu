﻿Namespace Utils
    Public Class MyConsole

        Public Shared ErrorCount As Int16 = 0
        Public Shared ShowInfos, ShowNotices, ShowWarnings As Boolean
        Private Shared ConsoleLock As New Object

        Public Shared Function GetVersion() As String
            Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
        End Function

        Public Shared Sub DrawTitle()

            Console.CursorVisible = False
            If Config.GetItem("MODE_LIGHT") = "True" Then

                Console.Title = "PodLight rev 0.3.0101.1 - Gameserver "
                Console.CursorVisible = False
                Write()
               Write("    _____   _____   _____   _       _   _____   _   _   _____   ", ConsoleColor.Cyan)
                Write("   |  _  \ /  _  \ |  _  \ | |     | | /  ___| | | | | |_   _|  ", ConsoleColor.Cyan)
                Write("   | |_| | | | | | | | | | | |     | | | |     | |_| |   | |    ", ConsoleColor.Cyan)
                Write("   |  ___/ | | | | | | | | | |     | | | |  _  |  _  |   | | by  Invic", ConsoleColor.Cyan)
                Write("   | |     | |_| | | |_| | | |___  | | | |_| | | | | |   | |          ", ConsoleColor.Cyan)
                Write("   |_|     \_____/ |_____/ |_____| |_| \_____/ |_| |_|   |_|         ", ConsoleColor.Cyan)
                Write("                                                           Mode Light", ConsoleColor.Cyan)

            End If
            If Config.GetItem("ACTIVE_HEROIC") = "True" Then
                Console.Title = "Pod Emu rev 0.3.0 - Gameserver "

                Write("(  ____ )(  ___  )(  __  \ (  ____ \(       )|\     /|", ConsoleColor.Cyan)
                Write("| (    )|| (   ) || (  \  )| (    \/| () () || )   ( |", ConsoleColor.Cyan)
                Write("| (____)|| |   | || |   ) || (__    | || || || |   | |", ConsoleColor.Cyan)
                Write("|  _____)| |   | || |   | ||  __)   | |(_)| || |   | |", ConsoleColor.Cyan)
                Write("| (      | |   | || |   ) || (      | |   | || |   | |", ConsoleColor.Cyan)
                Write("| )      | (___) || (__/  )| (____/\| )   ( || (___) |-By Invic", ConsoleColor.Cyan)
                Write("|/       (_______)(______/ (_______/|/     \|(_______)-Merci Night", ConsoleColor.Cyan)
                Write("                                                     Mode Heroic", ConsoleColor.Cyan)
                DrawLine("_", ConsoleColor.Cyan)

                Write()
                Write()

            End If
            If Config.GetItem("MODE_LIGHT") = "False" Then
                Console.Title = "Pod Emu rev 0.3.0 - Gameserver "
                Console.CursorVisible = False
                Write()
                Write("(  ____ )(  ___  )(  __  \ (  ____ \(       )|\     /|", ConsoleColor.Cyan)
                Write("| (    )|| (   ) || (  \  )| (    \/| () () || )   ( |", ConsoleColor.Cyan)
                Write("| (____)|| |   | || |   ) || (__    | || || || |   | |", ConsoleColor.Cyan)
                Write("|  _____)| |   | || |   | ||  __)   | |(_)| || |   | |", ConsoleColor.Cyan)
                Write("| (      | |   | || |   ) || (      | |   | || |   | |", ConsoleColor.Cyan)
                Write("| )      | (___) || (__/  )| (____/\| )   ( || (___) |-By Invic", ConsoleColor.Cyan)
                Write("|/       (_______)(______/ (_______/|/     \|(_______)-Merci Night", ConsoleColor.Cyan)
                Write("                                                     Mode Normale", ConsoleColor.Cyan)
                DrawLine("_", ConsoleColor.Cyan)

                Write()
            End If
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

        Public Shared Sub SetName(ByVal Id As Integer, ByVal Value As String, ByVal Right As Boolean)

            SyncLock ConsoleLock

                While Value.Length < 34
                    Value &= " "
                End While

                Console.ForegroundColor = ConsoleColor.White

                Console.CursorTop = 8 + Id * 2
                Console.CursorLeft = If(Right, 41, 3)

                Console.Write(Value)

                SetProperty(Id, 0, Right)

            End SyncLock

        End Sub

        Public Shared Sub SetProperty(ByVal Id As Integer, ByVal Value As String, ByVal Right As Boolean)

            SyncLock ConsoleLock

                While Value.Length < 12
                    Value &= " "
                End While

                Console.ForegroundColor = If(IsNumeric(Value), If(Value >= 0, If(Value > 0, ConsoleColor.Green, ConsoleColor.Cyan), ConsoleColor.Red), ConsoleColor.Magenta)

                Console.CursorTop = 8 + Id * 2
                Console.CursorLeft = If(Right, 61, 23)
                If Id = 3 Then
                    Console.ForegroundColor = ConsoleColor.Red
                End If

                Console.Write(Value)

            End SyncLock

        End Sub

        Public Shared Sub DrawLine(ByVal Character As Char, ByVal Color As ConsoleColor)

            Dim Bar As String = "  "

            For i As Integer = 0 To Console.BufferWidth - 5
                Bar &= Character
            Next

            Write(Bar, Color)

        End Sub

#Region " Loading tick "

        Private Shared WithEvents TimerLoad As New Timers.Timer
        Private Shared StartX, StartY As Integer
        Private Shared ActualSymbol As Integer

        Public Shared Sub StartLoading(ByVal Message As String)

            SyncLock ConsoleLock
                Write("[Info]", ConsoleColor.White, False)
                Write(": " & Message & " [-]", ConsoleColor.Gray, False)
                StartX = Console.CursorLeft - 2
                StartY = Console.CursorTop
                Console.WriteLine()
                ActualSymbol = 0
                TimerLoad.Interval = 50
                TimerLoad.Enabled = True
            End SyncLock

        End Sub

        Private Shared Sub DoLoad() Handles TimerLoad.Elapsed

            SyncLock ConsoleLock
                Dim LastX, LastY As Integer
                LastX = Console.CursorLeft
                LastY = Console.CursorTop

                Console.SetCursorPosition(StartX, StartY)

                Select Case ActualSymbol
                    Case 0
                        Console.Write("|")
                    Case 1
                        Console.Write("/")
                    Case 2
                        Console.Write("-")
                    Case 3
                        Console.Write("\")
                End Select

                Console.SetCursorPosition(LastX, LastY)

                ActualSymbol = (ActualSymbol + 1) Mod 4

            End SyncLock

        End Sub

        Public Shared Sub StopLoading()

            SyncLock ConsoleLock
                If TimerLoad.Enabled Then

                    Dim LastX, LastY As Integer
                    LastX = Console.CursorLeft
                    LastY = Console.CursorTop

                    TimerLoad.Enabled = False
                    Console.SetCursorPosition(StartX - 1, StartY)
                    Console.Write("Ok!")

                    Console.SetCursorPosition(LastX, LastY)

                End If
            End SyncLock

        End Sub

#End Region

#Region " Messages and logs "

        Public Shared ErrWriter As IO.StreamWriter

        Public Shared Sub Status(ByVal Message As String)
            SyncLock ConsoleLock
                Write("[Status]", ConsoleColor.Green, False)
                Write(": " & Message)
            End SyncLock
        End Sub

        Public Shared Sub Err(ByVal Value As String, Optional ByVal Fatal As Boolean = False)
            ErrorCount += 1
            If Fatal Then
                Console.Clear()
                DrawTitle()
                Write("       [FATAL ERROR]", ConsoleColor.Red)
                Write()
                Write(Value)
                Console.ReadKey()
                Environment.Exit(0)
                Exit Sub
            Else

                If ErrWriter Is Nothing Then StartLogs()

                ErrWriter.WriteLine("Exception caught at " & Now.TimeOfDay.ToString() & " : ")
                ErrWriter.WriteLine()
                ErrWriter.WriteLine(Value)
                ErrWriter.WriteLine()
                ErrWriter.WriteLine(" ----------- ")
                ErrWriter.WriteLine()
            End If
        End Sub

        Public Shared Sub StartLogs()

            If Not IO.Directory.Exists("logs/errors/") Then IO.Directory.CreateDirectory("logs/errors/")

            ErrWriter = New IO.StreamWriter(New IO.FileStream("logs/errors/" & Now.ToString.Replace("/", "-").Replace(":", "-") & ".txt", IO.FileMode.Create))
            ErrWriter.AutoFlush = True

        End Sub

#End Region

    End Class
End Namespace