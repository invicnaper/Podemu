Imports System.Text
Imports Podemu.Game.Actions
Imports Podemu.Game
Imports Podemu.Utils
Imports System.Threading
Namespace World
    Public Class Fight


#Region " Variables "

        Public Map As Map

        Public Id As Integer
        Public Type As FightType
        Public State As FightState
        Public StartTime As DateTime
        Public TimeOut As Long = 0

        Public Bonus As Integer

        Public Challenges As New List(Of Challenge)

        Private FighterList As New List(Of Fighter)
        Public TurnList As New List(Of Integer)
        Public Places As New Dictionary(Of Integer, List(Of Integer))

        Public MonsterGroup As MonsterGroup

        Public ActualId As Integer = -1

        Public MonsterFreeID As Integer = -1

        Public Blocks(1) As FightBlocks

        Private WithEvents MyTimer As New Timers.Timer

        Private ReadOnly Property PlacesString() As String
            Get
                Dim Pattern As New StringBuilder
                For i As Integer = 0 To 1
                    If i = 1 Then Pattern.Append("|")
                    For Each Cell As Integer In Places(i)
                        Pattern.Append(Cells.GetCellChars(Cell))
                    Next
                Next
                Return Pattern.ToString()
            End Get
        End Property

#End Region

#Region " Initialisation "

        Public Sub New(ByVal Player1 As GameClient, ByVal Player2 As GameClient, ByVal FightType As FightType)

            Map = Player1.Character.GetMap
            Id = Map.NextFightID
            Type = FightType

            If Not InitBattle() Then
                Exit Sub
            End If

            AddFighter(Player1, 0, True)
            AddFighter(Player2, 1, True)

        End Sub

        Public Sub New(ByVal Player As GameClient, ByVal MonsterGroup As MonsterGroup, ByVal FightType As FightType)

            Map = Player.Character.GetMap
            Id = Map.NextFightID
            Type = FightType

            Me.MonsterGroup = MonsterGroup

            If Not InitBattle() Then
                Exit Sub
            End If

            Blocks(1).Basic = True

            AddFighter(Player, 0, True)

            For Each Monster As Monster In MonsterGroup.MonsterList
                Monster.Id = MonsterFreeID
                AddMonster(Monster, 1, MonsterGroup.Cell)
                MonsterFreeID -= 1
            Next

            Bonus = MonsterGroup.BonusPercent

            Map.DelMonsterGroup(MonsterGroup)



        End Sub

        Public Sub AddFighter(ByVal Client As GameClient, ByVal Team As Integer, ByVal Start As Boolean)

            If Not Start Then
                If Blocks(Team).Basic OrElse (Blocks(Team).Party AndAlso _
                        GetFighter(TeamId(Team)).Client.Character.State.InParty AndAlso _
                        GetFighter(TeamId(Team)).Client.Character.State.GetParty.GetCharacter(Client.Character.ID) Is Nothing) Then
                    Client.Send("GA;903;f")
                    Exit Sub
                End If
            End If

            Dim Player As New Fighter(Client)
            Player.Team = Team
            Player.MapCell = Client.Character.MapCell
            Player.Fight = Me
            Player.Starter = Start

            If Not PlacePlayer(Player) Then Exit Sub

            Map.DelCharacter(Client)
            Client.Character.State.InBattle = True
            Client.Character.State.GetFight = Me

            AddPlayer(Player, Team)

        End Sub

        Public Sub AddMonster(ByVal Monster As Monster, ByVal Team As Integer, ByVal Cell As Integer)

            Dim Player As New Fighter(Monster)
            Player.Team = Team
            Player.MapCell = Cell
            Player.Fight = Me

            If Not PlacePlayer(Player) Then Exit Sub

            AddPlayer(Player, Team)

        End Sub

        Public Sub AddInvocation(ByVal Monster As Monster, ByVal Owner As Fighter, ByVal Cell As Integer)

            Monster.Id = MonsterFreeID
            MonsterFreeID -= 1

            Dim Player As New Fighter(Monster, Owner)
            Player.Team = Owner.Team
            Player.Cell = Cell
            Player.MapCell = Cell
            Player.Fight = Me

            AddPlayer(Player, Owner.Team)

            TurnList.Insert(TurnList.IndexOf(ActualId) + 1, Player.Id)

            SendList()

        End Sub

        Private Function InitBattle() As Boolean

            If Not InitializeCells() Then
                Return False
            End If

            For i As Integer = 0 To 1
                Blocks(i) = New FightBlocks
            Next

            Select Case Type
                Case Fight.FightType.Agression, Fight.FightType.PvM
                    TimeOut = Environment.TickCount + 30000
                Case Else
                    TimeOut = -1
            End Select

            MyTimer.Enabled = True

            Return True

        End Function

        Private Sub SendList()
            Dim PacketList As String = "GTL"
            For Each PosId As Integer In TurnList

                Dim p As Fighter = GetFighter(PosId)
                PacketList &= "|" & p.Id

            Next
            Send(PacketList)

        End Sub

        Private Sub SendList(ByVal Client As GameClient)
            Dim PacketList As String = "GTL"
            For Each PosId As Integer In TurnList

                Dim p As Fighter = GetFighter(PosId)
                PacketList &= "|" & p.Id

            Next
            Client.Send(PacketList)

        End Sub

        Private Function InitializeCells() As Boolean

            Places = CellsGenerator.Generate(Map)

            If Places Is Nothing Then
                Return False
            End If

            Return True

        End Function

        Private Sub AddPlayer(ByVal Player As Fighter, ByVal Team As Integer)

            Send("GM|+" & Player.GetPattern)

            FighterList.Add(Player)

            Select Case Type
                Case FightType.Challenge
                    Player.Send("GJK2|1|1|0|0|0")
                Case FightType.PvM
                    Player.Send("GJK2|0|1|0|29999|4")
                Case FightType.Agression
                    Player.Send("GJK2|0|1|0|29999|1")
            End Select

            Map.Send("Gt" & TeamId(Team) & "|+" & Player.Id & ";" & Player.Name & ";" & Player.Stats.Level)
            Player.Send("GP" & PlacesString & "|" & Team)
            Player.Send("GM" & FightersPattern)

        End Sub

        Private Function PlacePlayer(ByVal Player As Fighter) As Boolean
            Dim Possible As List(Of Integer) = (From Item In Places(Player.Team) Select Item Where (GetFighterFromCell(Item) Is Nothing)).ToList
            If Possible.Count > 0 Then
                Player.Cell = Possible(Utils.Basic.Rand(0, Possible.Count - 1))
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub PlacePlayer(ByVal Player As Fighter, ByVal Cell As Integer)
            Dim Possible As List(Of Integer) = (From Item In Places(Player.Team) Select Item Where (GetFighterFromCell(Item) Is Nothing)).ToList
            If Possible.Contains(Cell) Then
                Player.Cell = Cell
                Send("GIC|" & Player.Id & ";" & Cell & ";1")
            End If
        End Sub

#End Region

