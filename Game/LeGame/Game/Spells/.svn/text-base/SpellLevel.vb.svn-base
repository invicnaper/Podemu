Namespace Game
    Public Class SpellLevel

        Public This As Spell

        Sub New(ByVal MySpell As Spell)
            This = MySpell
        End Sub

        Public EffectList As New List(Of SpellEffect)
        Public CriticEffectList As New List(Of SpellEffect)

        Public MyLevel As Integer
        Public CostPA As Integer

        Public MinPO, MaxPO As Integer
        Public TauxCC, TauxEC As Integer
        Public InLine, LineOfVision As Boolean
        Public ModifablePO As Boolean
        Public MaxPerTurn, MaxPerPlayer, TurnNumber As Integer
        Public ECEndTurn As Boolean
        Public EmptyCell As Boolean
        Public TypePortee As String

        Public Function GetScore(ByVal TheClient As World.Fighter, ByVal Cell As Integer) As Integer

            Dim PorteeTypeCC As String = TypePortee.Substring(EffectList.Count * 2)
            Dim PorteeType As String = TypePortee.Substring(0, EffectList.Count * 2)

            Dim Score As Integer = 0

            Dim EffectNum As Integer = 0
            For Each Effect As SpellEffect In EffectList

                Dim EffetPortee As String = PorteeType.Substring(EffectNum * 2, 2)
                Dim Cibles As List(Of World.Fighter) = Effect.Cibles.RemixCibles(TheClient, Utils.Zone.GetCells(TheClient.Fight, Cell, TheClient.Cell, EffetPortee))

                If Effect.Chance > 0 And Effect.Chance < 100 Then
                    Continue For
                End If
                Score += Math.Floor(Effect.GetScore(TheClient, Cibles, Cell))

                EffectNum += 1
            Next


            EffectNum = 0
            For Each Effect As SpellEffect In CriticEffectList

                Dim EffetPortee As String = PorteeTypeCC.Substring(EffectNum * 2, 2)
                Dim Cibles As List(Of World.Fighter) = Effect.Cibles.RemixCibles(TheClient, Utils.Zone.GetCells(TheClient.Fight, Cell, TheClient.Cell, EffetPortee))

                If Effect.Chance > 0 And Effect.Chance < 100 Then
                    Continue For
                End If
                Score += Math.Floor(Effect.GetScore(TheClient, Cibles, Cell) / TauxCC)

                EffectNum += 1
            Next

            Return Score

        End Function

        Public Function HasEffect(ByVal Effect As Effect) As Boolean
            For Each Eff In EffectList
                If Eff.Effect = Effect Then
                    Return True
                End If
            Next
            Return False
        End Function


    End Class
End Namespace