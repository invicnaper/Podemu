Namespace World
    Public Class ForbiddenWords

        Public Shared ListOfForbiddenWords As New List(Of String)

        Public Shared Sub LoadFW()
            Utils.MyConsole.StartLoading("Loading ForbiddenWords from file...")
            Dim MyReader As New IO.StreamReader("forbidden_word.txt")
            Do
                If Not MyReader.ReadLine = Nothing Then
                    ListOfForbiddenWords.Add(MyReader.ReadLine)
                End If
            Loop Until MyReader.ReadLine = Nothing
            MyReader.Close()
            Utils.MyConsole.StopLoading()
            Utils.MyConsole.Status("'@" & ListOfForbiddenWords.Count & "@' ForbiddenWords loaded from file")
        End Sub

        Public Shared Function IfContainsForbiddenWord(ByVal Message As String) As Boolean
            For Each MyWord As String In ListOfForbiddenWords
                If Message.Contains(MyWord) Then
                    Return True
                End If
            Next
            Return False
        End Function

    End Class
End Namespace