#Region " Propriétées "


        Public ReadOnly Property Fighters() As List(Of Fighter)
            Get
                Dim NewList As New List(Of Fighter)
                NewList.AddRange(FighterList)
                Return NewList
            End Get
        End Property

        Public ReadOnly Property Pvp() As Boolean
            Get
                Return Type = FightType.Agression OrElse Type = FightType.Challenge
            End Get
        End Property

        Public ReadOnly Property TeamId(ByVal Team As Integer) As Integer
            Get
                Return Fighters(Team).Id
            End Get
        End Property

        Public ReadOnly Property TeamPattern(ByVal Team As Integer) As String
            Get
                Dim Pattern As String = Me.TeamId(Team)
                For Each Fighter As Fighter In Fighters
                    If Fighter.Team = Team Then Pattern &= String.Concat("|+", Fighter.Id, ";", Fighter.Name, ";", Fighter.Stats.Level)
                Next
                Return Pattern
            End Get
        End Property

        Public ReadOnly Property FightersPattern() As String
            Get
                Dim Pattern As New StringBuilder()
                For Each Fighter As Fighter In Fighters
                    Pattern.Append("|+" & Fighter.GetPattern)
                Next
                Return Pattern.ToString()
            End Get
        End Property

        Public ReadOnly Property BladesPattern() As String
            Get
                Return String.Format("{0};{1}|{2};{3};{4};{5}|{6};{7};{8};{9}", _
                    Id, CInt(Type), _
                    TeamId(0), Fighters(0).MapCell, If(Fighters(0).IsMonster, 1, 0), If(Type = FightType.Agression, Fighters(0).Client.Character.Player.Alignment.Id, -1), _
                    TeamId(1), Fighters(1).MapCell, If(Fighters(1).IsMonster, 1, 0), If(Type = FightType.Agression, Fighters(1).Client.Character.Player.Alignment.Id, -1))
            End Get
        End Property

        Public ReadOnly Property ActualPlayer() As Fighter
            Get
                Return GetFighter(ActualId)
            End Get
        End Property

#End Region

#Region " Functions "

        Public Function GetFighter(ByVal Id As Integer) As Fighter
            For Each Player As Fighter In Fighters
                If Player.Id = Id Then Return Player
            Next
            Return Nothing
        End Function

        Public Function GetFighterFromCell(ByVal Cell As Integer) As Fighter

            For Each Player As Fighter In Fighters
                If Player.Cell = Cell AndAlso Not Player.Dead AndAlso Not Player.Abandon Then Return Player
            Next
            Return Nothing

        End Function

        Private Function AllReady() As Boolean
            For Each Fighter As Fighter In Fighters
                If Fighter.Ready = False Then Return False
            Next
            Return True
        End Function

        Public Function AllTurnReady() As Boolean
            For Each Player As Fighter In Fighters
                If Not Player.IsMonster AndAlso Not Player.TurnReady AndAlso Not Player.Abandon Then Return False
            Next
            Return True
        End Function

        Public Function GetTeam(ByVal Id As Integer) As Integer
            Dim FighterList As List(Of Fighter) = Fighters
            If Id = FighterList(0).Id Then
                Return 0
            ElseIf (Not FighterList(1).IsMonster) Then
                If Id = FighterList(1).Id Then
                    Return 1
                Else
                    Return -1
                End If
            Else
                Return -1
            End If
        End Function

        Public Function WaitingPlayers() As String
            Dim List As String = ""
            Dim First As Boolean = True
            For Each Player As Fighter In Fighters
                If Not Player.IsMonster AndAlso Not Player.TurnReady AndAlso Not Player.Abandon Then
                    If Not First Then
                        List &= ", "
                    Else : First = False
                    End If
                    List &= Player.StringName
                End If
            Next
            Return List
        End Function

        Public Function GetEnnemyNearCell(ByVal TheClient As Fighter, ByVal Cell As Integer) As Fighter

            Dim BestEnemy As Fighter = Nothing
            Dim BestAgility = 0

            For i As Integer = 1 To 7 Step 2
                Dim CellToCheck As Integer = Utils.Cells.NextCell(Map, Cell, i)
                Dim Ennemy As Fighter = GetFighterFromCell(CellToCheck)
                If Ennemy IsNot Nothing AndAlso Ennemy.Team <> TheClient.Team Then
                    If Ennemy.Stats.Stats.Base.Agilite.Total > BestAgility Then
                        BestEnemy = Ennemy
                        BestAgility = Ennemy.Stats.Stats.Base.Agilite.Total
                    End If
                End If
            Next
            Return BestEnemy
        End Function

        Public Sub GenerateList()

            Dim ListToClear As List(Of Fighter) = Fighters.OrderByDescending(Function(f) f.Stats.Initiative).ToList()

            If ListToClear.Count < 2 Then Exit Sub

            Dim Player As Fighter = ListToClear(0)
            TurnList.Add(Player.Id)
            ListToClear.Remove(Player)

            Do While ListToClear.Count > 0

                Player = ListToClear.FirstOrDefault(Function(f) f.Team <> Player.Team)

                If Player IsNot Nothing Then
                    TurnList.Add(Player.Id)
                    ListToClear.Remove(Player)
                Else
                    Do While ListToClear.Count > 0
                        Player = ListToClear(0)
                        TurnList.Add(Player.Id)
                        ListToClear.Remove(Player)
                    Loop
                End If
            Loop

        End Sub

        Public Function IsValidCell(ByVal Cell As Integer) As Boolean
            If Not Map.IsCellReachable(Cell) Then Return False
            If Not GetFighterFromCell(Cell) Is Nothing Then Return False
            Return True
        End Function

        Public Sub SendAccountsStats()

            For Each Player As Fighter In Fighters
                If Not Player.IsMonster Then
                    Player.Client.Character.SendAccountStats()
                End If
            Next

        End Sub

#End Region

