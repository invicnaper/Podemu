Namespace Game
    Public Class CharacterEmotes

        Public Sub New(ByVal Character As Character)
            Me.Character = Character
        End Sub


        Public EmoteCapacity As Integer

        Public Function Can(ByVal Type As EmoteEnum) As Boolean
            Return (Me.EmoteCapacity And Type) = Type
        End Function

        Public Sub SetValue(ByVal Type As EmoteEnum, ByVal Value As Boolean)
            If Value Then
                EmoteCapacity = EmoteCapacity Or Type
            Else
                EmoteCapacity = EmoteCapacity Xor Type
            End If
        End Sub

        Public Character As Character
        Public PlayedEmote As EmoteEnum = 0


    End Class

    Public Enum EmoteEnum
        Sit = 1
        Bye = 2
        Applause = 4
        Angry = 8
        Fear = 16
        Weapon = 32
        Flute = 64
        Pet = 128
        Hello = 256
        Kiss = 512
        Stone = 1024
        Sheet = 2048
        Scissors = 4096
        CrossArm = 8192
        Point = 16384
        Crow = 32768
        Rest = 262144
        Champ = 1048576
        PowerAura = 2097152
        VampyrAura = 4194304
    End Enum

End Namespace