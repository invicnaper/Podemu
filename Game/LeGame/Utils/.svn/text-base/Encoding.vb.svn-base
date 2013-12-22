Imports System.Text

Namespace Utils
    Public Class Encoding

        Public Shared Function Utf8Encoder(ByVal msg As String) As String
            msg = msg.Replace("à", Chr(&HC3) & Chr(&HA0))
            msg = msg.Replace("ç", Chr(&HC3) & Chr(&HA8))
            msg = msg.Replace("è", Chr(&HC3) & Chr(&HA8))
            msg = msg.Replace("é", Chr(&HC3) & Chr(&HA9))
            msg = msg.Replace("ê", Chr(&HC3) & Chr(&HAA))
            msg = msg.Replace("ô", Chr(&HC3) & Chr(&HB4))
            Return msg
        End Function

        Public Shared Function AsciiDecoder(ByVal msg As String) As String

            Dim msgFinal As New StringBuilder

            Dim iMax As Integer = msg.Length
            Dim i As Integer = 0

            While (i < iMax)

                Dim caractC As Char = msg(i)
                Dim caractI As Integer = Asc(caractC)
                Dim nbLettreCode As Integer = 1

                If (caractI And 128) = 0 Then

                    msgFinal.Append(ChrW(caractI))

                Else

                    Dim temp As Integer = 64
                    Dim codecPremLettre As Integer = 63

                    While (caractI And temp)

                        temp >>= 1
                        codecPremLettre = codecPremLettre Xor temp
                        nbLettreCode += 1

                    End While

                    codecPremLettre = codecPremLettre And 255

                    Dim caractIFinal As Integer = caractI And codecPremLettre

                    nbLettreCode -= 1
                    i += 1

                    While (nbLettreCode <> 0)

                        caractC = msg(i)
                        caractI = Asc(caractC)

                        caractIFinal <<= 6
                        caractIFinal = caractIFinal Or (caractI And 63)

                        nbLettreCode -= 1
                        i += 1

                    End While

                    msgFinal.Append(ChrW(caractIFinal))

                End If

                i += nbLettreCode

            End While

            Return msgFinal.ToString()

        End Function

    End Class
End Namespace