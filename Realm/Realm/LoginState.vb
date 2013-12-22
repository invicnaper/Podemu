Namespace Realm
    Public Class LoginState

        Public Enum WaitingPacket
            None
            Version
            Account
            Basic
        End Enum

        Public State As WaitingPacket = WaitingPacket.Version
        Public Key As String = ""
        Private Shared hash() As String = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-", "_"}

        Public Function CryptPassword(ByVal Password As String) As String

            Dim PasswordCrypt As String = "1"

            For i As Integer = 0 To Password.Length - 1

                Dim CharCodePwd As Integer = Asc(Password(i))
                Dim CharCodeKey As Integer = Asc(Key(i))

                Dim Dat1 As Integer = Math.Floor(CharCodePwd / 16)
                Dim Dat2 As Integer = CharCodePwd Mod 16

                PasswordCrypt &= hash(((Dat1 + CharCodeKey) Mod (UBound(hash) + 1)) Mod (UBound(hash) + 1))
                PasswordCrypt &= hash(((Dat2 + CharCodeKey) Mod (UBound(hash) + 1)) Mod (UBound(hash) + 1))

            Next

            Return PasswordCrypt

        End Function

    End Class
End Namespace