#Region " State functions "

        Public Sub Start(ByVal Force As Boolean)

            If State <> FightState.Starting OrElse (Not AllReady() And Not Force) Then Exit Sub

            State = FightState.None
            Map.Send("Gc-" & Id)


            If Type = FightType.PvM Then

                Dim Challenge As Challenge = ChallengeHandler.GetRandomChallenge(Me)
                While Not Challenge.CanSet()
                    Challenge = ChallengeHandler.GetRandomChallenge(Me)
                End While
                Challenges.Add(Challenge)

                For Each Fighter In Fighters.Where(Function(f) Not f.IsMonster And Not f.Abandon)
                    For Each Challenge In Challenges
                        Challenge.Show(Fighter.Client)
                    Next
                Next
            End If

            StartTime = DateTime.Now
            GenerateList()

            Dim PacketPos As String = "GIC"
            Dim PacketList As String = "GTL"
            For Each PosId As Integer In TurnList

                Dim Player As Fighter = GetFighter(PosId)
                PacketPos &= "|" & Player.Id & ";" & Player.Cell & ";1"
                PacketList &= "|" & Player.Id

                Player.PA = Player.Stats.MaxPA
                Player.PM = Player.Stats.MaxPM

                Player.Stats.Stats.ResetBonus()
                Player.State.Reset()

            Next
            Send(PacketPos)
            Send("GS")
            Send(PacketList)

            ActualId = TurnList(TurnList.Count - 1)

            NextPlayer(False)

        End Sub

        Public Sub Join(ByVal Client As GameClient, ByVal Spect As Boolean)

            Client.Character.GetMap.DelCharacter(Client)

            If Not Spect Then
                Dim Fighter = Client.Character.State.GetFighter
                Fighter.Client = Client
                SendMessage(Client.Character.Name & " vient de se reconnecter en combat")
            End If

            Select Case Type
                Case FightType.Challenge
                    Client.Send("GJK2|1|1|" & If(Spect, "1", "0") & "|0|0")
                Case FightType.PvM
                    Client.Send("GJK2|0|1|" & If(Spect, "1", "0") & "|0|4")
                Case FightType.Agression
                    Client.Send("GJK2|0|1|" & If(Spect, "1", "0") & "|0|1")
            End Select

            Client.Send("GM" & FightersPattern)

            For Each Challenge In Challenges
                Challenge.Show(Client)
            Next

            Dim PacketPos As String = "GIC"
            Dim PacketList As String = "GTL"
            For Each PosId As Integer In TurnList
                Dim Player As Fighter = GetFighter(PosId)
                PacketPos &= "|" & Player.Id & ";" & Player.Cell & ";1"
                PacketList &= "|" & Player.Id
            Next

            Client.Send(PacketPos)
            Client.Send("GS")
            Client.Send(PacketList)

            Client.Send("GTS" & ActualId & "|29000")

        End Sub

        Private Sub SetNextId()

            For i As Integer = 0 To TurnList.Count - 1
                If TurnList(i) = ActualId Then
                    If i = TurnList.Count - 1 Then
                        ActualId = TurnList(0)
                    Else
                        ActualId = TurnList(i + 1)
                    End If
                    Exit For
                End If
            Next

            If ActualPlayer.Dead Then
                SetNextId()
                Exit Sub
            End If

        End Sub

        Public Sub NextPlayer(ByVal Force As Boolean)

            If Me.State = FightState.Result Then Exit Sub

            If Not AllTurnReady() Then
                If Force Then
                    SendMessage("En attente des joueurs : " & WaitingPlayers())
                Else
                    Exit Sub
                End If
            End If

            State = FightState.None

            SetNextId()

            MiddleTurn()

        End Sub

        Private Sub MiddleTurn()

            Dim iPlayer As Fighter = ActualPlayer

            'iPlayer.UpdateTurns()
            'iPlayer.State.Refresh(ActualPlayer)

            'iPlayer.PA = iPlayer.Stats.MaxPA
            'iPlayer.PM = iPlayer.Stats.MaxPM

            iPlayer.Buffs.Execute()
            UpdateGlyphs()
            If WalkOnGlyph(iPlayer.Cell) Then UseGlyphsEffects()

            If ActualPlayer.Dead Then NextPlayer(True)

            Dim PacketMiddle As String = "GTM"
            For Each Player As Fighter In Fighters
                PacketMiddle &= "|" & Player.Id & ";"
                If Player.Dead Then
                    PacketMiddle &= "1;"
                Else
                    PacketMiddle &= "0;" & Player.Life & ";"
                    PacketMiddle &= Player.PA
                    PacketMiddle &= ";" & Player.PM & ";"
                    PacketMiddle &= If(Player.State.Has(FighterState.State.Invisible), "", Player.Cell.ToString) & ";;"
                    PacketMiddle &= Player.Stats.MaximumLife
                End If
            Next
            Send(PacketMiddle)

            SendAccountsStats()

            StartTurn()

        End Sub

        Private Sub StartTurn()

            Send("GTS" & ActualId & "|29000")

            TimeOut = Environment.TickCount + 29000

            Dim ActualFighter As Fighter = ActualPlayer
            ActualFighter.LaunchedSpells.Clear()
            ActualFighter.LastMove = -1
            State = FightState.Playing

            If Type = FightType.PvM And Not ActualFighter.IsMonster Then
                For Each Challenge In Challenges
                    Challenge.BeginTurn(ActualFighter)
                Next
            End If

            If ActualFighter.IsMonster Then
                Dim IA As New MonsterIA(Me, ActualFighter)
                IA.ApplyThread()
                'FinishTurn()
            End If

        End Sub

        Public Sub FinishTurn()

            If Me.State = FightState.Result Then Exit Sub

            ActualPlayer.UpdateTurns()
            ActualPlayer.State.Refresh(ActualPlayer)

            ActualPlayer.PA = ActualPlayer.Stats.MaxPA
            ActualPlayer.PM = ActualPlayer.Stats.MaxPM

            If ActualPlayer.IsDeco Then
                ActualPlayer.DecoTurn += 1
                If ActualPlayer.DecoTurn = 20 Then
                    Abandon(ActualPlayer)
                Else
                    SendMessage(20 - ActualPlayer.DecoTurn & " avant la deconnection de " & ActualPlayer.Name)
                End If
            End If

            State = FightState.None

            Send("GTF" & ActualId)

            SendAccountsStats()
            Send("GTR" & ActualId)

            TimeOut = Environment.TickCount + 5000
            For Each Player As Fighter In Fighters
                Player.TurnReady = False
            Next

            State = FightState.WaitTurn

        End Sub

        Public Sub VerifyPlayers()
            For Each Player As Fighter In Fighters
                If Player.NewDeath AndAlso Player.Dead Then
                    Send("GA;103;" & ActualPlayer.Id & ";" & Player.Id)
                    Player.NewDeath = False

                    If Player.isInvocation Then Player.Owner.Invocs.Remove(Player)

                    For Each Invoc In Player.Invocs
                        Invoc.Life = 0
                    Next

                    Player.Invocs.Clear()

                    VerifyPlayers()

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.CheckDeath(Player)
                        Next
                    End If

                End If
            Next
        End Sub

#End Region

