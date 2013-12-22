Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Game

Namespace World
    Public Class Paddock

        Public Template As Game.PaddockTemplate
        Public Map As Map
        Public Mounts As New List(Of Mount)
        Public Items As New List(Of Int32)
        Public Price As Int32
        Public Owner As Guild

        Private Initialized As Boolean = False

        Public Sub New(ByVal Template As Game.PaddockTemplate, ByVal Map As Map, ByVal Mounts As List(Of Mount), ByVal Items As List(Of Integer), ByVal Price As Integer, ByVal Owner As Guild)

            Me.Template = Template
            Me.Map = Map
            Me.Mounts = Mounts
            Me.Items = Items
            Me.Price = Price
            Me.Owner = Owner

            AddHandler Map.OnInitialization, AddressOf OnInit

        End Sub


#Region "Owner"

        Public ReadOnly Property IsPublic() As Boolean
            Get
                Return Template.IsPublic
            End Get
        End Property

        Public ReadOnly Property HasOwner() As Boolean
            Get
                Return Owner IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property OwnerString() As String
            Get
                Return If(HasOwner, Owner.GuildID.ToString(), If(IsPublic, "-1", "0"))
            End Get
        End Property

        Public Function IsMine(ByVal Client As GameClient) As Boolean
            Return If(HasOwner, Owner.GuildID = Client.Character.Guild.GuildID, False)
        End Function

#End Region

#Region "Sell"

        Public ReadOnly Property IsSold As Boolean
            Get
                Return (Price <> 0) Or (Owner Is Nothing AndAlso Template.IsPublic)
            End Get
        End Property

        Public ReadOnly Property CurrentPrice As Int32
            Get
                Return If(Owner Is Nothing, Template.Price, Price)
            End Get
        End Property

#End Region

#Region "Display"

        Public ReadOnly Property PaddockDisplayInfo() As String
            Get
                Return OwnerString & ";" & If(IsSold, CurrentPrice, 0) & ";" & Template.MountPlace & ";" & Template.ItemPlace & ";" & If(HasOwner, Owner.Name, "") & ";" & If(HasOwner, Owner.Emblem.ToString, "") & ";"
            End Get
        End Property

#End Region

#Region "Paddock Content Display"

        Public Function PublicPaddockContentInfo(ByVal Client As GameClient) As String
            Return String.Concat(String.Join(";", ShedMounts(Client).Select(Function(m) m.MountInfo)), "~", String.Join(";", MountParkMounts(Client).Select(Function(m) m.MountInfo)))
        End Function

        Public Function PrivatePaddockContentInfo(ByVal Client As GameClient) As String
            Return String.Concat(String.Join(";", ShedMounts(Client).Select(Function(m) m.MountInfo)), "~", String.Join(";", MountParkGuildMounts(Client).Select(Function(m) m.MountInfo)))
        End Function

        Private Sub SendPaddockContentInfo(ByVal client As GameClient)
            If IsPublic Then
                client.Send("ECK16|" & PublicPaddockContentInfo(client))
            ElseIf IsMine(client) Then
                client.Send("ECK16|" & PrivatePaddockContentInfo(client))
            Else
                client.Send("ECE")
            End If
        End Sub

        Public Sub OpenPaddock(ByVal client As GameClient)
            SendPaddockContentInfo(client)
            CheckBabies(client)
        End Sub

#End Region

#Region "Inventory"

        Private Function InventoryMount(ByVal client As GameClient) As Mount
            Return client.Character.Mount
        End Function

        Public Function HasMountInInventory(ByVal client As GameClient) As Boolean
            Return InventoryMount(client) IsNot Nothing
        End Function

        Public Sub AddToInventory(ByVal Client As GameClient, ByVal Mount As Mount)
            Client.Character.Mount = Mount
            Mount.Equip(Client)
        End Sub

        Public Sub RemoveFromInventory(ByVal Client As GameClient, ByVal Mount As Mount)
            If Client.Character.State.IsMounted Then
                Mount.Unride(Client)
            End If
            Mount.Unequip(Client)
        End Sub

#End Region

