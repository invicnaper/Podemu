Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class SpellsHandler

        Public Shared ListOfSpells As New List(Of Spell)(2000)
        Private Shared SpellsToLearn As New List(Of SpellLearn)(250)

        Public Shared Function SpellExist(ByVal SpellId As Integer) As Boolean
            For Each Spell As Spell In ListOfSpells
                If Spell.SpellId = SpellId Then Return True
            Next
            Return False
        End Function

        Public Shared Function GetSpell(ByVal SpellId As Integer) As Spell
            For Each Spell As Spell In ListOfSpells
                If Spell.SpellId = SpellId Then Return Spell
            Next
            Return Nothing
        End Function

        Private Shared Sub ParseEffect(ByVal Level As SpellLevel, ByVal Data As String, ByVal CC As Boolean)

            Dim EffectList() As String = Data.Split("|")

            For i As Integer = 0 To EffectList.Length - 1

                Dim ActData As String = EffectList(i)
                If ActData = "-1" OrElse ActData = "" Then Continue For

                Dim NewEffect As New SpellEffect

                NewEffect.SpellID = Level.This.SpellId
                Dim Infos() As String = ActData.Split(";")

                NewEffect.Effect = Infos(0)
                NewEffect.Value = Infos(1)
                NewEffect.Value2 = Infos(2)
                NewEffect.Value3 = Infos(3)
                If Infos.Length >= 5 Then
                    NewEffect.Tours = Infos(4)
                Else
                    NewEffect.Tours = 0
                End If
                If Infos.Length >= 6 Then
                    NewEffect.Chance = Infos(5)
                Else
                    NewEffect.Chance = -1
                End If
                If Infos.Length >= 7 Then
                    NewEffect.Str = Infos(6)
                Else
                    NewEffect.Str = "0d0+0"
                End If
                If Infos.Length >= 8 Then
                    NewEffect.Cibles.Update(Infos(7))
                End If

                If CC Then
                    Level.CriticEffectList.Add(NewEffect)
                Else
                    Level.EffectList.Add(NewEffect)
                End If

            Next

        End Sub

        Private Shared Sub ParseLevel(ByVal Spell As Spell, ByVal Data As String, ByVal Level As Integer)

            If Data = "-1" Then
                Spell.LevelList.Add(Nothing)
                Exit Sub
            End If

            Dim NewLevel As New SpellLevel(Spell)

            Dim Stats() As String = Data.Split(",")
            Dim Effets As String = Stats(0)
            Dim CCEffets As String = Stats(1)

            NewLevel.CostPA = 6
            If IsNumeric(Stats(2)) Then
                NewLevel.CostPA = Stats(2)
            End If

            NewLevel.MinPO = Stats(3)
            NewLevel.MaxPO = Stats(4)
            NewLevel.TauxCC = Stats(5)
            NewLevel.TauxEC = Stats(6)
            NewLevel.InLine = (Stats(7).Trim = "true")
            NewLevel.LineOfVision = (Stats(8).Trim = "true")
            NewLevel.EmptyCell = (Stats(9).Trim = "true")
            NewLevel.ModifablePO = (Stats(10).Trim = "true")
            ' ?
            NewLevel.MaxPerTurn = Stats(12)
            NewLevel.MaxPerPlayer = Stats(13)
            NewLevel.TurnNumber = Stats(14)
            NewLevel.TypePortee = Stats(15).Trim
            NewLevel.MyLevel = Level
            NewLevel.ECEndTurn = (Stats(19).Trim = "true")

            ParseEffect(NewLevel, Effets, False)
            ParseEffect(NewLevel, CCEffets, True)

            Spell.LevelList.Add(NewLevel)

        End Sub

        Private Shared Sub SetupLearnSpells()

            SyncLock Sql.OthersSync

                SpellsToLearn.Clear()

                Dim SQLText As String = "SELECT * FROM spells_learn"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewLearn As New SpellLearn

                    NewLearn.Race = Result("Classe")
                    NewLearn.Level = Result("Level")
                    NewLearn.SpellId = Result("SpellId")
                    NewLearn.Position = Result("Position")

                    SpellsToLearn.Add(NewLearn)

                End While

                Result.Close()

            End SyncLock

        End Sub

        Public Shared Sub SetupSpells()

            MyConsole.StartLoading("Loading spells from database...")

            SyncLock Sql.OthersSync

                ListOfSpells.Clear()

                Dim SQLText As String = "SELECT * FROM spells_data"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewSpell As New Spell

                    NewSpell.SpellId = Result("id")
                    NewSpell.Name = Encoding.AsciiDecoder(Result("nom"))

                    NewSpell.AnimationId = Result("sprite")
                    NewSpell.SpriteInfos = Result("spriteInfos")

                    For i As Integer = 1 To 6
                        ParseLevel(NewSpell, Result("lvl" & i), i)
                    Next

                    ListOfSpells.Add(NewSpell)

                End While

                Result.Close()

            End SyncLock

            SetupLearnSpells()

            MyConsole.StopLoading()

            MyConsole.Status("'@" & ListOfSpells.Count & "@' spells and '@" & SpellsToLearn.Count & "@' to learn loaded from database")

        End Sub

        Public Shared Sub LearnSpells(ByVal Character As Character)

            Dim ToLearn As List(Of SpellLearn) = _
                (From Item In SpellsToLearn Select Item Where Item.Race = Character.Classe And _
                Item.Level <= Character.Player.Level And (Not Character.Spells.HaveSpell(Item.SpellId))).ToList

            For Each Spell As SpellLearn In ToLearn
                Character.Spells.AddSpell(Spell.SpellId, 1, Spell.Position)
            Next

        End Sub

    End Class
End Namespace