#Region " Actions functions "

        Private Function TryTacle(ByVal Player As Fighter) As Boolean

            Dim TacleFighter As Fighter = GetEnnemyNearCell(Player, Player.Cell)

            If TacleFighter IsNot Nothing Then

                Dim Chance As Integer = Formulas.ChanceTacle(Player, TacleFighter)
                If Basic.Rand(0, 99) > Chance Then

                    Dim Id As Integer = Player.Id

                    Dim PertePA As Integer = Player.PA * (Chance / 100)
                    If PertePA > Player.PA Then PertePA = Player.PA

                    Send("GA;104;" & Id)
                    Send("GA;102;" & Id & ";" & Id & ",-" & PertePA)
                    Send("GA;129;" & Id & ";" & Id & ",-" & Player.PM)

                    Player.PM = 0
                    Player.PA -= PertePA

                    Return False

                End If

            End If

            Return True

        End Function

        Public Function PlayerMove(ByVal Player As Fighter, ByVal Cells As String, ByVal Distance As Integer) As Boolean

            If Distance <= Player.PM Then

                If TryTacle(Player) Then

                    Dim NewCell As Integer = Utils.Cells.GetCellNum(Cells.Substring(Cells.Length - 2))

                    Dim ActualBattleCell As String = Utils.Cells.GetCellChars(Player.Cell)

                    Dim tempCell = Player.Cell
                    Player.PM -= Distance
                    Player.LastMove = Distance
                    Player.Cell = NewCell

                    If Player.State.Has(FighterState.State.Porteur) Then
                        Player.Link.Cell = NewCell
                    ElseIf Player.State.Has(FighterState.State.Porte) Then

                        Dim Porteur As Fighter = Player.Link

                        Porteur.State.Del(Porteur, FighterState.State.Porteur)
                        Porteur.Link = Nothing
                        Player.State.Del(Player, FighterState.State.Porte)
                        Player.Link = Nothing

                    End If

                    Player.Send("GAS" & Player.Id)
                    Dim Deplacement As String = "GA0;1;" & Player.Id & ";a" & ActualBattleCell & Cells
                    If Player.State.Has(FighterState.State.Invisible) Then
                        Player.Send(Deplacement)
                    Else
                        Send(Deplacement)
                    End If

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.CheckMovement(tempCell, NewCell, Distance)
                        Next
                    End If

                    Return True

                Else

                    Return False

                End If

            End If

        End Function

        Public Function CanLaunchSpell(ByVal Player As Fighter, ByVal Spell As SpellLevel, ByVal Cell As Integer) As Boolean

            If Player.PA < Spell.CostPA Then Return False

            If Not Map.IsCellReachable(Cell) Then Return False

            If Spell.InLine And (Not Cells.InLine(Map, Player.Cell, Cell)) Then Return False
            Dim Distance As Integer = Utils.Cells.GoalDistance(Map, Player.Cell, Cell)

            Dim MaxiPO As Integer = Spell.MaxPO
            If Spell.ModifablePO Then
                MaxiPO += Player.Stats.Stats.PO.BaseTotal
                If (MaxiPO - Spell.MinPO) < 1 Then MaxiPO = Spell.MinPO + 1
            End If
            If Distance < Spell.MinPO Then Return False
            If Distance > MaxiPO Then Return False

            If Spell.MaxPerTurn > 0 AndAlso Player.LaunchedSpells.ContainsKey(Spell.This.SpellId) Then
                If Player.LaunchedSpells(Spell.This.SpellId) >= Spell.MaxPerTurn Then
                    Return False
                End If
            End If

            For Each SpellTurn As Fighter.UsedSpells In Player.LaunchedTurnSpells
                If SpellTurn.SpellId = Spell.This.SpellId Then
                    Return False
                End If
            Next

            If Spell.HasEffect(Effect.Invocation) And (Player.Invocs.Count >= Player.Stats.Stats.MaxInvocation.Total OrElse Not IsValidCell(Cell)) Then Return False

            If Distance <> 0 AndAlso Spell.LineOfVision AndAlso Not Cells.CheckView(Player.Fight, Player.Cell, Cell) Then Return False

            Return True

        End Function

        Public Sub LaunchSpell(ByVal Player As Fighter, ByVal Spell As SpellLevel, ByVal Cell As Integer, Optional ByVal Trap As Boolean = False)

            If Me.State = FightState.Result Then Exit Sub

            If Trap OrElse CanLaunchSpell(Player, Spell, Cell) Then

                If Not Trap Then Player.PA -= Spell.CostPA
                Dim Id As Integer = Player.Id

                If Not Trap Then Player.Send("GAS" & Id)

                Dim IsEC As Boolean = False
                Dim TauxEC As Integer = Spell.TauxEC - Player.Stats.Stats.Damages.BonusEchecCritique.BaseTotal
                If TauxEC < 2 Then TauxEC = 2
                If (Not Trap) And Spell.TauxEC <> 0 And (Basic.Rand(1, Spell.TauxEC) = 1) Then
                    IsEC = True
                End If

                If Not IsEC Then

                    If Player.LaunchedSpells.ContainsKey(Spell.This.SpellId) Then
                        Player.LaunchedSpells(Spell.This.SpellId) += 1
                    Else
                        Player.LaunchedSpells.Add(Spell.This.SpellId, 1)
                    End If

                    If Spell.TurnNumber > 0 Then
                        Dim NewUsed As New Fighter.UsedSpells
                        NewUsed.Turn = Spell.TurnNumber - 2
                        NewUsed.SpellId = Spell.This.SpellId
                        Player.LaunchedTurnSpells.Add(NewUsed)
                    End If

                    If Not Trap Then Send("GA;300;" & Id & ";" & Spell.This.SpellId & "," & Cell & "," & Spell.This.AnimationId & "," & Spell.MyLevel & "," & Spell.This.SpriteInfos)

                    Dim IsCC As Boolean = False
                    If Spell.TauxCC <> 0 AndAlso (Spell.CriticEffectList.Count > 0) Then
                        Dim TauxCC As Integer = Spell.TauxCC - Player.Stats.Stats.Damages.BonusCoupCritique.BaseTotal
                        If TauxCC < 2 Then TauxCC = 2
                        If (Basic.Rand(1, TauxCC) = 1) Then
                            IsCC = True
                            If Not Trap Then Send("GA;301;" & Id & ";" & Spell.This.SpellId)
                        End If
                    End If

                    Dim ListEffect As List(Of SpellEffect) = If(IsCC, Spell.CriticEffectList, Spell.EffectList)

                    Dim EffectNum As Integer = 0
                    Dim ActualChance As Integer = 0
                    For Each Effect As SpellEffect In ListEffect

                        If (Effect.Chance > 0) Then
                            If Basic.Rand(1, 100) > (Effect.Chance + ActualChance) Then
                                ActualChance += Effect.Chance
                                Continue For
                            End If
                            ActualChance -= 100
                        End If

                        Dim PorteeType As String = If(IsCC, _
                            Spell.TypePortee.Substring(Spell.EffectList.Count * 2), _
                            Spell.TypePortee.Substring(0, Spell.EffectList.Count * 2))
                        Dim EffetPortee As String = PorteeType.Substring(EffectNum * 2, 2)

                        Dim Cibles As List(Of Fighter) = Effect.Cibles.RemixCibles(Player, Zone.GetCells(Me, Cell, Player.Cell, EffetPortee))

                        Effect.UseEffect(Player, Cibles, Cell, Trap)

                        EffectNum += 1

                        If Type = FightType.PvM And Not Player.IsMonster Then
                            For Each Challenge In Challenges
                                Challenge.CheckSpell(Player, Effect, Cibles, Cell)
                            Next
                        End If

                        VerifyPlayers()

                    Next

                Else
                    Send("GA;302;" & Id & ";" & Spell.This.SpellId)
                End If

                If Not Trap Then Send("GA;102;" & Id & ";" & Id & ",-" & Spell.CostPA)
                If Not Trap Then Player.Send("GAF0|" & Id)

                If OnlyOneTeam() Then

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.EndTurn(Player)
                        Next
                    End If

                    EndFight()
                    Exit Sub
                End If

                If Spell.ECEndTurn And IsEC Then

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.EndTurn(Player)
                        Next
                    End If

                    FinishTurn()
                    Exit Sub
                End If

            Else
                Player.SendMessage("Vous n'avez pas la ligne de vue nécessaire")
            End If

        End Sub

        Private Function CanUseWeapon(ByVal Client As Fighter, ByVal Item As Item, ByVal Cell As Integer) As Boolean

            Dim Template As ItemTemplate = Item.GetTemplate

            If Not Map.IsCellReachable(Cell) Then Return False
            If Client.PA < Template.CostPA Then Return False
            Dim Distance As Integer = Utils.Cells.GoalDistance(Map, Client.Cell, Cell)
            If Distance < Template.MinPO Then Return False
            If Distance > Template.MaxPO Then Return False

            If Distance <> 0 AndAlso Not Cells.CheckView(Client.Fight, Client.Cell, Cell) Then Return False

            Return True

        End Function

        Public Sub UseWeapon(ByVal Player As Fighter, ByVal Item As Item, ByVal Cell As Integer)

            If Me.State = FightState.Result Then Exit Sub

            If CanUseWeapon(Player, Item, Cell) Then

                Dim ItemTemp As ItemTemplate = Item.GetTemplate
                Dim Id As Integer = Player.Id

                Player.PA -= ItemTemp.CostPA

                Player.Send("GAS" & Id)

                Dim IsEC As Boolean = False
                Dim TauxEc As Integer = ItemTemp.TauxEC - Player.Stats.Stats.Damages.BonusEchecCritique.Base
                If TauxEc < 2 Then TauxEc = 2
                If ItemTemp.TauxEC <> 0 AndAlso (Basic.Rand(1, TauxEc) = 1) Then
                    IsEC = True
                End If

                If Not IsEC Then

                    Send("GA;303;" & Id & ";" & Cell)

                    Dim IsCC As Boolean = False
                    If ItemTemp.TauxCC <> 0 Then
                        Dim TauxCC As Integer = ItemTemp.TauxCC - Player.Stats.Stats.Damages.BonusCoupCritique.BaseTotal
                        If TauxCC < 2 Then TauxCC = 2
                        If (Basic.Rand(1, TauxCC) = 1) Then
                            IsCC = True
                            Send("GA;301;" & Id & ";0")
                        End If
                    End If

                    Dim ListEffect As List(Of SpellEffect) = Item.GetEffectList

                    If IsCC Then
                        For Each tEffect As SpellEffect In ListEffect
                            If tEffect.IsDegatsEffect Then
                                If tEffect.Value2 = -1 Then
                                    tEffect.Value += ItemTemp.BonusCC
                                    tEffect.Str = "0d0+" & tEffect.Value
                                Else
                                    tEffect.Value += ItemTemp.BonusCC
                                    tEffect.Value2 += ItemTemp.BonusCC
                                    Dim Max As Integer = tEffect.Value2
                                    Dim Min As Integer = tEffect.Value
                                    tEffect.Str = "1d" & (Max - Min + 1) & "+" & (Min - 1)
                                End If
                            End If
                        Next
                    End If

                    For Each Effect As SpellEffect In ListEffect

                        Dim Cibles As List(Of Fighter) = Effect.Cibles.RemixCibles(Player, Zone.GetCells(Me, Cell, Player.Cell, ItemTemp.Portee))

                        Effect.UseEffect(Player, Cibles, Cell, False)

                        VerifyPlayers()

                    Next

                Else
                    Send("GA;305;" & Id & ";")
                End If

                Send("GA;102;" & Id & ";" & Id & ",-" & ItemTemp.CostPA)
                Send("GAF0|" & Id)

                If OnlyOneTeam() Then

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.EndTurn(Player)
                        Next
                    End If

                    EndFight()
                    Exit Sub
                End If

                If IsEC Then

                    If Type = FightType.PvM And Not Player.IsMonster Then
                        For Each Challenge In Challenges
                            Challenge.EndTurn(Player)
                        Next
                    End If

                    FinishTurn()
                    Exit Sub
                End If

            End If

        End Sub

