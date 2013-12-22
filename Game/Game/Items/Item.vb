Imports System.Text
Imports Podemu.Utils.Basic

Namespace Game
    Public Class Item

        Private EffectList As New List(Of ItemEffect)
        Public TemplateID As Integer
        Public UniqueID As Integer
        Public Quantity As Integer
        Public Position As Integer
        Public Args As String

        Public ReadOnly Property GetTemplate() As ItemTemplate
            Get
                Return ItemsHandler.GetItemTemplate(TemplateID)
            End Get
        End Property

        Public Function EffectsInfos() As String

            Return String.Join(",", EffectList.Select(Function(e) ApplyItemEffect(e).ToString()))

        End Function

        Public Function ApplyItemEffect(ByVal Effect As ItemEffect) As ItemEffect

            Select Case Effect.EffectID

                Case Game.Effect.MountOwner
                    Effect.Value1 = UniqueID

                Case Game.Effect.LivingMood
                    Dim LastEat As ItemEffect = GetEffect(Game.Effect.LastEat)

                    If LastEat IsNot Nothing Then
                        Dim LastEatDate As New Date(LastEat.Value1, Math.Truncate(LastEat.Value2 / 100), LastEat.Value2 Mod 100, Math.Truncate(LastEat.Value3 / 100), LastEat.Value3 Mod 100, 0)
                        If Now.Subtract(LastEatDate).TotalHours < 48 Then
                            Effect.Value3 = 1
                        Else
                            Effect.Value3 = 0
                        End If
                    Else
                        Effect.Value3 = 0
                    End If


            End Select

            Return Effect
        End Function

        Public Overrides Function ToString() As String

            Dim ePosition As String = If(Position = -1, "", DeciToHex(Position))

            Return String.Concat(DeciToHex(UniqueID), "~", DeciToHex(TemplateID), "~", _
                DeciToHex(Quantity), "~", ePosition, "~", EffectsInfos())

        End Function
       
        Public Function GetEffect(ByVal EffectId As Effect) As ItemEffect
            Return EffectList.FirstOrDefault(Function(f) f.EffectID = EffectId)
        End Function

        Public Function HasEffect(ByVal EffectId As Effect) As Boolean
            Return GetEffect(EffectId) IsNot Nothing
        End Function

        Public Function GetEffects() As IEnumerable(Of ItemEffect)
            Return EffectList
        End Function

        Public Sub AddEffect(ByVal Effect As ItemEffect)
            Effect.Item = Me
            EffectList.Add(Effect)
        End Sub

        Public Sub RemoveEffect(ByVal EffectId As Effect)
            Dim eff As ItemEffect = GetEffect(EffectId)
            If eff IsNot Nothing Then EffectList.Remove(eff)
        End Sub

        Public Sub RemoveEffect(ByVal Effect As ItemEffect)
            EffectList.Remove(Effect)
        End Sub

        Public Function ToSaveString() As String

            Dim ePosition As String = IIf(Position = -1, "", DeciToHex(Position))

            Return DeciToHex(TemplateID) & "~" & DeciToHex(Quantity) & _
                "~" & ePosition & "~" & EffectsInfos() & "~" & Args

        End Function

        Public Function ToPricedItem(ByVal Price As Integer) As PricedItem

            Dim PricedItem As New PricedItem
            PricedItem.Position = -1
            PricedItem.Price = Price
            PricedItem.Quantity = Quantity
            PricedItem.TemplateID = TemplateID
            PricedItem.Args = Args

            For Each EffectToAdd As ItemEffect In EffectList
                Dim NewEffect As ItemEffect = EffectToAdd.Copy
                PricedItem.EffectList.Add(NewEffect)
            Next

            Return PricedItem

        End Function

        Public Function Copy() As Item

            Dim CopiedItem As New Item
            CopiedItem.TemplateID = Me.TemplateID
            CopiedItem.UniqueID = ItemsHandler.GetUniqueID
            CopiedItem.Quantity = Me.Quantity
            CopiedItem.Position = Me.Position
            CopiedItem.Args = Me.Args
            For Each EffectToAdd As ItemEffect In EffectList
                Dim NewEffect As ItemEffect = EffectToAdd.Copy
                CopiedItem.EffectList.Add(NewEffect)
            Next

            Return CopiedItem

        End Function

        Public Function IsSuperposable(ByVal Item As Item) As Boolean
            Return TemplateID = Item.TemplateID AndAlso Position = Item.Position AndAlso EffectsInfos() = Item.EffectsInfos() AndAlso Args = Item.Args
        End Function

        Public Function GetEffectList() As List(Of SpellEffect)

            Dim EffectSpellList As New List(Of SpellEffect)
            For Each Effect As ItemEffect In EffectList
                If Effect.IsWeaponEffect Then
                    EffectSpellList.Add(Effect.ConvertToSpellEffect)
                End If
            Next
            Return EffectSpellList

        End Function

        Public ReadOnly Property IsLivingItem() As Boolean
            Get
                Return GetEffect(Effect.LivingType) IsNot Nothing
            End Get
        End Property

        Public Shared Function GetItemById(ByVal ItemId As Integer) As Item
            For Each Map As KeyValuePair(Of Integer, World.Map) In World.MapsHandler.ListMaps
                If Not Map.Value.Initialized Then Continue For

                For Each Client As GameClient In Map.Value.ListGameCharacter
                    If Client.Character.Items.HasItemID(ItemId) Then
                        Return Client.Character.Items.GetItemFromID(ItemId)
                    End If
                Next

                For Each Merchant As Merchant In Map.Value.MerchantList
                    If Merchant.MerchantBag.HasItem(ItemId) Then
                        Return Merchant.MerchantBag.GetItem(ItemId)
                    End If
                Next

            Next
            Return Nothing
        End Function

    End Class
End Namespace