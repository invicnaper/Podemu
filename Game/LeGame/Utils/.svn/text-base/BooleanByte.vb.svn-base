Namespace Utils
    Public Class BooleanByte

        Public Shared Function SetFlag(ByVal Number As Integer, ByVal Flag As Integer, ByVal Value As Boolean) As Integer

            Select Case Flag

                Case 0
                    If Value Then
                        Return Number Or 1
                    Else
                        Return Number And (255 - 1)
                    End If
                Case 1
                    If Value Then
                        Return Number Or 2
                    Else
                        Return Number And (255 - 2)
                    End If
                Case 2
                    If Value Then
                        Return Number Or 4
                    Else
                        Return Number And (255 - 4)
                    End If
                Case 3
                    If Value Then
                        Return Number Or 8
                    Else
                        Return Number And (255 - 8)
                    End If
                Case 4
                    If Value Then
                        Return Number Or 16
                    Else
                        Return Number And (255 - 16)
                    End If
                Case 5
                    If Value Then
                        Return Number Or 32
                    Else
                        Return Number And (255 - 32)
                    End If
                Case 6
                    If Value Then
                        Return Number Or 64
                    Else
                        Return Number And (255 - 64)
                    End If
                Case 7
                    If Value Then
                        Return Number Or 128
                    Else
                        Return Number And (255 - 128)
                    End If
            End Select

        End Function

        Public Shared Function GetFlag(ByVal Number As Integer, ByVal Flag As Integer) As Boolean

            Select Case Flag

                Case 0
                    Return (Number And 1) <> 0
                Case 1
                    Return (Number And 2) <> 0
                Case 2
                    Return (Number And 4) <> 0
                Case 3
                    Return (Number And 8) <> 0
                Case 4
                    Return (Number And 16) <> 0
                Case 5
                    Return (Number And 32) <> 0
                Case 6
                    Return (Number And 64) <> 0
                Case 7
                    Return (Number And 128) <> 0

            End Select

            Return False

        End Function

    End Class

End Namespace