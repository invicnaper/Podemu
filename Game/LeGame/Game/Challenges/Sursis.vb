Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class Sursis : Inherits Challenge

        Sub New(ByVal Fight As Fight)
            MyBase.New(4, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = True

            Dim Monsters = Fight.Fighters.Where(Function(f) f.IsMonster).ToArray()
            TargetId = Monsters(Basic.Rand(0, Monsters.Count() - 1)).Id

        End Sub


        Public Overrides Sub CheckDeath(ByVal Fighter As World.Fighter)

            Dim Monsters = Fight.Fighters.LongCount(Function(f) f.IsMonster)

            If Fighter.Id = 0 Then

                If Monsters = 0 Then
                    Ok()
                Else
                    Fail()
                End If

            End If
        End Sub

    End Class
End Namespace