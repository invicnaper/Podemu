Imports System.Threading
Imports Podemu.Game.Actions

Namespace Game
    Public Class CharacterState

        Public ReadOnly Character As Character

        Private m_isMoving As Boolean = False

        Public Property IsMoving As Boolean
            Get
                Return m_isMoving
            End Get
            Set(ByVal value As Boolean)
                If Not value Then
                    Execute()
                    m_isMoving = False
                End If
                m_isMoving = value
            End Set
        End Property

        Public OriginalCell As Integer = -1
        Public DestinationCell As Integer = -1

        Public InBattle As Boolean = False
        Public ChallengeAsk As Integer = -1
        Public ChallengeAsked As Integer = -1
        Public GetFight As World.Fight
        Public GetFighter As World.Fighter

        Public IsSitted As Boolean = False
        Public IsPlayingFlute As Boolean = False
        Public IsCrossingArm As Boolean = False
        Public IsPointing As Boolean = False
        Public IsResting As Boolean = False
        Public IsChamp As Boolean = False

        Public IsMerchant As Boolean = False
        Public IsEmptyMerchant As Boolean = False

        Public DungeonId As Integer = -1

        Public IsInBank As Boolean = False

        Public IsGhost As Boolean = False

        Public InTournament As Boolean = False
        Public CurrentGameAction As Integer = 0
        Public IsTrading As Boolean = False
        Public TradeType As Trading
        Public TraderId As Integer = 0
        Public Trader As Object
        Public IsFauching As Boolean = False
        Public IsFM As Boolean = False
        Public IsFollowed As Boolean = False

        Public InParty As Boolean = False
        Public PartyInvited As Integer = -1
        Public PartyInvite As Integer = -1
        Public GetParty As Party

        Public DialogWith As Integer = 0

        Public IsAway As Boolean = False
        Public IsMounted As Boolean = False
        ' Public IsAway As Boolean = False
        Public isTradingHDVSOLD As Boolean = False
        Public isTradingHDVBUY As Boolean = False
        'Public IsMounted As Boolean = False
        Public Paddock As World.Paddock


        Public Sub New(ByVal Character As Character)
            Me.Character = Character
        End Sub

        Public Function Occuped() As Boolean
            Return m_isMoving OrElse IsTrading OrElse InBattle OrElse (PartyInvite <> -1) OrElse (PartyInvited <> -1) _
                Or (ChallengeAsk <> -1) OrElse (ChallengeAsked <> -1) OrElse (DialogWith <> 0) Or Character.IsDead Or IsGhost
        End Function

        Public Function IsInDugeon() As Boolean
            Return Character.GetMap.IsDungeon
        End Function

        Public Sub BeginTrade(ByVal Type As Trading, ByVal Trader As Object, Optional ByVal TraderId As Integer = 0)
            Me.IsTrading = True
            Me.TradeType = Type
            Me.TraderId = TraderId
            Me.Trader = Trader
        End Sub

        Public Function GetTraderAs(Of T)() As T
            If Trader Is Nothing Then Return Nothing
            Return DirectCast(Trader, T)
        End Function

        Public Sub EndTrade()
            Me.IsTrading = False
            Me.TradeType = Nothing
            Me.TraderId = Nothing
            Me.Trader = Nothing
        End Sub

        Private Shared WaitingActions As New List(Of Action)

        Public Sub ExecuteWhenArrived(ByVal Action As Action)
            If m_isMoving Then
                WaitingActions.Add(Action)
            Else
                Action.Invoke()
            End If
        End Sub

        Private Sub Execute()
            For Each Action As Action In WaitingActions
                Action.Invoke()
            Next
            WaitingActions.Clear()
        End Sub

    End Class

    Public Enum Trading
        Npc = 0
        Exchange = 1
        Merchant = 2
        MerchantBag = 3
        Paddock = 4
        Zaap = 5
        Storage = 6
    End Enum

End Namespace