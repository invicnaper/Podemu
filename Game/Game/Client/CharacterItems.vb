﻿Imports MySql.Data.MySqlClient
Imports System.Text
Imports Podemu.Utils
Imports Podemu.Utils.Basic
Imports Podemu.World

Namespace Game
    Public Class CharacterItems

        Public ReadOnly Character As Character
        Public ListOfItems As New List(Of Item)
        Private MyPanoplies As New Dictionary(Of Integer, List(Of Integer))
        Private PanopliesId As New List(Of Integer)
        Private Mount As World.Mount

        Public Pods As Integer = 0
        Private _compteItem As Integer

        Public Sub New(ByVal Character As Character)
            Me.Character = Character
        End Sub

        Property compteItem(ByVal iditem As Integer) As Integer
            Get
                Return _compteItem
            End Get
            Set(ByVal value As Integer)
                _compteItem = value
            End Set
        End Property

        Public Sub ClearItems()
            ListOfItems.Clear()
        End Sub

        Public Function HasItemID(ByVal ItemID As Integer) As Boolean

            For Each TempItem As Item In ListOfItems
                If TempItem.UniqueID = ItemID Then
                    Return True
                End If
            Next

            Return False

        End Function

        Public Function GetItemFromID(ByVal ItemID As Integer) As Item

            For Each TempItem As Item In ListOfItems
                If TempItem.UniqueID = ItemID Then
                    Return TempItem
                End If
            Next

            Return Nothing

        End Function

        Private Function GetTemplateAtPos(ByVal Position As Integer) As String

            For Each TempItem As Item In ListOfItems
                If TempItem.Position = Position Then
                    If TempItem.IsLivingItem Then
                        Dim Template As Integer = TempItem.GetEffect(Effect.LivingGfxId).Value3
                        Dim Skin As Integer = TempItem.GetEffect(Effect.LivingSkin).Value3
                        Return String.Concat(DeciToHex(Template), "~", TempItem.GetTemplate.Type, "~", Skin)
                    Else
                        Return String.Concat(DeciToHex(TempItem.TemplateID))
                    End If

                End If
            Next

            Return String.Empty

        End Function

        Public Function IsObjectOnPos(ByVal Position As Integer) As Boolean

            If Position = -1 Then Return False

            For Each TempItem As Item In ListOfItems
                If TempItem.Position = Position Then Return True
            Next

            Return False

        End Function

        Public Function GetObjectOnPos(ByVal Position As Integer) As Item


            For Each TempItem As Item In ListOfItems
                If TempItem.Position = Position Then Return TempItem
            Next

            Return Nothing

        End Function

        Private Function HasDofusOnPos(ByVal ObjectID As Integer, ByVal Position As Integer) As Boolean

            If Not IsObjectOnPos(Position) Then Return False
            Return GetObjectOnPos(Position).TemplateID = ObjectID

        End Function

        Private Function HasDofusID(ByVal ObjectID As Integer) As Boolean

            Return HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS1) Or _
                HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS2) Or _
                HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS3) Or _
                HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS4) Or _
                HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS5) Or _
                HasDofusOnPos(ObjectID, ItemsHandler.Positions.DOFUS6)

        End Function

        Public ReadOnly Property ItemNumber() As Integer
            Get
                Return ListOfItems.Count
            End Get
        End Property

        Public ReadOnly Property GetItemsIDs() As String
            Get
                Return String.Concat(
                GetTemplateAtPos(ItemsHandler.Positions.ARME), ",", _
                GetTemplateAtPos(ItemsHandler.Positions.COIFFE), ",", _
                GetTemplateAtPos(ItemsHandler.Positions.CAPE), ",", _
                GetTemplateAtPos(ItemsHandler.Positions.FAMILIER), ",", _
                GetTemplateAtPos(ItemsHandler.Positions.BOUCLIER))
            End Get
        End Property

        Public ReadOnly Property GetItemsString() As String
            Get
                Dim ItemSave As New StringBuilder
                Dim First As Boolean = True
                For Each Item As Item In ListOfItems
                    If First Then
                        First = False
                    Else
                        ItemSave.Append(";")
                    End If
                    ItemSave.Append(Item.ToString)
                Next
                Return ItemSave.ToString()
            End Get
        End Property

        Public ReadOnly Property GetItemsSave() As String
            Get
                Dim ItemSave As New StringBuilder
                Dim First As Boolean = True
                For Each Item As Item In ListOfItems
                    If First Then
                        First = False
                    Else
                        ItemSave.Append(";")
                    End If
                    ItemSave.Append(Item.ToSaveString)
                Next
                Return ItemSave.ToString()
            End Get
        End Property

        Public Sub UpdateItem(ByVal client As GameClient, ByVal item As Item)
            client.Send("OC;" & item.ToString())
        End Sub

        Private Sub RefreshItemSets(ByVal Client As GameClient)

            For Each PanoId As Integer In PanopliesId

                If ItemsHandler.SetExist(PanoId) Then
                    Dim NombreItem As Integer = MyPanoplies(PanoId).Count
                    Dim ListItem As New StringBuilder
                    Dim First As Boolean = True
                    For Each ItemID As Integer In MyPanoplies(PanoId)
                        If Not First Then ListItem.Append(";")
                        First = False
                        ListItem.Append(ItemID)
                    Next
                    Dim EffectString As New StringBuilder
                    If NombreItem > 1 Then
                        Dim Template As ItemSetTemplate = ItemsHandler.GetSetTemplate(PanoId)
                        Dim EffectList As List(Of ItemSetEffect) = Template.EffectList(NombreItem)
                        First = True
                        For Each Effect As ItemSetEffect In EffectList
                            If Not First Then EffectString.Append(",")
                            First = False
                            Effect.UseEffect(Client.Character.Player)
                            EffectString.Append(Effect.ToString)
                        Next
                    End If
                    Client.Send("OS+", PanoId, "|", ListItem.ToString, "|", EffectString.ToString)
                End If

            Next

        End Sub

        Public Sub RefreshItems(ByVal Client As GameClient, Optional ByVal First As Boolean = False)

            If Not First Then
                Client.Character.Player.Life -= Client.Character.Player.BonusLife
            End If

            Client.Character.Player.Stats.ResetItemBonus()
            MyPanoplies.Clear()
            PanopliesId.Clear()

            For Each Item As Item In ListOfItems
                If Item.Position <> ItemsHandler.Positions.NONE AndAlso Item.Position < ItemsHandler.Positions.BAR1 Then
                    Dim ItemTemplate As ItemTemplate = ItemsHandler.GetItemTemplate(Item.TemplateID)
                    For Each ItemEffect As ItemEffect In Item.GetEffects
                        ItemEffect.UseEffect(Client.Character.Player)
                    Next
                    If ItemTemplate.Pano > 0 Then
                        If Not MyPanoplies.ContainsKey(ItemTemplate.Pano) Then
                            MyPanoplies.Add(ItemTemplate.Pano, New List(Of Integer))
                            PanopliesId.Add(ItemTemplate.Pano)
                        End If
                        If Not MyPanoplies(ItemTemplate.Pano).Contains(ItemTemplate.ID) Then _
                            MyPanoplies(ItemTemplate.Pano).Add(ItemTemplate.ID)
                    End If
                End If
            Next

            RefreshMount(Client)

            RefreshItemSets(Client)

            Client.Character.Player.PM = Client.Character.Player.MaxPM
            Client.Character.Player.PA = Client.Character.Player.MaxPA

            If Not First Then
                Client.Character.Player.Life += Client.Character.Player.BonusLife
                If Client.Character.Player.Life > Client.Character.Player.MaximumLife Then
                    Client.Character.Player.Life = Client.Character.Player.MaximumLife
                End If
            End If

            Client.Character.SendAccountStats()
            RefreshPods(Client)

        End Sub

        Public Sub RefreshMount(ByVal Client As GameClient)
            If Mount IsNot Nothing Then

                For Each MountEffect As ItemEffect In Mount.Effects
                    MountEffect.UseEffect(Client.Character.Player)
                Next

            End If
        End Sub

        Public Sub RefreshPods(ByVal Client As GameClient)

            Pods = 0

            For Each Item As Item In ListOfItems
                Dim ItemTemplate As ItemTemplate = ItemsHandler.GetItemTemplate(Item.TemplateID)
                Pods += ItemTemplate.Pods * Item.Quantity
            Next

            Client.Character.SendPods()

        End Sub

        Public Sub LoadItem(ByVal TemplateID As String, ByVal Quantity As String, ByVal Position As String, ByVal Effects As String, ByVal Args As String)

            If Not ItemsHandler.ItemExist(Convert.ToInt32(TemplateID, 16)) Then
                Exit Sub
            End If

            Dim NewItem As New Item
            NewItem.UniqueID = ItemsHandler.GetUniqueID
            NewItem.TemplateID = Convert.ToInt32(TemplateID, 16)
            NewItem.Quantity = Convert.ToInt32(Quantity, 16)
            NewItem.Args = Args
            If Position.Length <> 0 Then
                NewItem.Position = Convert.ToInt32(Position, 16)
            Else : NewItem.Position = -1
            End If

            If Effects.Length <> 0 Then
                Dim EffectList() As String = Effects.Split(",")
                For Each Effect As String In EffectList
                    NewItem.AddEffect(ItemEffect.FromString(NewItem, Effect))
                Next
            End If

            ListOfItems.Add(NewItem)

        End Sub

        Public Sub AddItemOffline(ByVal TempItem As Item)

            For Each AlreadyItem As Item In ListOfItems
                If AlreadyItem.IsSuperposable(TempItem) Then
                    AlreadyItem.Quantity += TempItem.Quantity
                    Exit Sub
                End If
            Next

            TempItem.UniqueID = ItemsHandler.GetUniqueID()
            ListOfItems.Add(TempItem)

        End Sub

        Public Sub AddItem(ByVal Client As GameClient, ByVal TempItem As Item)

            For Each AlreadyItem As Item In ListOfItems
                If AlreadyItem.IsSuperposable(TempItem) Then
                    AlreadyItem.Quantity += TempItem.Quantity
                    Client.Send("OQ", AlreadyItem.UniqueID, "|", AlreadyItem.Quantity)
                    RefreshItems(Client)
                    Exit Sub
                End If
            Next

            TempItem.UniqueID = ItemsHandler.GetUniqueID()
            ListOfItems.Add(TempItem)
            Client.Send("OAKO" & TempItem.ToString)
            RefreshItems(Client)

        End Sub

        Public Function GetItemIdByUniqueId(ByVal UniqueId As Integer) As Integer
            For Each TempItem As Item In ListOfItems
                If TempItem.UniqueID = UniqueId Then
                    Return TempItem.TemplateID
                End If
            Next
            Return Nothing
        End Function

        Public Sub RemoveItem(ByVal Client As GameClient, ByVal TempItem As Item, ByVal DeleteQuantity As Integer)

            If TempItem.Quantity > DeleteQuantity Then
                TempItem.Quantity -= DeleteQuantity
                Client.Send("OQ", TempItem.UniqueID, "|", TempItem.Quantity)
                RefreshItems(Client)
                Exit Sub
            End If

            ListOfItems.Remove(TempItem)
            Client.Send("OR" & TempItem.UniqueID)
            RefreshItems(Client)
        End Sub

        Public Sub SetMount(ByVal Client As GameClient, ByVal mount As World.Mount)
            Me.Mount = mount
            RefreshItems(Client)
        End Sub

        Public Sub DeleteItem(ByVal Client As GameClient, ByVal ItemID As Integer, ByVal Quantity As Integer)

            For Each TempItem As Item In ListOfItems
                If TempItem.UniqueID = ItemID Then
                    TempItem.Quantity -= Quantity
                    If TempItem.Quantity <= 0 Then
                        ListOfItems.Remove(TempItem)
                        Client.Send("OR" & TempItem.UniqueID)
                    Else
                        Client.Send("OQ", TempItem.UniqueID, "|", TempItem.Quantity)
                    End If
                    RefreshItems(Client)
                    Exit For
                End If
            Next

        End Sub

        Public Sub MoveItemToPosition(ByVal Client As GameClient, ByVal Item As Item, ByVal Position As Integer, ByVal Quantity As Integer)

            Dim AlreadyItem = GetObjectOnPos(Position)

            If AlreadyItem IsNot Nothing Then
                RemoveItem(Client, AlreadyItem, AlreadyItem.Quantity)
                AlreadyItem.Position = -1
                AddItem(Client, AlreadyItem)
            End If

            RemoveItem(Client, Item, Quantity)
            Dim NewItem As Item = Item.Copy()
            NewItem.Quantity = Quantity
            NewItem.Position = Position
            AddItem(Client, NewItem)

        End Sub

        Public Sub UpdateLook(ByVal Client As GameClient)
            Client.Character.GetMap.Send("Oa", Client.Character.ID, "|", GetItemsIDs())
        End Sub

        Public Sub MoveItem(ByVal Client As GameClient, ByVal ItemID As Integer, ByVal NewPosition As Integer, ByVal Quantity As Integer)

            'Check Mount and register
            If NewPosition = ItemsHandler.Positions.FAMILIER AndAlso Not Client.Character.CheckRegister Then
                If Client.Character.State.IsMounted Then
                    Client.SendNormalMessage("Impossible avec une monture")
                    Exit Sub
                End If
            End If


            Dim Item As Item = GetItemFromID(ItemID)

            If Item Is Nothing Then Exit Sub

            Dim Template As ItemTemplate = Item.GetTemplate

            Dim Type As Integer = Template.Type

            'Check obvi
            If Type = ItemsHandler.Types.OBJET_VIVANT And Item.GetEffect(Effect.LivingGfxId) Is Nothing Then
                Associate(Client, Item, NewPosition)
                Exit Sub
            End If

            'Check Mount Food
            If NewPosition = ItemsHandler.Positions.MOUNT AndAlso Client.Character.Mount IsNot Nothing Then
                If Type = ItemsHandler.Types.POISSON Or Type = ItemsHandler.Types.POISSON_VIDE Then
                    RemoveItem(Client, Item, Quantity)
                    Client.Character.Mount.Energy += 10 * Quantity
                    Client.Character.Mount.Refresh(Client)
                Else
                    Client.SendNormalMessage("Impossible de nourrir votre monture avec cela.")
                End If
                Exit Sub
            End If

            If Not ItemsHandler.TestPosition(Type, Template.Usable Or Template.Targetable, NewPosition) Then
                Client.Send("BN")
                Exit Sub
            End If

            'Wrong quantity
            If Quantity > Item.Quantity Then
                Client.Send("BN")
                Exit Sub
            End If

            If NewPosition <> ItemsHandler.Positions.NONE Then

                ' Check Level
                If Template.Level > Client.Character.Player.Level Then
                    Client.Send("OAEL")
                    Exit Sub
                End If

                'Check Conditions
                If Not Template.Conditions.VerifyConditions(Client.Character) Then
                    Client.Send("Im119|44")
                    Exit Sub
                End If

                'TODO check pods

                'Check set
                If Template.Pano <> 0 AndAlso Template.Type = ItemsHandler.Types.ANNEAU Then
                    If ListOfItems.FirstOrDefault(Function(i) (i.TemplateID = Template.ID AndAlso i.UniqueID <> Item.UniqueID) AndAlso (i.Position = ItemsHandler.Positions.ANNEAU1 Or i.Position = ItemsHandler.Positions.ANNEAU2)) IsNot Nothing Then
                        Client.Send("OAEA")
                        Exit Sub
                    End If
                End If

            End If

            MoveItemToPosition(Client, Item, NewPosition, Quantity)

            UpdateLook(Client)

        End Sub

        Public Sub UseItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal TargetCell As String)

            If TargetCell <> "" Then
                Client.Character.GetMap.GetCharacterOnCell(TargetCell)
            End If

            If Not HasItemID(ItemId) Then
                Client.Send("OUE")
                Exit Sub
            End If

            Dim Item As Item = GetItemFromID(ItemId)
            Dim Template = Item.GetTemplate

            If Not Template.Usable Then
                Client.Send("BN")
                Exit Sub
            End If

            If Not Template.Conditions.VerifyConditions(Client.Character) Then
                Client.Send("Im119|44")
                Exit Sub
            End If

            If Template.UseEffects IsNot Nothing Then

                Template.UseEffects.Process(Client, TargetCell)

                DeleteItem(Client, ItemId, 1)

            Else
                MyConsole.Err("No template in DB for itemtemplate " & Template.ID, False)
                Client.Send("BN")
            End If

        End Sub

        Public Sub Associate(ByVal client As GameClient, ByVal LivingItem As Item, ByVal Position As Integer)

            If Not IsObjectOnPos(Position) Then
                client.Send("BN")
                Exit Sub
            End If

            Dim targetItem = GetObjectOnPos(Position)

            If targetItem.GetTemplate.Type <> LivingItem.GetEffect(Effect.LivingType).Value3 Then
                client.Send("BN")
                Exit Sub
            End If

            Dim GfxEffect As New ItemEffect
            GfxEffect.EffectID = Effect.LivingGfxId
            GfxEffect.Value3 = LivingItem.GetTemplate.ID

            LivingItem.AddEffect(GfxEffect)

            For Each Effect As ItemEffect In LivingItem.GetEffects
                targetItem.AddEffect(Effect)
            Next

            RemoveItem(client, LivingItem, 1)
            UpdateItem(client, targetItem)

            UpdateLook(client)

        End Sub

        Public Sub Dissociate(ByVal client As GameClient, ByVal ItemId As Integer, ByVal Position As Integer)

            Dim Item As Item = GetObjectOnPos(Position)

            If Item.UniqueID <> ItemId Or Not Item.IsLivingItem Then
                client.Send("BN")
                Exit Sub
            End If

            Dim LivingItem As New Item

            Dim LastEatEffect = Item.GetEffect(Effect.LastEat)
            Dim GfxEffect = Item.GetEffect(Effect.LivingGfxId)
            Dim MoodEffect = Item.GetEffect(Effect.LivingMood)
            Dim SkinEffect = Item.GetEffect(Effect.LivingSkin)
            Dim TypeEffect = Item.GetEffect(Effect.LivingType)
            Dim XpEffect = Item.GetEffect(Effect.LivingXp)

            LivingItem.UniqueID = ItemsHandler.GetUniqueID
            LivingItem.TemplateID = GfxEffect.Value3
            LivingItem.Position = -1

            If LastEatEffect IsNot Nothing Then LivingItem.AddEffect(LastEatEffect)
            LivingItem.AddEffect(MoodEffect)
            LivingItem.AddEffect(SkinEffect)
            LivingItem.AddEffect(TypeEffect)
            LivingItem.AddEffect(XpEffect)

            Item.RemoveEffect(LastEatEffect)
            Item.RemoveEffect(GfxEffect)
            Item.RemoveEffect(MoodEffect)
            Item.RemoveEffect(SkinEffect)
            Item.RemoveEffect(TypeEffect)
            Item.RemoveEffect(XpEffect)

            UpdateItem(client, Item)
            AddItem(client, LivingItem)

            UpdateLook(client)

        End Sub

        Public Sub SetLivingSkin(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Position As Integer, ByVal SkinId As Integer)

            Dim Item As Item = GetObjectOnPos(Position)

            If Item.UniqueID <> ItemId Or Not Item.IsLivingItem Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim Level = ExperienceTable.Levels.FirstOrDefault(Function(l) l.Value.Living > Item.GetEffect(Effect.LivingXp).Value3).Key - 1

            If Level <> -1 AndAlso SkinId > Level Then
                Client.Send("BN")
                Exit Sub
            End If

            Item.GetEffect(Effect.LivingSkin).Value3 = SkinId

            UpdateItem(Client, Item)
            UpdateLook(Client)
        End Sub

        Public Sub FeedLiving(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Position As Integer, ByVal FeedId As Integer)

            Dim LivingItem As Item = GetObjectOnPos(Position)
            Dim Food As Item = ListOfItems.FirstOrDefault(Function(i) i.UniqueID = FeedId)

            If LivingItem.UniqueID <> ItemId Or Not LivingItem.IsLivingItem Or Food Is Nothing Or LivingItem.GetTemplate.Type <> Food.GetTemplate.Type Then
                Client.Send("BN")
                Exit Sub
            End If

            Dim LastEat As ItemEffect = LivingItem.GetEffect(Effect.LastEat)

            If LastEat IsNot Nothing Then
                Dim LastEatDate As New Date(LastEat.Value1, Math.Truncate(LastEat.Value2 / 100), LastEat.Value2 Mod 100, Math.Truncate(LastEat.Value3 / 100), LastEat.Value3 Mod 100, 0)
                If Now.Subtract(LastEatDate).TotalHours < 12 Then
                    Client.SendNormalMessage("Cela fait moins de 12heures que vous avez nourri votre Obvijevant, doucement !")
                    Exit Sub
                End If
            Else
                LastEat = New ItemEffect()
                LastEat.EffectID = Effect.LastEat
                LivingItem.AddEffect(LastEat)
            End If

            LastEat.Value1 = Now.Year
            LastEat.Value2 = Now.Month * 100 + Now.Day
            LastEat.Value3 = Now.Hour * 100 + Now.Minute

            LivingItem.GetEffect(Effect.LivingMood).Value3 = 1

            LivingItem.GetEffect(Effect.LivingXp).Value3 += Food.GetTemplate.Level

            RemoveItem(Client, Food, 1)
            UpdateItem(Client, LivingItem)
            Client.SendNormalMessage("Votre Obvijevant apprecie le repas")
        End Sub


        Public Sub DeleteItem2(ByVal Client As GameClient, ByVal ItemID As Integer, ByVal Quantity As Integer)

            For Each TempItem As Item In ListOfItems
                If TempItem.TemplateID = ItemID Then
                    TempItem.Quantity -= Quantity
                    If TempItem.Quantity <= 0 Then
                        ListOfItems.Remove(TempItem)
                        Client.Send("OR" & TempItem.UniqueID)
                    Else
                        Client.Send("OQ" & TempItem.UniqueID & "|" & TempItem.Quantity)
                    End If
                    RefreshItems(Client)
                    Exit For
                End If
            Next

        End Sub

    End Class
End Namespace