#Region "Shed"

        Private Shared Sub SendAddMountToShed(ByVal client As GameClient, ByVal Mount As Mount)
            client.Send("Ee+" & Mount.MountInfo)
            client.Send("monture a " & Mount.OwnerName)
        End Sub

        Private Sub SendRemoveMountFromShed(ByVal client As GameClient, ByVal Mount As Mount)
            client.Send("Ee-" & Mount.Id)
        End Sub

        Private Function ShedMounts(ByVal client As GameClient) As List(Of Mount)
            Return client.Character.Mounts
        End Function

        Public Function GetMountFromShed(ByVal client As GameClient, ByVal id As Integer) As Mount
            Return client.Character.Mounts.FirstOrDefault(Function(m) m.Id = id)
        End Function

        Public Sub AddToShed(ByVal Client As GameClient, ByVal Mount As Mount)
            Client.Character.Mounts.Add(Mount)
            SendAddMountToShed(Client, Mount)
        End Sub

        Public Sub RemoveFromShed(ByVal Client As GameClient, ByVal Mount As Mount)
            Client.Character.Mounts.Remove(Mount)
            SendRemoveMountFromShed(Client, Mount)
        End Sub

        Public Sub CheckBabies(ByVal Client As GameClient)
            For Each Mount As Mount In ShedMounts(Client)
                Dim babies = Mount.GetBabies(Client)
                If babies Is Nothing Then Continue For
                For Each baby As Mount In babies
                    AddToShed(Client, baby)
                Next
            Next
        End Sub

#End Region

#Region "MountPark"

        Private Sub SendAddMountToMountPark(ByVal client As GameClient, ByVal Mount As Mount)
            client.Send("Ef+" & Mount.MountInfo)
        End Sub

        Private Sub SendRemoveMountFromMountPark(ByVal client As GameClient, ByVal Mount As Mount)
            client.Send("Ef-" & Mount.Id)
        End Sub

        Public Function GetRandomPossibleCase() As Integer
            Return GetMountPossibleCase(Rnd() * (GetMountPossibleCase().Count - 1))
        End Function

        Public Sub AddToMountPark(ByVal Client As GameClient, ByVal Mount As Mount)
            Mounts.Add(Mount)
            Mount.Paddock = Me
            Mount.CellId = GetMountPossibleCase().FirstOrDefault(Function(c) c = Template.CellId + 16 Or c = Template.CellId - 14 Or c = Template.CellId + 20 Or c = Template.CellId - 18)
            Mount.Direction = If(Rnd() < 0.5, 1, 5)

            SendAddMountToMountPark(Client, Mount)
            Map.AddMount(Client, Mount)
        End Sub

        Public Sub RemoveFromMountPark(ByVal Client As GameClient, ByVal Mount As Mount)
            Mounts.Remove(Mount)
            Mount.Paddock = Nothing
            SendRemoveMountFromMountPark(Client, Mount)
            Map.DelMount(Client, Mount)
        End Sub

        Public Function GetMountFromMountPark(ByVal id As Integer) As Mount
            Return Mounts.FirstOrDefault(Function(m) m.Id = id)
        End Function

        Public Function GetFecondableMountOnCell(ByVal CellId As Integer, ByVal Sex As Integer) As Mount
            Return Mounts.FirstOrDefault(Function(m) m.CellId = CellId AndAlso m.Sex = Sex AndAlso m.Fecondable)
        End Function

        Public Function ContainsMount(ByVal id As Integer) As Boolean
            Return GetMountFromMountPark(id) IsNot Nothing
        End Function

        Private Function MountParkMounts(ByVal client As GameClient) As List(Of Mount)
            Return Mounts.Where(Function(m) m.OwnerName = client.Character.Name).ToList()
        End Function

        Private Function MountParkGuildMounts(ByVal client As GameClient) As List(Of Mount)
            Return Nothing
        End Function

        Public Sub PlaceMountOnRandomCase(ByVal Mount As Mount)
            Mount.CellId = GetRandomPossibleCase()
            Mount.Direction = Rnd() * 7
        End Sub

        Public Sub ApplyCollision(ByVal Mount As Mount)

            'Apply Accouplements
            Dim otherMount As Mount = GetFecondableMountOnCell(Mount.CellId, If(Mount.Sex = 0, 1, 0))
            If otherMount IsNot Nothing Then
                Threading.Thread.Sleep(1000)
                If otherMount.Sex = 1 Then
                    otherMount.Accouple(Mount)
                Else
                    Mount.Accouple(otherMount)
                End If
            End If

            'Apply Objets

        End Sub

        Public Sub OnInit()

            For Each Mount In Mounts
                PlaceMountOnRandomCase(Mount)
            Next

        End Sub

