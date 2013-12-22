Imports System.IO
Imports System.Net
Namespace World
    Public Class Podpconfig
        Private WithEvents WClient As New WebClient
        Private ActualVersion As Integer
        Private NewVersion As Integer
        Public Shared _item As New List(Of Item)

        'Author Invic
        Public Shared Function _Get(ByVal name As String) As String
            For Each i As Item In _item
                If name = i.name Then
                    Return i.args
                End If
            Next
            Return Nothing
        End Function

        Public Shared Sub loadConfig()

            If IO.File.Exists("p_co/lunch.txt") Then
                Dim r As New IO.StreamReader("p_co/lunch.txt")
                Dim l As String
                Do
                    l = r.ReadLine
                    If Not l = Nothing Then
                        If Not l.StartsWith("//") Then
                            Dim s() As String = l.Split("=")
                            Dim i As New Item
                            With i
                                .name = s(0)
                                .args = s(1)
                            End With
                            _item.Add(i)
                        End If
                    End If
                Loop Until l = Nothing
                r.Close()
            Else

            End If
        End Sub

        Public Class Item

            Public name As String
            Public args As String

        End Class

    End Class
End Namespace