#End Region

#Region " End functions "

        Public Function OnlyOneTeam() As Boolean

            Dim Team1 As Boolean = False
            Dim Team2 As Boolean = False

            For Each Player As Fighter In Fighters
                If (Player.Team = 0) And (Not Player.Dead) And (Not Player.Abandon) Then Team1 = True
                If (Player.Team = 1) And (Not Player.Dead) And (Not Player.Abandon) Then Team2 = True
            Next

            Return Not (Team1 And Team2)

        End Function

        Private Function GetWinTeam() As Integer

            Return If(Fighters.Where(Function(f) f.Team = 0).All(Function(f) f.Dead OrElse f.Abandon), 1, 0)

        End Function

        Private Function GetLooseTeam() As Integer

            Return If(Fighters.Where(Function(f) f.Team = 0).All(Function(f) f.Dead OrElse f.Abandon), 0, 1)

        End Function

        Private Function GetLosersLevels(ByVal WinTeam As Integer) As Integer()

            Return (From Item As Fighter In Fighters Where Item.Team <> WinTeam Select Item.Stats.Level).ToArray

        End Function

        Private Function GetLosersTotalLevel(ByVal WinTeam As Integer) As Integer

            Dim Total As Integer = 0
            For Each Loose As Integer In GetLosersLevels(WinTeam)
                Total += Loose
            Next
            Return Total

        End Function

        Private Function GetWinnersLevels(ByVal WinTeam As Integer) As Integer()

            Return (From Item As Fighter In Fighters Where Item.Team = WinTeam Select Item.Stats.Level).ToArray

        End Function

        Private Function GetWinnersTotalLevel(ByVal WinTeam As Integer) As Integer

            Dim Total As Integer = 0
            For Each Win As Integer In GetWinnersLevels(WinTeam)
                Total += Win
            Next
            Return Total

        End Function

        Private Function GetWinNumber(ByVal WinTeam As Integer) As Integer

            Return (From Item As Fighter In Fighters Where Item.Team = WinTeam Select Item.Stats.Level).Count

        End Function

        Private Function GetMaxLevel(ByVal WinTeam As Integer) As Integer

            Dim Maximum As Integer = 1
            For Each Player As Fighter In Fighters
                If Player.Team = WinTeam AndAlso Player.Stats.Level > Maximum Then Maximum = Player.Stats.Level
            Next
            Return Maximum

        End Function

        Public Sub EndFight()

            EndFightThread()

        End Sub

        Public Function GeneratePacket(ByVal WinTeam As Integer, ByVal Player As Fighter, ByVal Items As IEnumerable(Of Item), ByVal Kamas As Integer, ByVal Xp As Long, ByVal HonorXp As Integer, ByVal MountXp As Integer, ByVal GuildXp As Integer) As String


            Dim Packet As String = String.Concat("|", If(WinTeam = Player.Team, "2", "0"), ";", Player.Id, _
            ";", Player.Name, ";", Player.Stats.Level, ";", If(Player.Dead, "1", "0"))


            Dim MyDrops As String = String.Join(",", Items.Select(Function(i) i.TemplateID & "~" & i.Quantity))


            Select Case Type


                Case FightType.PvM

                    If Not Player.IsMonster Then
                        Packet &= String.Concat(";", ExperienceTable.AtLevel(Player.Stats.Level).Character, _
                         ";", Player.Stats.Exp, _
                       ";", ExperienceTable.AtLevel(Player.Stats.Level + 1).Character, _
                        ";", Xp, _
                        ";", GuildXp, _
                         ";", MountXp, _
                        ";", MyDrops, _
                         ";", Kamas)
                    Else
                        Packet &= String.Concat(";", _
                          ";", _
                        ";", _
                         ";", Xp, _
                         ";", _
                          ";", _
                         ";", MyDrops, _
                          ";", Kamas)
                    End If

                Case FightType.Agression

                    If Player.Stats.Alignment.Enabled Then
                        Packet &= String.Concat(";", ExperienceTable.AtLevel(Player.Stats.Alignment.Rank).Pvp, _
                        ";", Player.Stats.Alignment.Exp, _
                        ";", ExperienceTable.AtLevel(Player.Stats.Alignment.Rank + 1).Pvp, _
                         ";", HonorXp)
                    Else
                        Packet &= ";;;;"
                    End If

                    Packet &= String.Concat(";", Player.Stats.Alignment.Rank, ";0;100", ";", MyDrops, ";", Kamas)

                    If Not Player.IsMonster Then
                        Packet &= String.Concat(";", ExperienceTable.AtLevel(Player.Stats.Level).Character, _
                       ";", Player.Stats.Exp, _
                        ";", ExperienceTable.AtLevel(Player.Stats.Level + 1).Character, _
                      ";", Xp)
                    Else
                        Packet &= ";;;;" & Xp
                    End If

            End Select

            Return Packet

        End Function

        Dim ItemLoots As New List(Of Item)
        Dim KamasLoots As Long = 0
        Dim ExpLoots As Long = 0

        Public Sub EndFightThread()

            If Me.State = FightState.Result Then Exit Sub

            Me.State = FightState.Result
            MyTimer.Enabled = False
            MyTimer.Dispose()
            MyTimer = Nothing

            If Type = FightType.PvM Then
                For Each Challenge In Challenges
                    If Challenge.State And Not Challenge.Signaled Then
                        Challenge.Ok()
                    End If
                Next
            End If

            Dim WinChallenges = Challenges.Where(Function(c) c.State).ToList()
            Dim ChallengeXpBonus = 1
            Dim ChallengeDropBonus = 1
            If WinChallenges.Count > 0 Then
                ChallengeXpBonus = Math.Round((100 + WinChallenges.Sum(Function(c) c.BasicXpBonus + c.TeamXpBonus)) / 100)
                ChallengeDropBonus = Math.Round((100 + WinChallenges.Sum(Function(c) c.BasicDropBonus + c.TeamDropBonus)) / 100)
            End If

            'Remove Invocations
            FighterList.Remove(Function(f) f.isInvocation)

            Dim WinTeam As Integer = GetWinTeam()
            Dim LooseTeam As Integer = GetLooseTeam()
            Dim PresentFighters As List(Of Fighter) = Fighters.Where(Function(f) f.IsMonster OrElse Not f.Abandon).ToList()
            Dim WinnersPlayers As IEnumerable(Of Fighter) = Fighters.Where(Function(f) Not f.IsMonster AndAlso f.Team = WinTeam)
            Dim LoosersPlayers As IEnumerable(Of Fighter) = Fighters.Where(Function(f) Not f.IsMonster AndAlso f.Team = LooseTeam AndAlso Not f.Abandon)
            Dim WinnersMonsters As IEnumerable(Of Fighter) = Fighters.Where(Function(f) f.IsMonster AndAlso f.Team = WinTeam)
            Dim LoosersMonsters As IEnumerable(Of Fighter) = Fighters.Where(Function(f) f.IsMonster AndAlso f.Team = LooseTeam)
            Dim LosersLevels() As Integer = GetLosersLevels(WinTeam)
            Dim MaxLevel As Integer = GetMaxLevel(WinTeam)
            Dim TotalLevelLoosers As Integer = GetLosersTotalLevel(WinTeam)
            Dim TotalLevelWinners As Integer = GetWinnersTotalLevel(WinTeam)
            Dim WinnerPP = WinnersPlayers.Sum(Function(f) f.Stats.Prospection)
            Dim IsHeroic As Boolean = Config.GetItem(Of Boolean)("ACTIVE_HEROIC")
            Dim BattleTime As Long = DateTime.Now.Subtract(StartTime).TotalMilliseconds
            Dim Packet As New StringBuilder("GE" & BattleTime & ";" & If(MonsterGroup IsNot Nothing, Bonus.ToString(), "") & "|" & Id & "|" & If(Type = FightType.Agression, "1", "0"))

            If IsHeroic Then
                'looser player loot
                For Each Fighter As Fighter In LoosersPlayers

                    'Items
                    For Each Item As Item In Fighter.Character.Items.ListOfItems
                        Item.Position = -1
                        ItemLoots.Add(Item)
                    Next
                    Fighter.Character.Items.ClearItems()

                    'Sac
                    For Each Item As Item In Fighter.Character.MerchantBag.ListOfItems
                        Item.Position = -1
                        ItemLoots.Add(Item.Copy())
                    Next
                    Fighter.Character.MerchantBag.ClearItems()

                    'Kamas
                    KamasLoots += Fighter.Character.Player.Kamas
                    Fighter.Character.Player.Kamas = 0

                    'Xp
                    If Type = FightType.Agression Then
                        ExpLoots += Math.Round(Fighter.Character.Player.Exp / 10)
                    End If
                Next

                'Monster loots
                If WinnersMonsters.Count = 0 Then
                    ItemLoots.AddRange(MonsterGroup.Items)
                    MonsterGroup.Items.Clear()
                    KamasLoots += MonsterGroup.Kamas
                    MonsterGroup.Kamas = 0
                End If

            End If

            'Monsters loots
            For Each Fighter In Fighters.Where(Function(f) f.IsMonster AndAlso f.Team = LooseTeam)
                Dim template = Fighter.Monster.Level.Template
                KamasLoots += Math.Round(Basic.Rand(template.MinKamas, template.MaxKamas) * If(IsHeroic, 3, 1) * ChallengeDropBonus * Config.GetItem(Of Double)("DROPS_RATE") * (CDbl(100 + MonsterGroup.BonusPercent) / 100))
                ItemLoots.AddRange(DropsManager.GetDrops(WinnerPP, Fighter.Monster.Level.Template, Utils.Config.GetItem("DROPS_RATE") * If(IsHeroic, 3, 1) * ChallengeDropBonus * (CDbl(100 + MonsterGroup.BonusPercent) / 100)))
            Next

            Dim count As Integer = 0
            Dim count2 As Integer = 0
            For Each Player As Fighter In PresentFighters

                'Experience
                Dim ExpWin As Long = 0
                Dim MountXp As Long = 0
                Dim GuildXp As Long = 0
                Dim HonorExp As Integer = 0
                Dim KamasWin As Integer = 0
                Dim ItemsWin As New List(Of Item)

                If Player.Team = WinTeam And Not Player.IsMonster And Not Player.Abandon Then

                    If IsHeroic Then
                        ExpWin += Math.Round(ExpLoots / WinnersPlayers.Count)
                    End If

                    If Type = FightType.PvM Then
                        ExpWin += Formulas.PvmExp(LoosersMonsters.ToArray(), WinnersPlayers.ToArray(), Player, GetMaxLevel(WinTeam))
                        ExpWin *= (CDbl(100 + MonsterGroup.BonusPercent) / 100)
                        ExpWin *= ChallengeXpBonus
                    End If

                    If Player.Character.niveauMax <> 0 AndAlso Player.Character.niveauMax > Player.Stats.Level Then
                        ExpWin *= 6
                    End If

                    If Player.Client.Character.State.IsMounted Then
                        MountXp += Formulas.MountExp(Player, ExpWin, Player.Client.Character.Mount)
                        Player.Client.Character.Mount.Experience += MountXp
                    End If

                    If Player.Stats.Level < ExperienceTable.MaxLevel Then
                        Player.Stats.Exp += ExpWin
                        Player.Client.Character.LevelUp()
                    End If

                    KamasWin = Math.Round(KamasLoots * ((Player.Stats.Prospection) / CDbl(WinnerPP)))
                    Player.Stats.Kamas += KamasWin

                    If Type = FightType.Agression Then
                        If Player.Character.Player.Alignment.Enabled Then
                            HonorExp = Formulas.HonorExp(Player.Stats.Level, TotalLevelWinners, TotalLevelLoosers)
                            Player.Stats.Alignment.AddExp(Player.Client, HonorExp)
                        End If
                    End If

                    ItemsWin = GetItems((Player.Stats.Prospection) / WinnerPP, ItemLoots, count + 1 - WinnersPlayers.Count)
                    DropsManager.Merge(ItemsWin)
                    For Each Item As Item In ItemsWin
                        Player.Character.Items.AddItem(Player.Client, Item)
                    Next
                    count += 1

                ElseIf Player.Team = LooseTeam AndAlso Not Player.IsMonster Then

                    If Type = FightType.Agression AndAlso Player.Character.Player.Alignment.Enabled Then
                        Dim ExpLost = Math.Round(Formulas.HonorLooseExp(Player.Stats.Level, TotalLevelWinners, TotalLevelLoosers) / 2)
                        Player.Stats.Alignment.RemExp(Player.Client, ExpLost)
                        HonorExp = -ExpLost
                    End If

                    If Type = FightType.PvM Then
                        ExpWin = Formulas.PvmExp(WinnersMonsters.Where(Function(f) f.Dead), LoosersPlayers, Player, WinnersMonsters.Where(Function(f) f.Dead).Max(Function(f) f.Stats.Level)) / 10

                        If Player.Stats.Level < ExperienceTable.MaxLevel Then
                            Player.Stats.Exp += ExpWin
                            Player.Client.Character.LevelUp()
                        End If
                    End If

                ElseIf Player.IsMonster AndAlso Player.Team = WinTeam And IsHeroic Then

                    KamasWin = Math.Round(KamasLoots / WinnersMonsters.Count)
                    MonsterGroup.Kamas += KamasWin

                    ItemsWin = GetItems(100 / WinnersMonsters.Count, ItemLoots, count2 + 1 - WinnersMonsters.Count)
                    DropsManager.Merge(ItemsWin)
                    MonsterGroup.Items.AddRange(ItemsWin)
                    count2 += 1
                End If

                Packet.Append(GeneratePacket(WinTeam, Player, ItemsWin, KamasWin, ExpWin, HonorExp, MountXp, GuildXp))
                Player.WaitingResult = True
            Next

            Send(Packet.ToString())

            FightsHandler.RemoveFight(Me)

            For Each Player As Fighter In Fighters.Where(Function(f) f.IsMonster Or Not f.Abandon)

                If Not Player.IsMonster Then

                    Player.Client.State.State = GameState.WaitingPacket.Create
                    Player.Stats.Stats.ResetBonus()
                    Player.PM = Player.Stats.MaxPM
                    Player.PA = Player.Stats.MaxPA
                    If Player.Life = 0 Then Player.Life = 1

                    If Type <> FightType.Challenge Then
                        If (Player.Dead And Player.Team = LooseTeam) Then
                            Player.Character.Dead()
                            Continue For
                        End If

                        If Player.Team = WinTeam AndAlso Not Player.IsMonster Then
                            If Player.Character.State.IsInDugeon() Then
                                DungeonsHandler.GetTemplate(Map).ToNextRoom(Player.Client)
                            End If
                        End If
                    End If

                End If
            Next

            If Type = FightType.PvM Then
                If WinnersMonsters.Count > 0 Then
                    Map.ReplaceMonsterGroup(MonsterGroup)
                Else
                    Map.GenerateMonsterGroup(False)
                End If
            End If

            Thread.Sleep(800)

            For Each Player As Fighter In Fighters()

                If Not Player.IsMonster AndAlso Not Player.Abandon Then
                    Player.Character.State.GetFight = Nothing
                    Player.Character.State.GetFighter = Nothing
                    Player.Character.State.InBattle = False
                End If
                Player.WaitingResult = False
                Player = Nothing
            Next

        End Sub

        Public Function GetItems(ByVal PercentPP As Integer, ByRef AllItems As List(Of Item), ByVal Restant As Integer) As IEnumerable(Of Item)

            Dim loots As New List(Of Item)

            If Restant = 0 Then
                For Each Item In AllItems
                    loots.Add(Item)
                Next
                For Each Item In loots
                    AllItems.Remove(Item)
                Next
                Return loots
            End If

            For Each Item As Item In AllItems
                Dim rnd = Basic.Rand(1, 100)

                If rnd < (PercentPP * 100) Then
                    loots.Add(Item)
                End If

            Next

            For Each Item In loots
                AllItems.Remove(Item)
            Next

            DropsManager.Merge(loots)

            Return loots

        End Function