#End Region

#Region "Certificats"

        Public Sub AddCertificate(ByVal Client As GameClient, ByVal Cert As Item)
            Client.Character.Items.AddItem(Client, Cert)
        End Sub

        Public Sub RemoveCertificate(ByVal Client As GameClient, ByVal Cert As Item)
            Client.Character.Items.RemoveItem(Client, Cert, 1)
            Client.Character.Items.RefreshItems(Client)
        End Sub

        Public Function ToCertificate(ByVal Client As GameClient, ByVal Mount As Mount) As Item
            Dim cert As Item = Game.ItemsHandler.GetItemTemplate(7806).GenerateItem(1)
            cert.AddEffect(ItemEffect.FromString(cert, "3e3####"))
            cert.AddEffect(ItemEffect.FromString(cert, "3e5####" & Mount.Name))
            cert.AddEffect(ItemEffect.FromString(cert, "3e4####" & Client.Character.Name))
            cert.AddEffect(ItemEffect.FromString(cert, "3e6#14#17#3b#"))
            cert.Args = Mount.SaveString

            Return cert
        End Function

        Public Function FromCertificate(ByVal Client As GameClient, ByVal Cert As Item) As Mount
            Return New Mount(Cert.Args)
        End Function

        Public Function GetCertificate(ByVal Client As GameClient, ByVal CertId As Integer) As Item
            Return Client.Character.Items.GetItemFromID(CertId)
        End Function


#End Region

#Region "Errors"

        Private Sub SendInventoryNotEmpty(ByVal client As GameClient, ByVal price As Integer)
            client.Send("ReE-")
        End Sub

        Private Sub SendAlreadyHaveOne(ByVal client As GameClient, ByVal price As Integer)
            client.Send("ReE+")
        End Sub

        Private Sub SendUnknowError(ByVal client As GameClient, ByVal price As Integer)
            client.Send("ReEr")
        End Sub

#End Region

#Region "Mounts Management"

        Private ListOfTradder As New List(Of GameClient)

        Public Sub BeginTrade(ByVal Client As GameClient)
            ListOfTradder.Add(Client)
            Client.Character.State.BeginTrade(Trading.Paddock, Me)
            OpenPaddock(Client)
        End Sub

        Public Sub EndTrade(ByVal Client As GameClient)
            ListOfTradder.Remove(Client)
            Client.Character.State.EndTrade()
            Client.Send("EV")
        End Sub

        Public Sub MountFromShedToMountPark(ByVal client As GameClient, ByVal MountId As Integer)
            Dim shedMount As Mount = GetMountFromShed(client, MountId)
            If (shedMount IsNot Nothing) Then
                RemoveFromShed(client, shedMount)
                AddToMountPark(client, shedMount)
            Else
                client.SendNormalMessage("Impossible de trouver la monture")
            End If
        End Sub

        Public Sub MountFromMountParkToShed(ByVal client As GameClient, ByVal MountId As Integer)
            Dim mountParkMount As Mount = GetMountFromMountPark(MountId)
            If (mountParkMount IsNot Nothing) Then
                RemoveFromMountPark(client, mountParkMount)
                AddToShed(client, mountParkMount)
            Else
                client.SendNormalMessage("Impossible de trouver la monture")
            End If
        End Sub

        Public Sub MountFromInventoryToShed(ByVal client As GameClient)
            Dim shedMount As Mount = InventoryMount(client)
            If (shedMount IsNot Nothing) Then
                RemoveFromInventory(client, shedMount)
                AddToShed(client, shedMount)
            Else
                client.SendNormalMessage("Impossible de trouver la monture")
            End If
        End Sub

        Public Sub MountFromShedToInventory(ByVal client As GameClient, ByVal MountId As Integer)
            Dim shedMount As Mount = GetMountFromShed(client, MountId)
            If (shedMount IsNot Nothing) Then
                RemoveFromShed(client, shedMount)
                If HasMountInInventory(client) Then
                    MountFromInventoryToShed(client)
                End If
                AddToInventory(client, shedMount)
            Else
                client.SendNormalMessage("Impossible de trouver la monture")
            End If
        End Sub

        Public Sub MountFromShedToCertificate(ByVal client As GameClient, ByVal MountId As Integer)
            Dim shedMount As Mount = GetMountFromShed(client, MountId)
            If (shedMount IsNot Nothing) Then
                RemoveFromShed(client, shedMount)
                AddCertificate(client, ToCertificate(client, shedMount))
            Else
                client.SendNormalMessage("Impossible de trouver la monture")
            End If
        End Sub

        Public Sub MountFromCertificateToShed(ByVal client As GameClient, ByVal CertificatId As Integer)
            Dim cert As Item = GetCertificate(client, CertificatId)
            If cert IsNot Nothing Then
                Dim certMount As Mount = FromCertificate(client, cert)
                If (certMount IsNot Nothing) Then
                    RemoveCertificate(client, cert)
                    AddToShed(client, certMount)
                Else
                    client.SendNormalMessage("Impossible de trouver la monture correspondance")
                End If
            Else
                client.SendMessage("Ce certificat n'existe pas")
            End If
        End Sub

