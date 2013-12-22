Namespace Utils
    Public Class Md5

        Private Shared Function ByteArrayToString(ByVal arrInput() As Byte) As String
            Dim sOutput As New System.Text.StringBuilder(arrInput.Length)
            For i As Integer = 0 To arrInput.Length - 1
                sOutput.Append(arrInput(i).ToString("X2"))
            Next
            Return sOutput.ToString().ToLower
        End Function

        Public Shared Function Hash(ByVal Text As String) As String

            Dim Md5Crypto As New System.Security.Cryptography.MD5CryptoServiceProvider

            Dim ByteText() As Byte = System.Text.Encoding.Default.GetBytes(Text)
            Dim ByteHash() As Byte = Md5Crypto.ComputeHash(ByteText)

            Md5Crypto.Clear()

            Return ByteArrayToString(ByteHash)

        End Function

    End Class
End Namespace