#End Region

#Region " Quit functions "

        Public Sub Abandon(ByVal Fighter As Fighter)

            Dim IsHeroic As Boolean = Config.GetItem(Of Boolean)("ACTIVE_HEROIC")
            Dim BattleTime As Long = DateTime.Now.Subtract(StartTime).TotalMilliseconds
            Dim Packet As New StringBuilder("GE" & BattleTime & ";" & If(MonsterGroup IsNot Nothing, Bonus.ToString(), "") & "|" & Id & "|" & If(Type = FightType.Agression, "1", "0"))

            Dim LootedItems As New List(Of Item)
            Dim LootedKamas = 0

            'Remove Invocations
            FighterList.Remove(Function(f) f.isInvocation AndAlso f.Owner Is Fighter)

            If IsHeroic AndAlso Type = FightType.PvM OrElse Type = FightType.Agression Then

                'Items
                For Each Item As Item In Fighter.Character.Items.ListOfItems
                    Item.Position = -1
                    ItemLoots.Add(Item)
                Next
                Fighter.Character.Items.ClearItems()

                'Sac
                For Each Item As Item In Fighter.Character.MerchantBag.ListOfItems
                    Item.Position = -1
                    ItemLoots.Add(Item.Copy())
                Next
                Fighter.Character.MerchantBag.ClearItems()

                ItemLoots.AddRange(LootedItems)

                'Kamas
                LootedKamas = Fighter.Character.Player.Kamas
                KamasLoots += Fighter.Character.Player.Kamas
                Fighter.Character.Player.Kamas = 0

                'Xp
                If Type = FightType.Agression Then
                    ExpLoots += Math.Round(Fighter.Character.Player.Exp / 10)
                End If

            End If

            Packet.Append(GeneratePacket(If(Fighter.Team = 1, 0, 1), Fighter, New List(Of Item), 0, 0, 0, 0, 0))

            Dim Enemies = Fighters.Where(Function(f) f.Team <> Fighter.Team).ToList()
            For Each Enemy As Fighter In Enemies
                If IsHeroic Then
                    Dim WinItems = GetItems(100 / Enemies.Count, LootedItems, Enemies.IndexOf(Enemy) + 1 - Enemies.Count)
                    DropsManager.Merge(WinItems)
                    Packet.Append(GeneratePacket(Enemy.Team, Enemy, WinItems, Math.Round(LootedKamas / Enemies.Count), 0, 0, 0, 0))
                Else
                    Packet.Append(GeneratePacket(Enemy.Team, Enemy, New List(Of Item), 0, 0, 0, 0, 0))
                End If
            Next

            Fighter.Character.Client.Send(Packet.ToString())
            Fighter.WaitingResult = True

            Fighter.Stats.Stats.ResetBonus()
            Fighter.PM = Fighter.Stats.MaxPM
            Fighter.PA = Fighter.Stats.MaxPA
            Fighter.Character.State.GetFight = Nothing
            Fighter.Character.State.GetFighter = Nothing
            Fighter.Character.State.InBattle = False
            Fighter.Client.State.State = GameState.WaitingPacket.Create

            Fighter.Character.Dead()

            Thread.Sleep(800)
            Fighter.WaitingResult = False

            Fighter = Nothing

        End Sub

        Public Sub RemovePlayer(ByVal Player As Fighter)

            Send("GM|-" & Player.Id)

            Player.Client.Character.State.GetFight = Nothing
            Player.Client.Character.State.GetFighter = Nothing
            Player.Client.Character.State.InBattle = False

            Player.Client.State.State = GameState.WaitingPacket.Create

            Select Case State

                Case FightState.Playing

                    Player.Abandon = True
                    TurnList.Remove(Player.Id)
                    Abandon(Player)

                    If OnlyOneTeam() Then

                        If Type = FightType.PvM And Not Player.IsMonster Then
                            For Each Challenge In Challenges
                                Challenge.EndTurn(Player)
                            Next
                        End If

                        EndFight()
                    ElseIf Player.Id = ActualPlayer.Id Then

                        If Type = FightType.PvM And Not Player.IsMonster Then
                            For Each Challenge In Challenges
                                Challenge.EndTurn(Player)
                            Next
                        End If

                        FinishTurn()
                    End If

                Case FightState.None

                    Player.Abandon = True
                    TurnList.Remove(Player.Id)
                    Abandon(Player)

                    If OnlyOneTeam() Then
                        EndFight()
                    End If

                Case FightState.WaitTurn

                    Player.Abandon = True
                    TurnList.Remove(Player.Id)
                    Abandon(Player)

                    If OnlyOneTeam() Then
                        EndFight()
                    Else
                        NextPlayer(False)
                    End If

                Case FightState.Starting

                    If Type = FightType.Agression Then
                        Player.Abandon = True
                        Abandon(Player)
                    End If

                    FighterList.Remove(Player)
                    If Player.Starter Then
                        CancelBattle()
                    End If

            End Select

        End Sub

        Public Sub CancelBattle()

            Map.Send("Gc-" & Id)
            FightsHandler.RemoveFight(Me)

            For Each Player As Fighter In Fighters
                CancelPlayer(Player)
            Next

        End Sub

        Private Sub CancelPlayer(ByVal Player As Fighter)

            If Player.IsMonster Then Exit Sub

            Player.Client.Character.State.GetFight = Nothing
            Player.Client.Character.State.GetFighter = Nothing
            Player.Client.Character.State.InBattle = False

            Player.Client.State.State = GameState.WaitingPacket.Create

            Player.Send("GV")

        End Sub

