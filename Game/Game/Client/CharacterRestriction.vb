Imports System.Threading
Imports Podemu.Utils

Namespace Game
    Public Class CharacterRestriction

        <Flags()>
        Public Enum RestrictionEnum
            CanBeAssault = 1
            CanBeChallenge = 2
            CanExchange = 4
            CanBeAttack = 8
            ForceWalk = 16
            IsSlow = 32
            CanSwitchInCreature = 64
            IsTombe = 128
        End Enum

        Public ReadOnly Character As Character
        Public Restriction As Integer = 0

        Public Sub New(ByVal Character As Character)
            Me.Character = Character
        End Sub

        Public ReadOnly Property B36Restrictions As String
            Get
                Return Basic.ToBase36(Restriction)
            End Get
        End Property


        Public Function Can(ByVal Type As RestrictionEnum) As Boolean
            Return (Me.Restriction And Type) = Type
        End Function

        Public Sub SetValue(ByVal Type As RestrictionEnum, ByVal Value As Boolean)
            If Value Then
                Restriction = Restriction Or Type
            Else
                Restriction = Restriction Xor Type
            End If
        End Sub

    End Class
End Namespace