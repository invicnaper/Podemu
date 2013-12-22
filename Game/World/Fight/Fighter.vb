Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class Fighter

#Region " Initialisation "

        Public WaitingResult As Boolean
        Public IsDeco As Boolean
        Public DecoTurn As Integer = 0

        Public IsMonster As Boolean
        Public isInvocation As Boolean

        Public Client As GameClient
        Public Character As Character
        Public Fight As Fight
        Public Monster As Monster
        Public Starter As Boolean
        Public Owner As Fighter

        Public Invocs As New List(Of Fighter)

        Public Sub New(ByVal eClient As GameClient)
            Client = eClient
            Character = eClient.Character
            Client.Character.State.GetFighter = Me
            IsMonster = False
            Ready = False
            TurnReady = True
        End Sub

        Public Sub New(ByVal eMonster As Monster)
            Monster = eMonster
            IsMonster = True
            Starter = False
            Ready = True
            TurnReady = True
        End Sub

        Public Sub New(ByVal eMonster As Monster, ByVal Owner As Fighter)
            Monster = eMonster
            Me.Owner = Owner
            Owner.Invocs.Add(Me)
            IsMonster = True
            isInvocation = True
            Starter = False
            Ready = True
            TurnReady = True
        End Sub

#End Region

#Region " Variables "

        Public State As New FighterState
        Public Buffs As New FighterBuffs

        Public Cell As Integer
        Public MapCell As Integer
        Public Team As Integer

        Public Ready, TurnReady As Boolean

        Public LastMove As Integer

        Public NewDeath As Boolean = True
        Public Abandon As Boolean = False

        Public Link As Fighter = Nothing

#End Region

#Region " Spells Used "

        Public Class UsedSpells
            Public Turn As Integer
            Public SpellId As Integer
        End Class

        Public LaunchedSpells As New Dictionary(Of Integer, Integer)
        Public LaunchedTurnSpells As New List(Of UsedSpells)

        Public Sub UpdateTurns()

            Dim ToDelete As New List(Of UsedSpells)

            For Each Turn As UsedSpells In LaunchedTurnSpells
                Turn.Turn -= 1
                If Turn.Turn <= 0 Then ToDelete.Add(Turn)
            Next

            While ToDelete.Count > 0
                LaunchedTurnSpells.Remove(ToDelete(0))
                ToDelete.RemoveAt(0)
            End While

        End Sub

#End Region

#Region " Properties "

        Public ReadOnly Property Id() As Integer
            Get
                If IsMonster Then
                    Return Monster.Id
                Else
                    Return Client.Character.ID
                End If
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                If IsMonster Then
                    Return Monster.Level.TemplateID
                Else
                    Return Client.Character.Name
                End If
            End Get
        End Property

        Public ReadOnly Property StringName() As String
            Get
                If IsMonster Then
                    Return Monster.Level.Template.Nom
                Else
                    Return Client.Character.Name
                End If
            End Get
        End Property

        Public ReadOnly Property Skin() As Integer
            Get
                If IsMonster Then
                    Return Monster.Level.Template.Skin
                Else
                    Return Client.Character.Skin
                End If
            End Get
        End Property

        Public ReadOnly Property Stats() As StatsPlayer
            Get
                If IsMonster Then
                    Return Monster.Stats
                Else
                    Return Character.Player
                End If
            End Get
        End Property

        Public ReadOnly Property GetPattern() As String
            Get
                If IsMonster Then
                    Return Monster.GetBattleGameInfo(Cell, Team)
                Else
                    Return Client.Character.PatternBattle(Cell, Team)
                End If
            End Get
        End Property

#End Region

#Region " Link Properties "

        Public Property PA() As Integer
            Get
                Return Stats.PA
            End Get
            Set(ByVal value As Integer)
                Stats.PA = value
            End Set
        End Property

        Public Property PM() As Integer
            Get
                Return Stats.PM
            End Get
            Set(ByVal value As Integer)
                Stats.PM = value
            End Set
        End Property

        Public Property Life() As Integer
            Get
                Return Stats.Life
            End Get
            Set(ByVal value As Integer)
                Stats.Life = value
            End Set
        End Property

        Public ReadOnly Property Dead() As Boolean
            Get
                If Stats.Life < 0 Then Stats.Life = 0
                Return Stats.Life = 0
            End Get
        End Property

#End Region

#Region " Send Functions "

        Public Sub Send(ByVal Packet As String)

            If IsMonster Or Abandon Then Exit Sub
            Client.Send(Packet)

        End Sub

        Public Sub SendMessage(ByVal Packet As String)

            If IsMonster Or Abandon Then Exit Sub
            Client.SendMessage(Packet)

        End Sub

#End Region

    End Class
End Namespace