#End Region

#Region " Traps "

        Private TrapList As New List(Of EffectTrap)

        Private ReadOnly Property GetTraps() As List(Of EffectTrap)
            Get
                Return (From Trap As EffectTrap In TrapList Select Trap Where Trap.Glyph = False).ToList
            End Get
        End Property

        Private ReadOnly Property GetGlyphs() As List(Of EffectTrap)
            Get
                Return (From Trap As EffectTrap In TrapList Select Trap Where Trap.Glyph = True).ToList
            End Get
        End Property

        Public Sub AddTrap(ByVal Trap As EffectTrap)
            TrapList.Add(Trap)
            Send("GA;999;" & Trap.Owner.Id & ";GDZ+" & Trap.Cell & ";" & Trap.Lenght & ";" & 7, Trap.Owner.Team)
            Trap.Owner.Send("GA;999;" & Trap.Owner.Id & ";GDC" & Trap.Cell & ";Haaaaaaaaz3005;")
        End Sub

        Public Sub AddGlyph(ByVal Glyph As EffectTrap)
            TrapList.Add(Glyph)
            Send("GA;999;" & Glyph.Owner.Id & ";GDZ+" & Glyph.Cell & ";" & Glyph.Lenght & ";" & Glyph.Color)
            Send("GA;999;" & Glyph.Owner.Id & ";GDC" & Glyph.Cell & ";Haaaaaaaaa3005;")
        End Sub

        Public Function WalkOnTrap(ByVal Cell As Integer) As Boolean
            For Each Trap As EffectTrap In GetTraps()
                If Cells.GoalDistance(Map, Trap.Cell, Cell) <= Trap.Lenght Then Return True
            Next
            Return False
        End Function

        Public Function WalkOnGlyph(ByVal Cell As Integer) As Boolean
            For Each Glyph As EffectTrap In GetGlyphs()
                If Cells.GoalDistance(Map, Glyph.Cell, Cell) <= Glyph.Lenght Then Return True
            Next
            Return False
        End Function

        Public Function TrapOnCell(ByVal Cell As Integer) As Boolean
            For Each Trap As EffectTrap In GetTraps()
                If Trap.Cell = Cell Then Return True
            Next
            Return False
        End Function

        Public Sub LaunchTrapEffects(ByVal Player As Fighter, ByVal Cell As Integer)

            For Each Trap As EffectTrap In GetTraps()
                If Utils.Cells.GoalDistance(Map, Trap.Cell, Cell) <= Trap.Lenght Then

                    Send("GA1;306;" & Player.Id & ";" & Trap.OriginalSpell & "," & Cell & ",407,1,1," & Trap.Owner.Id)
                    TrapList.Remove(Trap)
                    Trap.Use(Me)

                    Send("GA;999;" & Trap.Owner.Id & ";GDZ-" & Trap.Cell & ";" & Trap.Lenght & ";" & 7, Trap.Owner.Team)
                    Trap.Owner.Send("GA;999;" & Trap.Owner.Id & ";GDC" & Trap.Cell)

                End If
            Next

        End Sub

        Public Sub UpdateGlyphs()

            Dim Id As Integer = ActualPlayer.Id

            For Each Glyph As EffectTrap In GetGlyphs()

                If Id = Glyph.Owner.Id Then
                    Glyph.Turn -= 1
                    If Glyph.Turn <= 0 Then
                        TrapList.Remove(Glyph)
                        Send("GA;999;" & Glyph.Owner.Id & ";GDZ-" & Glyph.Cell & ";" & Glyph.Lenght & ";" & Glyph.Color)
                        Send("GA;999;" & Glyph.Owner.Id & "GDC" & Glyph.Cell)
                    End If
                End If

            Next

        End Sub

        Public Sub UseGlyphsEffects()

            Dim Cell As Integer = ActualPlayer.Cell

            For Each Glyph As EffectTrap In GetGlyphs
                If Utils.Cells.GoalDistance(Map, Glyph.Cell, Cell) <= Glyph.Lenght Then
                    Glyph.Use(Me, Cell)
                End If
            Next

        End Sub

