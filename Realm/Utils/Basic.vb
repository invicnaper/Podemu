Imports System.Text

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

        Public Shared StartTime As Long = 0

        Public Shared Function GetUptime() As String

            Dim TempTime As TimeSpan = TimeSpan.FromMilliseconds(Environment.TickCount - StartTime)
            Return String.Concat(TempTime.Days, "j ", TempTime.Hours, "h ", TempTime.Minutes, "m ", TempTime.Seconds, "s")

        End Function

    End Class
End Namespace