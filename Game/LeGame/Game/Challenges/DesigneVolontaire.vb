Imports Podemu.Utils
Imports Podemu.World

Namespace Game
    Public Class DesigneVolontaire : Inherits Challenge

        Public IsDead As Boolean = False

        Sub New(ByVal Fight As Fight)
            MyBase.New(3, Fight)

            BasicDropBonus = 10
            BasicXpBonus = 10

            TeamDropBonus = 15
            TeamXpBonus = 15

            ShowTarget = True

            Dim Monsters = Fight.Fighters.Where(Function(f) f.IsMonster).ToArray()
            TargetId = Monsters(Basic.Rand(0, Monsters.Count() - 1)).Id

        End Sub


        Public Overrides Function CanSet() As Boolean
            If Fight.Fighters.LongCount(Function(f) f.IsMonster) = 1 Then
                Return False
            End If
            Return True
        End Function

        Public Overrides Sub CheckDeath(ByVal Fighter As World.Fighter)
            If Not IsDead Then
                If Fighter.Id = TargetId Then
                    Ok()
                Else
                    Fail()
                End If
            End If
        End Sub

    End Class
End Namespace