#End Region

#Region " Send functions "

        Public Sub Send(ByVal Packet As String)
            For Each Fighter As Fighter In Fighters
                Fighter.Send(Packet)
            Next
        End Sub

        Public Sub Send(ByVal Packet As String, ByVal Team As Integer)
            For Each Fighter As Fighter In Fighters
                If Fighter.Team = Team Then
                    Fighter.Send(Packet)
                End If
            Next
        End Sub

        Public Sub SendMessage(ByVal Packet As String)
            For Each Fighter As Fighter In Fighters
                Fighter.SendMessage(Packet)
            Next
        End Sub

        Public Sub SendBlocks(ByVal Client As GameClient)
            For i As Integer = 0 To 1
                Dim Block As FightBlocks = Blocks(i)
                If Block.Basic Then Client.Send("Go+A" & Fighters(i).Id)
                If Block.Help Then Client.Send("Go+H" & Fighters(i).Id)
                If Block.Spectator Then Client.Send("Go+S" & Fighters(i).Id)
                If Block.Party Then Client.Send("Go+P" & Fighters(i).Id)
            Next
        End Sub

#End Region

#Region " Enums "

        Public Enum FightState

            Starting
            WaitTurn
            None
            Playing
            Result
            Finish

        End Enum

        Public Enum FightType
            Challenge = 0
            Agression = 1
            PvMA = 2
            MXvM = 3
            PvM = 4
            PvT = 5
            PvMU = 6
        End Enum

#End Region

#Region " Update "

        Public Sub Update() Handles MyTimer.Elapsed

            If State = World.Fight.FightState.Starting AndAlso
                TimeOut < Environment.TickCount AndAlso
                TimeOut <> -1 Then
                Start(True)
            ElseIf State = World.Fight.FightState.Playing AndAlso Not ActualPlayer.IsMonster AndAlso
                TimeOut < Environment.TickCount Then

                If Type = FightType.PvM And Not ActualPlayer.IsMonster Then
                    For Each Challenge In Challenges
                        Challenge.EndTurn(ActualPlayer)
                    Next
                End If

                FinishTurn()
            ElseIf State = World.Fight.FightState.WaitTurn AndAlso
                TimeOut < Environment.TickCount Then
                NextPlayer(True)
            End If

        End Sub

#End Region

    End Class
End Namespace