#End Region

#Region "Paddock Purchase"

        Private Sub SendOpenBuyInterface(ByVal client As GameClient)
            client.Send("RD|" & CurrentPrice)
        End Sub

        Private Sub SendCloseBuyInterface(ByVal client As GameClient)
            client.Send("Rv")
        End Sub

        Public Sub OpenBuyInterface(ByVal client As GameClient)
            SendOpenBuyInterface(client)
        End Sub

        Public Sub CloseBuyInterface(ByVal client As GameClient)
            SendCloseBuyInterface(client)
        End Sub

        Public Sub Buy(ByVal client As GameClient, ByVal Price As Integer)
            If IsSold AndAlso Price = CurrentPrice Then


            Else
                client.SendNormalMessage("Cette enclos n'est pas à vendre ou vous n'avez pas les moyens de l'acheter")
            End If
        End Sub

        Public Sub Sell(ByVal client As GameClient, ByVal Price As Integer)
            If Price > 0 Then

            Else
                client.SendNormalMessage("Précisez un prix superieur à 0kamas")
            End If
        End Sub

#End Region

#Region "MountPark Display"

        Public Sub DataInfo(ByVal client As GameClient, ByVal SpriteId As Integer)


        End Sub

        Public Sub RemoveObject(ByVal client As GameClient, ByVal CellId As Integer)

        End Sub

        Public Function GetMountPossibleCase() As List(Of Integer)
            Return Map.CellInfos.Where(Function(k) k.Value.Movement = MovementEnum.PADDOCK).Select(Function(c) c.Key).ToList()
        End Function

        Public ReadOnly Property MountParkDisplayInfos() As String
            Get
                Return "|+" & String.Join("|+", (From Mount In Mounts Select Mount.MountDisplayInfo).ToArray())
            End Get
        End Property

#End Region

#Region "Save"

        Public Sub Save()

            SyncLock Sql.CharactersSync

                Dim exists As Integer = New MySqlCommand("SELECT COUNT(*) FROM paddocks WHERE id=" & Template.Id, Sql.Characters).ExecuteScalar()

                Dim SQLText As String = ""

                If (exists) Then
                    SQLText = "UPDATE paddocks SET mounts=@mounts, items=@items, guild_id=@guild_id, price=@price WHERE id=@id"
                Else
                    SQLText = "INSERT INTO paddocks(id, mounts, items, guild_id, price) VALUES(@id, @mounts, @items, @guild_id, @price )"
                End If

                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Characters)

                SQLCommand.Parameters.Add(New MySqlParameter("@id", Template.Id))
                SQLCommand.Parameters.Add(New MySqlParameter("@mounts", String.Join("~", Mounts.Select(Function(m) m.SaveString))))
                SQLCommand.Parameters.Add(New MySqlParameter("@items", String.Join("*", Items.Select(Function(i) i))))
                SQLCommand.Parameters.Add(New MySqlParameter("@guild_id", If(HasOwner, Owner.GuildID.ToString(), "")))
                SQLCommand.Parameters.Add(New MySqlParameter("@price", Price))

                SQLCommand.ExecuteNonQuery()

            End SyncLock

        End Sub

#End Region

    End Class
End Namespace