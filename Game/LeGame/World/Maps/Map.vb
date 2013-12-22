Imports Podemu.World.Maps
Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class Map

        Public Id As Integer
        Public Width, Height, PosX, PosY As Integer
        Public Subarea As Integer
        Public Time As String
        Public MapData As String = ""
        Public Key As String = ""
        Public MapTriggers As New List(Of MapTrigger)

        Public NeedRegister As Boolean

        Public Paddocks As New List(Of Paddock)

        Public CellInfos As New Dictionary(Of Integer, CellInfo)(560)

        Public InteractiveObjects As New Dictionary(Of Integer, InteractiveObject)

        Public ListGameCharacter As New List(Of GameClient)
        Public NpcList As New List(Of Npc)
        Public MonsterList As New ConcurrentList(Of MonsterGroup)
        Public MerchantList As New List(Of Merchant)
        Public PrismList As New List(Of PrismTemplate)

        Public Initialized As Boolean = False

        Private FightList As New List(Of Fight)

        Private WithEvents TimerActions As New Timers.Timer
        Private WithEvents TimerActions2 As New Timers.Timer

        Public Sub Init()
            Compressor.DecompressMapData(Me, MapData)
            MapData = String.Empty
            Initialized = True
            GenerateMonsterGroup(True)
            TimerActions.Interval = 15000
            TimerActions.Enabled = True
            TimerActions2.Interval = 13000
            TimerActions2.Enabled = True

            RaiseEvent OnInitialization()
        End Sub

        Private ReadOnly Property GetCharacters() As GameClient()
            Get
                For Each Client As GameClient In ListGameCharacter
                    If Client Is Nothing Then
                        ListGameCharacter.Remove(Client)
                    End If
                Next
                Return ListGameCharacter.ToArray()
            End Get
        End Property

        Private ReadOnly Property GetFights As Fight()
            Get
                For Each Fight As Fight In FightList
                    If Fight Is Nothing Then
                        FightList.Remove(Fight)
                    ElseIf Fight.Fighters.Count < 2 Then
                        Fight.CancelBattle()
                    End If
                Next
                Return FightList.ToArray
            End Get
        End Property

        Private ReadOnly Property SubAreaTemplate As SubAreaTemplate
            Get
                Return SubAreasHandler.GetTemplate(Subarea)
            End Get
        End Property

        Public ReadOnly Property IsDungeon As Boolean
            Get
                Return DungeonsHandler.IsDungeon(Me)
            End Get
        End Property

        Private Sub SendFightBlades(ByVal Client As GameClient)
            For Each Fight As Fight In GetFights
                If Not Fight Is Nothing Then
                    If Fight.State = World.Fight.FightState.Starting Then
                        Client.Send("Gc+" & Fight.BladesPattern)
                        Client.Send("Gt" & Fight.TeamPattern(0))
                        Client.Send("Gt" & Fight.TeamPattern(1))
                        Fight.SendBlocks(Client)
                    End If
                End If
            Next
        End Sub

        Public Delegate Sub Initialization()
        Public Event OnInitialization As Initialization

        Public Delegate Sub AddedCharacter(ByVal Client As GameClient)
        Public Event OnAddedCharacter As AddedCharacter

        Public Sub AddCharacter(ByVal Client As GameClient)

            If Not Initialized Then
                Init()
            End If

            If NeedRegister Then
                If Not Client.Infos.IsRegister AndAlso Not Client.Infos.HasSignalUnregister Then
                    Client.Send("BP+10")
                    Client.Infos.HasSignalUnregister = True
                End If
            Else
                If Client.Infos.HasSignalUnregister Then
                    Client.Send("BP-")
                    Client.Infos.HasSignalUnregister = False
                End If
            End If

            If Not ListGameCharacter.Contains(Client) Then
                Send("GM|+" & Client.Character.PatternDisplayChar)
                ListGameCharacter.Add(Client)
                Client.Send("GM" & CharactersPattern)
                If NpcExist() Then Client.Send("GM" & NpcsPattern)
                If FightExist() Then SendFightBlades(Client)
                If MonsterExist() Then Client.Send("GM" & MonstersPattern)
                If MerchantExist() Then Client.Send("GM" & MerchantsPattern)
                If InteractiveExist() Then Client.Send("GDF" & InteractivesPattern)
                If PaddockExist() Then Client.Send("GM" & PaddocksPattern)
                If PrismExist() Then Client.Send("GM" & PrismsPattern)

                RaiseEvent OnAddedCharacter(Client)

            End If

        End Sub

        Public Sub RefreshCharacter(ByVal Client As GameClient)
            If ListGameCharacter.Contains(Client) Then
                Send("GM|~" & Client.Character.PatternDisplayChar)
            End If
        End Sub

        Public Sub DelCharacter(ByVal Client As GameClient)
            ListGameCharacter.Remove(Client)
            Send("GM|-" & Client.Character.ID)
        End Sub

        Public Sub AddMount(ByVal Client As GameClient, ByVal mount As Mount)
            Send("GM|+" & mount.MountDisplayInfo)
        End Sub

        Public Sub AddEntity(ByVal pattern As String)
            Send("GM|+" & pattern)
        End Sub

        Public Sub RemoveEntity(ByVal id As Integer)
            Send("GM|-" & id)
        End Sub

        Public Sub RefreshMount(ByVal Client As GameClient, ByVal mount As Mount)
            Send("GM|~" & mount.MountDisplayInfo)
        End Sub

        Public Sub DelMount(ByVal Client As GameClient, ByVal mount As Mount)
            Send("GM|-" & mount.Id)
        End Sub

        Public Sub AddFight(ByVal Fight As Fight)
            FightList.Add(Fight)
            For Each Client As GameClient In GetCharacters
                Client.Send("Gc+" & Fight.BladesPattern)
                Client.Send("Gt" & Fight.TeamPattern(0))
                Client.Send("Gt" & Fight.TeamPattern(1))
            Next
        End Sub

        Public Sub Send(ByVal Packet As String)
            For Each Player As GameClient In GetCharacters()
                Player.Send(Packet)
            Next
        End Sub

        Public Sub Send(ByVal ParamArray Packet As String())
            For Each Player As GameClient In GetCharacters()
                Player.Send(Packet)
            Next
        End Sub

        Public Delegate Sub SendedMessage(ByVal Client As GameClient, ByVal Message As String)
        Public Event OnSendedMessage As SendedMessage

        Public Sub SendMessage(ByVal Message As String)
            For Each Player As GameClient In GetCharacters()
                Player.SendMessage(Message)
            Next
        End Sub

        Public Sub OnMessage(ByVal Client As GameClient, ByVal Message As String)
            RaiseEvent OnSendedMessage(Client, Message)
        End Sub

        Public ReadOnly Property CharactersPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Player In GetCharacters Select Player.Character.PatternDisplayChar).ToArray())
            End Get
        End Property

        Public ReadOnly Property NpcsPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Npc In NpcList Select Npc.GameInfo).ToArray())
            End Get
        End Property

        Public ReadOnly Property MonstersPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Monster In MonsterList Select Monster.GetGameInfo).ToArray())
            End Get
        End Property

        Public ReadOnly Property MerchantsPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Merchant In MerchantList Select Merchant.PatternDisplayMerchant).ToArray())
            End Get
        End Property

        Public ReadOnly Property PrismsPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Prism In PrismList Select Prism.PatternPrismView(Prism)).ToArray())
            End Get
        End Property

        Public ReadOnly Property PaddocksPattern() As String
            Get
                Return "|+" & String.Join("|+", (From Paddock In Paddocks Select Paddock.MountParkDisplayInfos).ToArray())
            End Get
        End Property

        Public ReadOnly Property InteractivesPattern() As String
            Get
                Return "|" & String.Join("|", (From Interactive In InteractiveObjects Select Interactive.Value.DisplayPattern).ToArray())
            End Get
        End Property

        Public Function IsCellReachable(ByVal Cell As Integer) As Boolean

            If Not Initialized Then Init()

            If Not CellInfos.ContainsKey(Cell) Then Return False
            Return (CellInfos(Cell).IsReachable)

        End Function

        Public Function IsCellEmpty(ByVal Cell As Integer) As Boolean

            Return NpcList.FirstOrDefault(Function(n) n.CellID = Cell) Is Nothing And ListGameCharacter.FirstOrDefault(Function(c) c.Character.MapCell = Cell) Is Nothing

        End Function

        Public Function MonsterOnCell(ByVal Cell As Integer) As Boolean
            For Each M As MonsterGroup In MonsterList
                If M.Cell = Cell Then Return True
            Next
            Return False
        End Function

        Public Function GetMonsterOnCell(ByVal Cell As Integer) As MonsterGroup
            For Each M As MonsterGroup In MonsterList
                If M.Cell = Cell Then Return M
            Next
            Return Nothing
        End Function

        Public Function GetCharacterOnCell(ByVal Cell As Integer) As GameClient
            For Each character In ListGameCharacter
                If character.Character.MapCell = Cell Then Return character
            Next
            Return Nothing
        End Function

        Public Function GetMount(ByVal MountId As Integer) As Mount
            For Each Paddock As Paddock In Paddocks
                If Paddock.ContainsMount(MountId) Then
                    Return Paddock.GetMountFromMountPark(MountId)
                End If
            Next
            Return Nothing
        End Function

        Public Function NothingOnCell(ByVal Cell As Integer) As Boolean
            For Each Npc As Npc In NpcList
                If Npc.CellID = Cell Then Return False
            Next
            For Each M As MonsterGroup In MonsterList
                If M.Cell = Cell Then Return False
            Next
            Return True
        End Function

        Public Function CharacterExist(ByVal Id As Integer) As Boolean
            For Each Player As GameClient In GetCharacters
                If Player.Character.ID = Id Then Return True
            Next
            Return False
        End Function

        Public Function GetCharacter(ByVal Id As Integer) As GameClient
            For Each Player As GameClient In GetCharacters
                If Player.Character.ID = Id Then Return Player
            Next
            Return Nothing
        End Function

        Public Function GetCharacter(ByVal Name As String) As GameClient
            For Each Player As GameClient In GetCharacters
                If Player.Character.Name = Name Then Return Player
            Next
            Return Nothing
        End Function

        Public Function NpcExist(ByVal Id As Integer) As Boolean
            For Each Npc As Npc In NpcList
                If Npc.ID = Id Then Return True
            Next
            Return False
        End Function

        Public Function MonsterExist(ByVal Id As Integer) As Boolean
            For Each M As MonsterGroup In MonsterList
                If M.Id = Id Then Return True
            Next
            Return False
        End Function

        Public Function MonsterExist() As Boolean
            Return MonsterList.Count > 0
        End Function

        Public Function InteractiveExist() As Boolean
            Return InteractiveObjects.Count > 0
        End Function

        Public Function MerchantExist() As Boolean
            Return MerchantList.Count > 0
        End Function

        Public Function PaddockExist() As Boolean
            Return Paddocks.Count > 0
        End Function

        Public Function PrismExist() As Boolean
            Return PrismList.Count > 0
        End Function

        Public Function GetNpc(ByVal Id As Integer) As Npc
            For Each Npc As Npc In NpcList
                If Npc.ID = Id Then Return Npc
            Next
            Return Nothing
        End Function

        Public Function GetMerchand(ByVal Id As Integer) As Merchant
            For Each Merchant As Merchant In MerchantList
                If Merchant.ID = Id Then Return Merchant
            Next
            Return Nothing
        End Function

        Public Function GetPaddock(ByVal CellId As Integer) As Paddock
            For Each Paddock As Paddock In Paddocks
                If Paddock.Template.CellId = CellId Then Return Paddock
            Next
            Return Nothing
        End Function

        Public Function HasSkillOnCell(ByVal CellId As Integer, ByVal SkillId As Integer) As Boolean
            If (InteractiveObjects.ContainsKey(CellId)) Then
                Return InteractiveObjects(CellId).HasSkills(SkillId)
            End If
            Return False
        End Function

        Public Sub ActiveInteractiveObject(ByVal Client As GameClient, ByVal CellId As Integer, ByVal SkillId As Integer)
            If Not HasSkillOnCell(CellId, SkillId) Then
                Return
            End If
            Client.Character.State.ExecuteWhenArrived(Sub() InteractiveObjects(CellId).Use(Client, SkillId))
        End Sub

        Public Function NpcExist() As Boolean
            Return NpcList.Count > 0
        End Function

        Public Function FightExist(ByVal Id As Integer) As Boolean
            For Each Fight As Fight In GetFights
                If Fight.Id = Id Then Return True
            Next
            Return False
        End Function

        Public Sub RemoveFight(ByVal Fight As Fight)
            FightList.Remove(Fight)
        End Sub

        Public Function GetFight(ByVal Id As Integer) As Fight
            For Each Fight As Fight In GetFights
                If Fight.Id = Id Then Return Fight
            Next
            Return Nothing
        End Function

        Public Function FightExist() As Boolean
            Return GetFights.Count > 0
        End Function

        Public ReadOnly Property NextID() As Integer
            Get
                Dim i As Integer = -1
                While (NpcExist(i) Or MonsterExist(i))
                    i -= 1
                End While
                Return i
            End Get
        End Property

        Public ReadOnly Property NextFightID() As Integer
            Get
                Dim i As Integer = 1
                While (FightExist(i))
                    i += 1
                End While
                Return i
            End Get
        End Property

        Public ReadOnly Property MaxCell() As Integer
            Get
                Return Math.Floor(Math.Sqrt(Width ^ 2 + Height ^ 2))
            End Get
        End Property

        Public Function GetRandomCell(ByVal OnBattle As Boolean, Optional ByVal Battle As Fight = Nothing) As Integer

            If Not Initialized Then Init()

            Dim PossibleList As New List(Of Integer)
            For i As Integer = 1 To CellInfos.Count - 1
                If OnBattle Then
                    If IsCellReachable(i) AndAlso (Battle.GetFighterFromCell(i) Is Nothing) Then
                        PossibleList.Add(i)
                    End If
                Else
                    If IsCellReachable(i) AndAlso NothingOnCell(i) AndAlso CellInfos(i).Movement <> MovementEnum.PADDOCK AndAlso CellInfos(i).Movement <> MovementEnum.TRIGGER Then
                        PossibleList.Add(i)
                    End If
                End If
            Next
            Return PossibleList(Basic.Rand(0, PossibleList.Count - 1))
        End Function

        Public Sub AddMonsterGroup(ByVal MonsterGroup As MonsterGroup)

            MonsterGroup.Id = NextID()

            Dim Cell As Integer

            While True

                Cell = GetRandomCell(False)

                If Not IsCellReachable(Cell) Then Continue While

                For Each OtherGroup In MonsterList

                    Dim Dist = Cells.GoalDistanceEstimate(Me, Cell, OtherGroup.Cell)

                    If Dist < 8 Then Continue While

                Next

                Exit While

            End While

            MonsterGroup.Cell = Cell
            MonsterGroup.Dir = Basic.RandomFourDir()

            MonsterList.Add(MonsterGroup)
            Send("GM" & MonsterGroup.GetGameInfo)

        End Sub

        Public Sub ReplaceMonsterGroup(ByVal MonsterGroup As MonsterGroup)

            MonsterGroup.Id = NextID()

            MonsterList.Add(MonsterGroup)
            Send("GM" & MonsterGroup.GetGameInfo)

        End Sub

        Public Sub DelMonsterGroup(ByVal MonsterGroup As MonsterGroup)

            Send("GM|-" & MonsterGroup.Id)
            MonsterList.Remove(MonsterGroup)

        End Sub

        Public Sub GenerateMonsterGroup(ByVal Now As Boolean)

            If Not SubAreasHandler.Exist(Subarea) Then Exit Sub

            If ZaapsHandler.HasZaap(Id) AndAlso ZaapsHandler.GetTemplates(Id).FirstOrDefault(Function(t) Not t.HasMonster) IsNot Nothing Then
                Exit Sub
            End If

            If ZaapisManager.HasZaapi(Id) AndAlso ZaapisManager.GetTemplates(Id).FirstOrDefault(Function(t) Not t.HasMonster) IsNot Nothing Then
                Exit Sub
            End If

            If PrismHandler.HasPrism(Id) AndAlso PrismHandler.GetTemplates(Id).FirstOrDefault(Function(t) Not t.HasMonster) IsNot Nothing Then
                Exit Sub
            End If

            Dim temp = SubAreasHandler.GetTemplate(Subarea)

            While MonsterList.Count < temp.GroupNumber

                Dim NewGroup = temp.GetGroup(Me)

                If NewGroup Is Nothing Then Exit While

                If Now OrElse temp.RespawnTime = 0 Then
                    AddMonsterGroup(NewGroup)
                Else
                    Dim timer As New Threading.Timer(DirectCast(AddressOf AddMonsterGroup, Threading.TimerCallback), NewGroup, Basic.Rand(temp.RespawnTime * 0.5, temp.RespawnTime * 1.5), -1)
                End If

            End While

        End Sub

        Public Sub Spawnmob(ByVal id As Integer)
            Try
                Dim NewGroup As New MonsterGroup
                Dim NewMonster As New Monster(Game.MonstersHandler.GetTemplate(id).GetRandomLevel)
                NewGroup.MonsterList.Add(NewMonster)
                AddMonsterGroup(NewGroup)
            Catch ex As Exception

            End Try
        End Sub

        Public Sub Spawnmobs(ByVal number As Integer, ByVal IDS() As String)
            Dim NewGroup As New MonsterGroup
            For i As Integer = 0 To number - 1
                Dim NewMonster As New Monster(Game.MonstersHandler.GetTemplate(IDS(i)).GetRandomLevel)
                NewGroup.MonsterList.Add(NewMonster)
            Next
            AddMonsterGroup(NewGroup)
        End Sub

        Public Sub RespawnMobs()
            For Each MG As MonsterGroup In MonsterList
                Send("GM|-" & MG.Id)
            Next
            MonsterList.Clear()
            GenerateMonsterGroup(True)
        End Sub

        Private Sub MoveMonsterGroup() Handles TimerActions.Elapsed

            If ListGameCharacter.Count > 0 AndAlso ListGameCharacter.Count < 10 AndAlso MonsterList.Count > 0 Then

                Dim MonsterToMove As MonsterGroup = MonsterList(Basic.Rand(0, MonsterList.Count - 1))
                Dim Cell As Integer = Cells.NextCell(Me, MonsterToMove.Cell, Basic.RandomFourDir)

                MoveEntity(MonsterToMove.Id, MonsterToMove.Cell, MonsterToMove.Dir, Cell)

                MonsterToMove.Cell = Cell

            End If

        End Sub

        Public Function MoveEntity(ByVal Id As Integer, ByVal StartCell As Integer, ByVal StartDirection As Integer, ByVal EndCell As Integer) As Boolean

            If IsCellReachable(EndCell) Then
                Dim Pathfinding As New Pathfinding()
                Dim Path As String = Pathfinding.Pathing(Me, StartCell, EndCell, True)

                If Path.Length <> 0 Then
                    Send("GA0;1;", Id, ";", Utils.Cells.GetDirChar(StartDirection), Utils.Cells.GetCellChars(StartCell), Path)
                    Return True
                End If
            End If

            Return False

        End Function

        Private Sub MoveMounts() Handles TimerActions2.Elapsed

            If ListGameCharacter.Count > 0 AndAlso Paddocks.Count > 0 Then

                Dim paddock As Paddock = Paddocks(Basic.Rand(0, Paddocks.Count - 1))

                If paddock.Mounts.Count = 0 Then
                    Return
                End If

                Dim mount As Mount = paddock.Mounts(Basic.Rand(0, paddock.Mounts.Count - 1))
                Dim Cell As Integer = paddock.GetRandomPossibleCase()

                If (MoveEntity(mount.Id, mount.CellId, mount.Direction, Cell)) Then
                    mount.CellId = Cell
                    paddock.ApplyCollision(mount)
                End If

            End If

        End Sub

    End Class
End Namespace