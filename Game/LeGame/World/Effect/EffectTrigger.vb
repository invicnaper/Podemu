﻿Imports Podemu.Game
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace World
    Public Class EffectTrigger

        Public Base As SpellEffect
        Public MyEffect As Effect
        Public ShowedEffect As Effect
        Public Player As Fighter
        Public Cibles As List(Of Fighter)
        Public Cell As Integer
        Public Tours As Integer

        Public figher As Integer
        Public Trap As Boolean = False

        Public Value, Value2, Value3 As Integer
        Dim BuffLauncher As Object
        Dim DegatsStr As String

        Public Sub New(ByVal BaseEffect As SpellEffect, ByVal Player As Fighter, ByVal Cibles As List(Of Fighter), ByVal Cell As Integer, ByVal OverrideEffect As Effect)

            Base = BaseEffect
            MyEffect = OverrideEffect
            ShowedEffect = Base.Effect
            Me.Player = Player
            Me.Cibles = Cibles
            Me.Cell = Cell

            Me.Tours = Base.Tours
            Me.Value = Base.Value
            Me.Value2 = Base.Value2
            Me.Value3 = Base.Value3

        End Sub

        Public Function Copy() As EffectTrigger
            Dim CopyCibles As New List(Of Fighter)
            CopyCibles.AddRange(Cibles)
            Dim NewTrigger As New EffectTrigger(Base, Player, CopyCibles, Cell, MyEffect)
            NewTrigger.Value = Value
            NewTrigger.Value2 = Value2
            NewTrigger.Value3 = Value3
            Return NewTrigger
        End Function

        Public Sub UseEffect()

            Select Case MyEffect

                Case Effect.Teleport
                    EffectTeleport()
                Case Effect.Transpose
                    EffectTranspose()

                Case Effect.PushBack
                    EffectPush()
                Case Effect.PushFear
                    EffectPushFear()
                Case Effect.PushFront
                    EffectPushFront()

                Case Effect.DamageAir
                    EffectDamage()
                Case Effect.DamageEau
                    EffectDamage()
                Case Effect.DamageFeu
                    EffectDamage()
                Case Effect.DamageTerre
                    EffectDamage()
                Case Effect.DamageNeutre
                    EffectDamage()

                Case Effect.VolAir
                    EffectVol()
                Case Effect.VolEau
                    EffectVol()
                Case Effect.VolFeu
                    EffectVol()
                Case Effect.VolTerre
                    EffectVol()
                Case Effect.VolNeutre
                    EffectVol()

                Case Effect.AddState
                    EffectChangeState(True)
                Case Effect.LostState
                    EffectChangeState(False)

                Case Effect.Heal
                    EffectHeal()

                Case Effect.Invisible
                    EffectInvisible()
                Case Effect.UseTrap
                    EffectTrap()
                Case Effect.UseGlyph
                    EffectGlyph()
                Case Effect.DeleteAllBonus
                    EffectDeleteBonus()
                Case Effect.AddChatiment
                    AddChatiment()
                Case Effect.ChangeSkin
                    ChangeSkin(False)

                Case Effect.Porter
                    EffectPorter()
                Case Effect.Lancer
                    EffectLancer()

                Case Effect.Invocation
                    EffectInvocation()

                Case Effect.UseTrap

                Case Effect.DoNothing

                Case Else
                    Value = GetRandomJet(Base.Str)
                    BuffEffects(False)

            End Select

        End Sub

        Dim iValue As Integer = -1

        Public Sub BuffEffects(ByVal Reverse As Boolean)

            If Reverse Then Value = -Value
            If Value = 0 Then Exit Sub
            Dim OriginalValue As Integer = Value

            ShowedEffect = MyEffect

            Dim DoNothing As Boolean = False

            For Each Cible As Fighter In Cibles

                iValue = OriginalValue

                Select Case MyEffect

                    Case Effect.SubPAEsquive
                        EffectSubPA(Cible, True)
                        DoNothing = True
                    Case Effect.SubPMEsquive
                        EffectSubPM(Cible, True)
                        DoNothing = True

                    Case Effect.SubPA
                        EffectSubPA(Cible, False)
                        DoNothing = True
                    Case Effect.SubPM
                        EffectSubPM(Cible, False)
                        DoNothing = True

                    Case Effect.AddPABis, Effect.AddPA
                        EffectBuff(Cible.Stats.Stats.PA.Boosts)
                        If Not Reverse Then EffectBuff(Cible.Stats.PA)
                    Case Effect.AddPM
                        EffectBuff(Cible.Stats.Stats.PM.Boosts)
                        If Not Reverse Then EffectBuff(Cible.Stats.PM)

                    Case Effect.AddPO
                        EffectBuff(Cible.Stats.Stats.PO.Boosts)
                    Case Effect.SubPO
                        ShowedEffect = Effect.AddPO
                        EffectBuff(Cible.Stats.Stats.PO.Boosts, True)

                    Case Effect.AddDamage
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegats.Boosts)
                    Case Effect.AddDamageCritic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusCoupCritique.Boosts)
                    Case Effect.AddDamageMagic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsMagique.Boosts)
                    Case Effect.AddDamagePercent
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsPercent.Boosts)
                    Case Effect.AddDamagePhysic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsPhysique.Boosts)
                    Case Effect.AddDamagePiege
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsPiege.Boosts)
                    Case Effect.AddDamagePiegePercent
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsPiegePercent.Boosts)
                    Case Effect.AddSoins
                        EffectBuff(Cible.Stats.Stats.Damages.BonusSoins.Boosts)
                    Case Effect.AddEchecCritic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusEchecCritique.Boosts)
                    Case Effect.AddRenvoiDamage
                        EffectBuff(Cible.Stats.Stats.Damages.RenvoiDegats.Boosts)

                    Case Effect.SubDamage
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegats.Boosts, True)
                    Case Effect.SubDamageCritic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusCoupCritique.Boosts, True)
                    Case Effect.SubDamageMagic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsMagique.Boosts, True)
                    Case Effect.SubDamagePhysic
                        EffectBuff(Cible.Stats.Stats.Damages.BonusDegatsPhysique.Boosts, True)
                    Case Effect.SubSoins
                        EffectBuff(Cible.Stats.Stats.Damages.BonusSoins.Boosts, True)

                    Case Effect.AddChance
                        EffectBuff(Cible.Stats.Stats.Base.Chance.Boosts)
                    Case Effect.AddForce
                        EffectBuff(Cible.Stats.Stats.Base.Force.Boosts)
                    Case Effect.AddSagesse
                        EffectBuff(Cible.Stats.Stats.Base.Sagesse.Boosts)
                    Case Effect.AddIntelligence
                        EffectBuff(Cible.Stats.Stats.Base.Intelligence.Boosts)
                    Case Effect.AddAgilite
                        EffectBuff(Cible.Stats.Stats.Base.Agilite.Boosts)

                    Case Effect.SubChance
                        EffectBuff(Cible.Stats.Stats.Base.Chance.Boosts, True)
                    Case Effect.SubForce
                        EffectBuff(Cible.Stats.Stats.Base.Force.Boosts, True)
                    Case Effect.SubSagesse
                        EffectBuff(Cible.Stats.Stats.Base.Sagesse.Boosts, True)
                    Case Effect.SubIntelligence
                        EffectBuff(Cible.Stats.Stats.Base.Intelligence.Boosts, True)
                    Case Effect.SubAgilite
                        EffectBuff(Cible.Stats.Stats.Base.Agilite.Boosts, True)

                    Case Effect.AddReduceDamageAir
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Air.Boosts)
                    Case Effect.AddReduceDamageEau
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Eau.Boosts)
                    Case Effect.AddReduceDamageFeu
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Feu.Boosts)
                    Case Effect.AddReduceDamageTerre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Terre.Boosts)
                    Case Effect.AddReduceDamageNeutre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Neutre.Boosts)
                    Case Effect.AddReduceDamagePhysic
                        EffectBuff(Cible.Stats.Stats.Armor.ReductionPhysique.Boosts)
                    Case Effect.AddReduceDamageMagic
                        EffectBuff(Cible.Stats.Stats.Armor.ReductionMagique.Boosts)
                    Case Effect.AddReduceDamagePourcentAir
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentAir.Boosts)
                    Case Effect.AddReduceDamagePourcentEau
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentEau.Boosts)
                    Case Effect.AddReduceDamagePourcentFeu
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentFeu.Boosts)
                    Case Effect.AddReduceDamagePourcentTerre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentTerre.Boosts)
                    Case Effect.AddReduceDamagePourcentNeutre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentNeutre.Boosts)

                    Case Effect.AddArmor, Effect.AddArmorBis
                        EffectAddArmor(Cible)

                    Case Effect.SubReduceDamageAir
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Air.Boosts, True)
                    Case Effect.SubReduceDamageEau
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Eau.Boosts, True)
                    Case Effect.SubReduceDamageFeu
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Feu.Boosts, True)
                    Case Effect.SubReduceDamageTerre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Terre.Boosts, True)
                    Case Effect.SubReduceDamageNeutre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.Neutre.Boosts, True)
                    Case Effect.SubReduceDamagePourcentAir
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentAir.Boosts, True)
                    Case Effect.SubReduceDamagePourcentEau
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentEau.Boosts, True)
                    Case Effect.SubReduceDamagePourcentFeu
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentFeu.Boosts, True)
                    Case Effect.SubReduceDamagePourcentTerre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentTerre.Boosts, True)
                    Case Effect.SubReduceDamagePourcentNeutre
                        EffectBuff(Cible.Stats.Stats.Armor.Resistances.PercentNeutre.Boosts, True)

                    Case Effect.AddEsquivePA
                        EffectBuff(Cible.Stats.Stats.Armor.EsquivePA.Boosts)
                    Case Effect.AddEsquivePM
                        EffectBuff(Cible.Stats.Stats.Armor.EsquivePM.Boosts)
                    Case Effect.AddInvocationMax
                        EffectBuff(Cible.Stats.Stats.MaxInvocation.Boosts)
                    Case Effect.SubEsquivePA
                        EffectBuff(Cible.Stats.Stats.Armor.EsquivePA.Boosts, True)
                    Case Effect.SubEsquivePM
                        EffectBuff(Cible.Stats.Stats.Armor.EsquivePM.Boosts, True)

                    Case Else
                        iValue = Integer.MinValue

                End Select

                If (Not Reverse) AndAlso iValue <> Integer.MinValue AndAlso Not DoNothing Then
                    Cible.Buffs.Add(Me.Copy(), Cible, BuffType.Stats)
                    If ShowedEffect <> Effect.None Then Player.Fight.Send("GA;" & ShowedEffect & ";" & Player.Id & ";" & Cible.Id & "," & iValue & "," & Tours)
                End If

            Next

        End Sub

        Private Sub EffectSubPA(ByVal Cible As Fighter, ByVal CanEsquive As Boolean)

            If iValue > 0 Then ' Perte de PA

                ' Evitons de perdre plus que possible
                Dim Original As Integer = If(iValue > Cible.Stats.MaxPA, Cible.Stats.MaxPA, iValue)

                If CanEsquive Then
                    ' Appliquons les esquives diverses
                    iValue = Formulas.PointLost(Player, Cible, Original, False)
                End If

                ' Si y'a eu une esquive
                If iValue < Original Then
                    ' On envoi le message
                    Player.Fight.Send("GA;308;" & Player.Id & ";" & Cible.Id & "," & (Original - iValue))
                End If

                ' On Buff les PA
                Cible.Stats.Stats.PA.Boosts -= iValue
                Cible.Stats.PA -= iValue

                Player.Fight.Send("GA;101;" & Player.Id & ";" & Cible.Id & ",-" & iValue)

                Value = iValue

                Cible.Buffs.Add(Me.Copy(), Cible, BuffType.Stats)

            Else ' Re-gain de PA

                iValue = -iValue

                Dim Maximum As Integer = Cible.Stats.RealMaxPA - Cible.Stats.MaxPA
                If Maximum > 0 Then

                    iValue = If(iValue > Maximum, Maximum, iValue)
                    Cible.Stats.Stats.PA.Boosts += iValue
                    Cible.Stats.PA += iValue

                End If

                Value = iValue

            End If

        End Sub

        Private Sub EffectSubPM(ByVal Cible As Fighter, ByVal CanEsquive As Boolean)

            If iValue > 0 Then ' Perte de PM

                ' Evitons de perdre plus que possible
                Dim Original As Integer = If(iValue > Cible.Stats.MaxPM, Cible.Stats.MaxPM, iValue)

                If CanEsquive Then
                    ' Appliquons les esquives diverses
                    iValue = Formulas.PointLost(Player, Cible, Original, True)
                End If

                ' Si y'a eu une esquive
                If iValue < Original Then
                    ' On envoi le message
                    Player.Fight.Send("GA;309;" & Player.Id & ";" & Cible.Id & "," & (Original - iValue))
                End If

                ' On Buff les PM
                Cible.Stats.Stats.PM.Boosts -= iValue
                Cible.Stats.PM -= iValue

                Player.Fight.Send("GA;127;" & Player.Id & ";" & Cible.Id & ",-" & iValue)

                Value = iValue

                Cible.Buffs.Add(Me.Copy(), Cible, BuffType.Stats)

            Else ' Re-gain de PM

                iValue = -iValue

                Dim Maximum As Integer = Cible.Stats.RealMaxPM - Cible.Stats.MaxPM
                If Maximum > 0 Then

                    iValue = If(iValue > Maximum, Maximum, iValue)
                    Cible.Stats.Stats.PM.Boosts += iValue
                    Cible.Stats.PM += iValue

                End If

                Value = iValue

            End If

        End Sub

        Private Sub EffectAddArmor(ByVal Cible As Fighter)

            ShowedEffect = Effect.None

            Select Case Base.SpellID

                Case 1
                    EffectBuff(Cible.Stats.Stats.Armor.ReductionFeu.Boosts)

                Case 6
                    EffectBuff(Cible.Stats.Stats.Armor.ReductionTerre.Boosts)

                Case 14
                    EffectBuff(Cible.Stats.Stats.Armor.ReductionAir.Boosts)

                Case 18
                    EffectBuff(Cible.Stats.Stats.Armor.ReductionEau.Boosts)

                Case Else
                    EffectBuff(Cible.Stats.Stats.Armor.ReductionNeutre.Boosts)

            End Select

        End Sub

        Private Sub EffectBuff(ByRef Stat As Integer, Optional ByVal Soustract As Boolean = False)

            iValue = If(Soustract, -iValue, iValue)
            Stat += iValue

        End Sub

        Private Sub EffectTeleport()

            If Player.Fight.IsValidCell(Cell) Then
                Player.Cell = Cell
                Player.Fight.Send("GA0;4;" & Player.Id & ";" & Player.Id & "," & Cell)
            End If

        End Sub


        Private Sub EffectTranspose()

            Dim Transposed As Fighter = Player.Fight.GetFighterFromCell(Cell)
            If Not Transposed Is Nothing Then

                Transposed.Cell = Player.Cell
                Player.Cell = Cell

                Player.Fight.Send("GA0;4;" & Player.Id & ";" & Player.Id & "," & Player.Cell)
                Player.Fight.Send("GA0;4;" & Player.Id & ";" & Transposed.Id & "," & Transposed.Cell)

            End If

        End Sub
        'Private Sub Attract(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter), ByVal Cell As Integer)
        '
        ' For Each Player As Fighter In ListOfCible
        '  Dim Direction As Integer = -1
        '    If Player.Cell = Cell Then
        '   Direction = Utils.Cells.GetDirection(TheClient.MyBattle.GetMap(), Player.Cell, TheClient.Cell)
        ' Else
        '     Direction = Utils.Cells.GetDirection(TheClient.MyBattle.GetMap(), Player.Cell, Cell)
        ' End If
        ' Dim NewCell As Integer = Player.Cell
        '    For i As Integer = 1 To Value
        ' Dim PossibleNewCell As Integer = Utils.Cells.NextCell(TheClient.MyBattle.GetMap(), NewCell, Direction)
        '    If TheClient.MyBattle.IsCellOk(PossibleNewCell) Then
        '       NewCell = PossibleNewCell
        '  Else
        '      Exit For
        '  End If
        '  If TheClient.MyBattle.WalkOnTrap(NewCell) Then
        '      Player.Cell = NewCell
        '     TheClient.MyBattle.SendToAll("GA0;5;" & Client.GetID & ";" & Player.GetID & "," & NewCell)
        '    TheClient.MyBattle.LaunchTrapEffects(NewCell)
        '     Exit Sub
        '  End If
        ' Next
        '  Player.Cell = NewCell
        ' TheClient.MyBattle.SendToAll("GA0;5;" & TheClient.GetID & ";" & Player.GetID & "," & NewCell)
        '  Next

        'End Sub


        Private Sub UseChatiment(ByVal TheClient As Fighter, ByVal ListOfCible As List(Of Fighter))

            For Each Player As Fighter In ListOfCible

                ' AddChatiment(Player, Client)
                '  If Not IsBuff Then Buff(Player, TheClient)

            Next

        End Sub
        Public Sub UseChatimentEffect(ByVal TheClient As Fighter, ByVal Degats As Integer)

            Dim ListOfCible As New List(Of Fighter)
            ListOfCible.Add(TheClient)
            Value = Degats / 2
            Value2 = -1
            DegatsStr = "0d0+" & Value
            ' UseEffect(BuffLauncher, ListOfCible, 0)

        End Sub

        Private Sub EffectPorter()

            If Not Player.State.Has(FighterState.State.Porte) AndAlso Not Player.State.Has(FighterState.State.Porteur) Then
                Dim Cible As Fighter = Player.Fight.GetFighterFromCell(Cell)
                If Not Cible Is Nothing Then

                    Player.Link = Cible
                    Cible.Link = Player

                    Player.State.Add(Player, FighterState.State.Porteur, Integer.MaxValue)
                    Cible.State.Add(Cible, FighterState.State.Porte, Integer.MaxValue)
                    Player.Fight.Send("GA0;50;" & Player.Id & ";" & Cible.Id)

                End If
            End If

        End Sub

        Private Sub EffectLancer()

            If Player.State.Has(FighterState.State.Porteur) Then

                If Player.Fight.IsValidCell(Cell) Then

                    Dim Cible As Fighter = Player.Link

                    Player.State.Del(Player, FighterState.State.Porteur)
                    Player.Link = Nothing
                    Cible.State.Del(Cible, FighterState.State.Porte)
                    Cible.Link = Nothing

                    Cible.Cell = Cell

                    Player.Fight.Send("GA0;51;" & Player.Id & ";" & Cell)

                End If

            End If

        End Sub

        Private Sub EffectDamage()

            If Tours <= 0 And (Not Trap) Then Player.State.LostInvisible(Player)

            Dim Damages As Integer = Player.Stats.Stats.GetDamages(GetRandomJet(Base.Str), MyEffect, Trap)

            For Each Cible As Fighter In Cibles
                If Tours > 0 Then
                    Cible.Buffs.Add(Me.Copy(), Cible, BuffType.Damage)
                Else
                    TakeDamage(Cible, Cible.Stats.Stats.ResistDamages(Damages, MyEffect, Player.Fight.Pvp))
                End If
            Next

        End Sub

        Private Sub TakeDamage(ByVal Cible As Fighter, ByVal Damages As Integer)

            If Damages < 0 Then Damages = 0

            Cible.Buffs.ExecuteOnAttack(Player, Damages / 2)

            Dim Armor As Integer = Cible.Stats.Stats.GetArmor(MyEffect)

            If Armor > 0 Then
                Damages -= Armor
                Player.Fight.Send("GA;105;" & Player.Id & ";" & Cible.Id & "," & Armor)
                If Damages < 0 Then Damages = 0
            End If

            If Damages > Cible.Life Then Damages = Cible.Life
            Cible.Life -= Damages

            Damages = (-Damages)
            Player.Fight.Send("GA;100;" & Player.Id & ";" & Cible.Id & "," & Damages)

        End Sub

        Private Sub EffectVol()

            If Tours <= 0 And (Not Trap) Then Player.State.LostInvisible(Player)

            Dim Damages As Integer = Player.Stats.Stats.GetDamages(GetRandomJet(Base.Str), MyEffect, Trap)

            For Each Cible As Fighter In Cibles
                If Tours > 0 Then
                    Cible.Buffs.Add(Me.Copy, Cible, BuffType.Damage)
                Else
                    TakeVol(Cible, Cible.Stats.Stats.ResistDamages(Damages, MyEffect, Player.Fight.Pvp))
                End If
            Next

        End Sub

        Private Sub TakeVol(ByVal Cible As Fighter, ByVal Damages As Integer)

            If Damages < 0 Then Damages = 0

            Cible.Buffs.ExecuteOnAttack(Player, Damages / 2)

            Dim Armor As Integer = Cible.Stats.Stats.GetArmor(MyEffect)

            If Armor > 0 Then
                Damages -= Armor
                Player.Fight.Send("GA;105;" & Player.Id & ";" & Cible.Id & "," & Armor)
                If Damages < 0 Then Damages = 0
            End If

            If Damages > Cible.Life Then Damages = Cible.Life
            Dim Soin As Integer = Math.Ceiling(Damages / 2)

            If Soin > (Player.Stats.MaximumLife - Player.Life) Then Soin = (Player.Stats.MaximumLife - Player.Life)

            Cible.Life -= Damages
            Player.Life += Soin

            Damages = (-Damages)
            Player.Fight.Send("GA;100;" & Player.Id & ";" & Cible.Id & "," & Damages)
            Player.Fight.Send("GA;100;" & Player.Id & ";" & Player.Id & "," & Soin)

        End Sub

        Private Sub EffectHeal()

            Dim Soin As Integer = Player.Stats.Stats.GetSoin(GetRandomJet(Base.Str))

            For Each Cible As Fighter In Cibles
                If Tours > 0 Then
                    Cible.Buffs.Add(Me.Copy, Cible, BuffType.Damage)
                Else
                    TakeHeal(Cible, Soin)
                End If
            Next

        End Sub

        Private Sub TakeHeal(ByVal Cible As Fighter, ByVal Soin As Integer)

            If Soin + Cible.Life > Cible.Stats.MaximumLife Then Soin = Cible.Stats.MaximumLife - Cible.Life
            Cible.Life += Soin
            Player.Fight.Send("GA;100;" & Player.Id & ";" & Cible.Id & "," & Soin)

        End Sub

        Private Sub EffectChangeState(ByVal Add As Boolean)

            For Each Cible As Fighter In Cibles
                ChangeState(Cible, Add)
            Next

        End Sub

        Private Sub ChangeState(ByVal Cible As Fighter, ByVal Add As Boolean)

            If Add Then
                Cible.State.Add(Player, Value3, Base.Tours)
            Else
                Cible.State.Del(Player, Value3)
            End If

        End Sub

        Private Sub EffectInvisible()

            For Each Cible As Fighter In Cibles
                Cible.State.Add(Player, FighterState.State.Invisible, Base.Tours)
            Next

        End Sub

        Private Sub EffectPush()

            For Each Cible As Fighter In Cibles

                Dim Direction As Integer = -1

                If Cells.InLine(Player.Fight.Map, Cell, Cible.Cell) AndAlso Cell <> Cible.Cell Then
                    Direction = Utils.Cells.GetDirection(Player.Fight.Map, Cell, Cible.Cell)
                ElseIf Cells.InLine(Player.Fight.Map, Player.Cell, Cible.Cell) Then
                    Direction = Utils.Cells.GetDirection(Player.Fight.Map, Player.Cell, Cible.Cell)
                Else
                    Direction = Basic.Rand(0, 3) * 2 + 1
                End If

                Dim NewCell As Integer = Cible.Cell
                For i As Integer = 1 To Value
                    Dim PossibleNewCell As Integer = Utils.Cells.NextCell(Player.Fight.Map, NewCell, Direction)
                    If Player.Fight.IsValidCell(PossibleNewCell) Then
                        NewCell = PossibleNewCell
                    Else
                        Dim Coef As Integer = Basic.Rand(8, 17)
                        Dim Coef2 As Double = Player.Stats.Level / 50
                        If Coef2 < 0.1 Then Coef2 = 0.1
                        Dim Degats As Integer = Math.Floor(Coef * Coef2) * (Value - i + 1)
                        If Degats > Cible.Life Then Degats = Cible.Life
                        Cible.Life -= Degats
                        Player.Fight.Send("GA;100;" & Player.Id & ";" & Cible.Id & ",-" & Degats)
                        Exit For
                    End If
                Next

                Cible.Cell = NewCell
                Player.Fight.Send("GA0;5;" & Player.Id & ";" & Cible.Id & "," & NewCell)

            Next

        End Sub

        Private Sub EffectPushFear()

            If Cells.InLine(Player.Fight.Map, Cell, Player.Cell) Then

                Dim Direction As Integer = Cells.GetDirection(Player.Fight.Map, Player.Cell, Cell)
                Dim Cible As Fighter = Player.Fight.GetFighterFromCell(Cells.NextCell(Player.Fight.Map, Player.Cell, Direction))

                If Not Cible Is Nothing Then

                    Dim Value As Integer = Cells.GoalDistance(Player.Fight.Map, Player.Cell, Cell)

                    Dim NewCell As Integer = Cible.Cell
                    For i As Integer = 1 To Value
                        Dim PossibleNewCell As Integer = Utils.Cells.NextCell(Player.Fight.Map, NewCell, Direction)
                        If Player.Fight.IsValidCell(PossibleNewCell) Then
                            NewCell = PossibleNewCell
                        Else
                            Exit For
                        End If
                    Next

                    Cible.Cell = NewCell
                    Player.Fight.Send("GA0;5;" & Player.Id & ";" & Cible.Id & "," & NewCell)

                End If

            End If

        End Sub

        Private Sub EffectPushFront()

            For Each Cible As Fighter In Cibles

                Dim Direction As Integer = -1

                If Cells.InLine(Player.Fight.Map, Cell, Cible.Cell) AndAlso Cell <> Cible.Cell Then
                    Direction = Utils.Cells.GetDirection(Player.Fight.Map, Cible.Cell, Cell)
                ElseIf Cells.InLine(Player.Fight.Map, Player.Cell, Cible.Cell) Then
                    Direction = Utils.Cells.GetDirection(Player.Fight.Map, Cible.Cell, Player.Cell)
                Else
                    Direction = Basic.Rand(0, 3) * 2 + 1
                End If

                Dim NewCell As Integer = Cible.Cell
                For i As Integer = 1 To Value
                    Dim PossibleNewCell As Integer = Utils.Cells.NextCell(Player.Fight.Map, NewCell, Direction)
                    If Player.Fight.IsValidCell(PossibleNewCell) Then
                        NewCell = PossibleNewCell
                    Else
                        Exit For
                    End If
                Next

                Cible.Cell = NewCell
                Player.Fight.Send("GA0;5;" & Player.Id & ";" & Cible.Id & "," & NewCell)

            Next

        End Sub

        Private Sub EffectTrap()

            Player.Fight.AddTrap(New EffectTrap(Player, Base.SpellID, Base.Value, Value2, Cell))

        End Sub

        Private Sub EffectGlyph()

            Player.Fight.AddGlyph(New EffectTrap(Player, Base.SpellID, Base.Value, Value2, Cell, Base.Value3, Tours))

        End Sub

        Private Sub EffectDeleteBonus()

            For Each Cible As Fighter In Cibles

                Cible.Buffs.DeleteAll()
                Player.Fight.Send("GA0;132;" & Player.Id & ";" & Cible.Id)

            Next

        End Sub

        Private Sub AddChatiment()

            For Each Cible As Fighter In Cibles

                Cible.Buffs.Add(Me.Copy, Cible, BuffType.BuffOnAttack)

            Next


        End Sub


        Public Sub ChangeSkin(ByVal Reverse As Boolean)

            If Reverse Then Value3 = -1
            If Not Reverse Then Tours -= 1

            For Each Cible As Fighter In Cibles

                Dim Skin As Integer = If(Value3 = -1, Cible.Skin, Value3)
                If Value3 <> -1 Then Cible.Buffs.Add(Me.Copy, Cible, BuffType.Skin)
                Player.Fight.Send("GA;149;" & Player.Id & ";" & Cible.Id & "," & Skin & "," & Skin & "," & Tours)

            Next

        End Sub

        Private Sub EffectInvocation()

            If Player.Invocs.Count >= Player.Stats.Stats.MaxInvocation.BridTotal Then
                Exit Sub
            End If

            If Not Player.Fight.IsValidCell(Cell) Then
                Exit Sub
            End If

            Dim Template = MonstersHandler.GetTemplate(Value)

            If Template Is Nothing Then
                Exit Sub
            End If

            Dim MonsterGrade = Template.LevelList.FirstOrDefault(Function(l) l.Grade = Value2)

            If MonsterGrade Is Nothing Then
                Exit Sub
            End If

            Dim Monster As New Monster(MonsterGrade)

            Player.Fight.AddInvocation(Monster, Player, Cell)

        End Sub
    End Class


End Namespace