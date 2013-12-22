Imports System.Text
Imports Podemu.Game.Actions

Namespace Game
    Public Class ItemTemplate

        Public EffectList As New List(Of ItemEffect)

        Public UseEffects As SerializedActionList

        Public ID As Integer
        Public Name As String
        Public Type As Integer
        Public Level As Integer

        Public Pods As Integer
        Public Pano As Integer
        Public Price As Integer

        Public CostPA, MaxPO, MinPO, TauxCC, TauxEC As Integer
        Public BonusCC As Integer
        Public TwoHand As Boolean

        Public Conditions As ItemConditions

        Public Usable, Targetable, Forgemageable, IsBuff As Boolean

        Public Function GenerateItem(Optional ByVal Quantity As Integer = 1, Optional ByVal MaxMin As Integer = 0) As Item

            Dim NewItem As New Item

            NewItem.UniqueID = -1
            NewItem.TemplateID = ID
            NewItem.Quantity = Quantity
            NewItem.Position = -1
            For Each EffectToAdd As ItemEffect In EffectList
                Dim EffectAdd As ItemEffect = EffectToAdd.Copy
                If Usable OrElse EffectAdd.HoldEffect(MaxMin) Then _
                    NewItem.AddEffect(EffectAdd)
            Next

            Return NewItem

        End Function

        Public Function GenerateItemFromBank(ByVal Quantity As Integer) As Item

            Dim NewItem As New Item

            NewItem.UniqueID = -1
            NewItem.TemplateID = ID
            NewItem.Quantity = Quantity
            NewItem.Position = -1
            For Each EffectToAdd As ItemEffect In EffectList
                Dim EffectAdd As ItemEffect = EffectToAdd.Copy
                ' NewItem.EffectList.Add(EffectAdd)
            Next

            Return NewItem

        End Function

        Public Function EffectsInfos() As String

            Dim ItemInfo As New stringbuilder

            Dim First As Boolean = True
            For Each Effect As ItemEffect In EffectList
                If First Then
                    First = False
                Else
                    ItemInfo.Append(",")
                End If
                ItemInfo.Append(Effect.ToString)
            Next

            Return ItemInfo.ToString()

        End Function

        Public Overrides Function ToString() As String
            Return ID & ";" & EffectsInfos()
        End Function

        Public ReadOnly Property Portee() As String
            Get
                Select Case Type
                    Case ItemsHandler.Types.MARTEAU
                        Return "Xb"
                    Case ItemsHandler.Types.BATON
                        Return "Tb"
                    Case ItemsHandler.Types.ARBALETE
                        Return "Lc"
                    Case Else
                        Return "Pa"
                End Select
            End Get
        End Property

    End Class
End Namespace