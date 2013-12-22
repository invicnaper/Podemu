Imports System.Text
Imports System.Runtime.CompilerServices

Namespace Utils
    Public Class Basic

        Private Shared _Chars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_"
        Private Shared _Rand As New Random

        Public Shared Function Rand(ByVal Min As Integer, ByVal Max As Integer) As Integer
            Return _Rand.Next(Min, Max + 1)
        End Function

        Public Shared Function RandomString(ByVal Lenght As Integer) As String
            Dim NewString As New StringBuilder
            For i As Integer = 1 To Lenght
                NewString.Append(_Chars(Rand(0, _Chars.Length - 1)))
            Next
            Return NewString.ToString()
        End Function

        Public Shared Function DeciToHex(ByVal Deci As Integer) As String
            If Deci = -1 Then Return "-1"
            Return Deci.ToString("x")
        End Function

        Public Shared Function HexToDeci(ByVal Hex As String) As Integer
            If Hex = "-1" Or Hex = "" Then Return -1
            Return Convert.ToInt64(Hex, 16)
        End Function

        Public Shared Function ToBase36(ByVal intNumber As Integer) As String

            Dim strSum As String = ""
            Dim intCarry As Integer
            Dim intConvertBase As Integer = 36

            Do
                intCarry = intNumber Mod intConvertBase
                If intCarry > 9 Then
                    strSum = Chr(intCarry + 87) + strSum
                Else
                    strSum = intCarry & strSum
                End If
                intNumber = Math.Floor(intNumber / intConvertBase)
            Loop Until intNumber = 0

            Return strSum

        End Function

        Public Shared Function GetMinJet(ByVal JetStr As String) As Integer

            Return GetRandomJet(JetStr, 2)

        End Function

        Public Shared Function GetMaxJet(ByVal JetStr As String) As Integer

            Return GetRandomJet(JetStr, 1)

        End Function

        Public Shared Function GetRandomJet(ByVal JetStr As String, Optional ByVal MaxMin As Integer = 0) As Integer

            Dim degat As Integer = 0
            Dim des As Integer = JetStr.Split("d")(0)
            Dim faces As Integer = JetStr.Split("d")(1).Split("+")(0)
            Dim fixe As Integer = JetStr.Split("d")(1).Split("+")(1)

            If des <> 0 Then
                For i As Integer = 1 To des
                    If MaxMin = 1 Then
                        degat += faces
                    ElseIf MaxMin = 2 Then
                        degat += 1
                    ElseIf MaxMin = 3 Then
                        degat += Math.Ceiling(faces / 2)
                    Else
                        degat += Rand(1, faces)
                    End If
                Next
            End If

            Return degat + fixe

        End Function

        Public Shared StartTime As Long = 0

        Public Shared Function GetUptime() As String

            Dim TempTime As TimeSpan = TimeSpan.FromMilliseconds(Environment.TickCount - StartTime)
            Return Math.Floor(TempTime.TotalHours) & "h " & TempTime.Minutes & "m " & TempTime.Seconds & "s"

        End Function

        Public Shared Function RandomFourDir() As Byte
            Return Basic.Rand(0, 3) * 2 + 1
        End Function

        Public Shared Function HalfedRand(ByVal Min As Integer, ByVal Max As Integer) As Integer

            Dim rnds As New Dictionary(Of Integer, Integer)(100)
            Dim Half As Double = (Min + Max) / 2

            For i As Integer = Min To Max

                Dim Ecart = Math.Round(Half - Math.Abs(Half - i))

                For j As Integer = 0 To Ecart + 100000

                    Dim rnd = If(Min + i <= Max - i, Rand(Min + i, Max - i), Rand(Max - i + 1, Min + i - 1))

                    If Not rnds.ContainsKey(rnd) Then rnds.Add(rnd, 0)
                    rnds(rnd) += 1

                Next
            Next

            Dim Sum = rnds.Sum(Function(f) f.Value)
            Dim random = Rand(0, Sum)
            Dim tot = 0
            For Each Key In rnds
                tot += Key.Value
                If tot >= random Then Return Key.Key
            Next

        End Function


    End Class
End Namespace