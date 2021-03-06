﻿Imports Podemu.Utils.Basic

Namespace Utils
    Public Class Name

        Private Shared Voyelles As String = "aeiouy"
        Private Shared Consonnes As String = "bcdfghjklmnpqrstvwxz"

        Private Shared Function Voyelle() As String
            Return Voyelles(Rand(0, Voyelles.Length - 1))
        End Function

        Private Shared Function Consonne() As String
            Return Consonnes(Rand(0, Consonnes.Length - 1))
        End Function

        Public Shared Function IsCorrectName(ByVal CharName As String) As Boolean

            If CharName.Length > 18 OrElse CharName.Length < 3 Then Return False

            Dim TiretCount As Integer = CharName.Split("-").Length
            If TiretCount > 2 Then Return False

            For Each Letter As Char In CharName
                If (Not Char.IsLetter(Letter)) And (Not Letter = "-") Then Return False
            Next

            Return True

        End Function

        Public Shared Function MakeCorrectName(ByVal CharName As String) As String

            Dim NewName As String = ""
            Dim Part() As String = CharName.Split("-")
            Dim First As Boolean = True

            For i As Integer = 0 To Part.Length - 1

                If First Then
                    First = False
                Else
                    NewName &= "-"
                End If

                Dim ActPart As String = Part(i)
                If i = 0 Then
                    NewName &= Mid(ActPart, 1, 1).ToUpper & Mid(ActPart, 2).ToLower
                Else
                    NewName &= Mid(ActPart, 1, 1) & Mid(ActPart, 2).ToLower
                End If

            Next

            Return NewName

        End Function

        Public Shared Function GetRandomName() As String

            Dim Name As String = ""

            Name &= Consonne.ToUpper
            Name &= Voyelle()
            Name &= Voyelle()
            Name &= Consonne()
            Name &= Consonne()
            Name &= Voyelle()
            If Rand(0, 1) = 0 Then
                Name &= Consonne()
                Name &= Voyelle()
            End If

            Return Name

        End Function

    End Class
End Namespace