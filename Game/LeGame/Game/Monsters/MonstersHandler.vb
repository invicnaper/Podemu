Imports MySql.Data.MySqlClient
Imports Podemu.Utils

Namespace Game
    Public Class MonstersHandler

        Public Shared TemplateList As New Dictionary(Of Integer, MonsterTemplate)(600)

        Public Shared Function Exist(ByVal TemplateId As Integer) As Boolean
            Return TemplateList.ContainsKey(TemplateId)
        End Function

        Public Shared Function GetTemplate(ByVal TemplateId As Integer) As MonsterTemplate
            Return TemplateList(TemplateId)
        End Function

        Private Shared Sub SetupLevels()

            Dim SQLText As String = "SELECT * FROM creature_grades"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read

                Dim NewLevel As New MonsterLevel

                NewLevel.TemplateID = Result("Mob_Id")
                NewLevel.Level = Result("Level")
                NewLevel.MaxPA = Result("pa")
                NewLevel.MaxPM = Result("pm")
                NewLevel.VieMaximum = Result("Life")
                NewLevel.Size = 100
                NewLevel.Base.Sagesse.Base = Result("Sagesse")
                NewLevel.Base.Force.Base = Result("Force")
                NewLevel.Base.Intelligence.Base = Result("Intelligence")
                NewLevel.Base.Chance.Base = Result("Chance")
                NewLevel.Base.Agilite.Base = Result("Agilite")
                NewLevel.Resistances.PercentNeutre.Base = Result("rNeutral")
                NewLevel.Resistances.PercentTerre.Base = Result("rEarth")
                NewLevel.Resistances.PercentFeu.Base = Result("rFire")
                NewLevel.Resistances.PercentEau.Base = Result("rWater")
                NewLevel.Resistances.PercentAir.Base = Result("rAir")
                NewLevel.Grade = Result("grade")

                Dim Spells As String = Result("spells").ToString
                If Spells <> "" Then
                    Dim DataSpells() As String = Spells.Split(";")
                    For Each Spell As String In DataSpells
                        Dim SpellData() As String = Spell.Split("@")
                        NewLevel.SpellList.Add(SpellsHandler.GetSpell(SpellData(0)).AtLevel(SpellData(1)))
                    Next
                End If

                If TemplateList.ContainsKey(NewLevel.TemplateID) Then
                    TemplateList(NewLevel.TemplateID).LevelList.Add(NewLevel)
                End If

            End While

            Result.Close()

        End Sub

        Public Shared Sub SetupMonsters()

            Utils.MyConsole.StartLoading("Loading monsters from database ...")

            SyncLock Sql.OthersSync

                TemplateList.Clear()

                Dim SQLText As String = "SELECT * FROM creature_db"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewTemplate As New MonsterTemplate

                    NewTemplate.Id = Result("ID")
                    NewTemplate.Nom = Result("Name")
                    Dim Colors = Result.GetString("Colors").Split(",")
                    NewTemplate.Color(0) = If(Colors(0) = "-1", -1, Basic.HexToDeci(Colors(0)))
                    NewTemplate.Color(1) = If(Colors(1) = "-1", -1, Basic.HexToDeci(Colors(1)))
                    NewTemplate.Color(2) = If(Colors(2) = "-1", -1, Basic.HexToDeci(Colors(2)))
                    NewTemplate.Skin = Result("Gfxid")
                    Dim Kamas As String = Result("Kamas_Dropped")
                    If Kamas.Contains(";") Then
                        NewTemplate.MinKamas = Kamas.Split(";")(0)
                        NewTemplate.MaxKamas = Kamas.Split(";")(1)
                    End If
                    NewTemplate.AI = Result("AI_Type")

                    TemplateList.Add(NewTemplate.Id, NewTemplate)

                End While

                Result.Close()

                SetupLevels()

            End SyncLock

            Utils.MyConsole.StopLoading()
            Utils.MyConsole.Status("'@" & TemplateList.Count & "@' monsters loaded from database")

        End Sub


    End Class
End Namespace