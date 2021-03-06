﻿Imports System.Text
Imports Vemu_gs.Utils
Imports Vemu_gs.Game
Namespace World
    Public Class Mount

        Public Shared UidCount As Integer = 5000

        Public Id As Integer
        Public OwnerName As String
        Public Paddock As Paddock
        Public CellId As Integer
        Public Direction As Integer
        Public Type As Game.MountTemplate
        Public IsWild As Boolean
        Public IsCastrate As Boolean
        Public Ancestors(14) As MountAncestor
        Public Capacity As Int32
        Public Name As String
        Public Experience As Int32
        Public Sex As Integer
        Public Stamina As Integer
        Public Maturity As Integer
        Public Energy As Integer
        Public Serenity As Integer
        Public Love As Integer
        Public Fecondation As MountFecondation
        Public Tired As Integer
        Public Reprod As Integer
        Public Percent As Int32

#Region "Ctor"

        Public Sub New(ByVal data As String)

            Dim sData = data.Split(":")

            OwnerName = sData(0)

            Id = UidCount

            UidCount += 1

            Type = MountsHandler.GetTemplate(sData(1))
            IsWild = sData(2)
            IsCastrate = sData(3)

            Dim sAncestors As String() = sData(4).Split("|")
            For i As Integer = 0 To 13
                If sAncestors(i) = "" Then
                    Ancestors(i) = Nothing
                Else
                    Ancestors(i) = New MountAncestor(sAncestors(i))
                End If
            Next

            Capacity = sData(5)
            Name = sData(6)
            Experience = sData(7)
            Sex = sData(8)
            Stamina = sData(9)
            Maturity = sData(10)
            Energy = sData(11)
            Serenity = sData(12)
            Love = sData(13)
            If sData(14) <> "" Then
                Fecondation = New MountFecondation(sData(14))
            End If
            Tired = sData(15)
            Reprod = sData(16)
            Percent = sData(17)

        End Sub

        Public Sub New(ByVal ownerName As String, ByVal type As Integer, ByVal iswild As Boolean, ByVal capacity As Integer, ByVal sex As Integer, ByVal stamina As Integer, ByVal serenity As Integer, ByVal love As Integer, ByVal ancestors As MountAncestor())

            Me.OwnerName = ownerName

            Me.Id = UidCount

            UidCount += 1

            Me.Type = MountsHandler.GetTemplate(type)
            Me.IsWild = iswild
            Me.IsCastrate = False
            Me.Capacity = capacity
            Me.Name = ""
            Me.Sex = sex
            Me.Experience = 0
            Me.Stamina = stamina
            Maturity = 0
            Energy = 0
            Me.Serenity = serenity
            Me.Love = love
            Fecondation = Nothing
            Tired = 0
            Reprod = 0
            Percent = 0
            Me.Ancestors = ancestors
        End Sub

#End Region

#Region "Capacities"

        Public Function HasCapacity(ByVal capacity As MountCapacity) As Boolean
            Return (Me.Capacity And capacity) = capacity
        End Function

        Public Function FromFlag(ByVal capa As Array, ByVal flag As Integer) As Integer
            Dim count As Integer = 1
            For Each value As Integer In capa
                If value < flag Then
                    count += 1
                End If
            Next
            Return count
        End Function

        Public ReadOnly Property Capacities() As List(Of Integer)
            Get
                Dim result As New List(Of Integer)
                Dim capa As Array = [Enum].GetValues(GetType(MountCapacity))
                For Each value As Integer In capa
                    If HasCapacity(value) Then
                        result.Add(FromFlag(capa, value))
                    End If
                Next
                Return result
            End Get
        End Property

        Public ReadOnly Property CapacitiesAsString() As String
            Get
                Return String.Join(",", Capacities)
            End Get
        End Property

#End Region

#Region "Exp"

        Public Property Level As Integer
            Get
                Return MountsHandler.GetLevel(Experience)
            End Get
            Set(ByVal value As Integer)
                Experience = MountsHandler.GetLowerBound(value)
            End Set
        End Property

        Public ReadOnly Property LowBound As Integer
            Get
                Return MountsHandler.GetLowerBound(Level)
            End Get
        End Property

        Public ReadOnly Property UpperBound As Integer
            Get
                Return MountsHandler.GetUpperBound(Level)
            End Get
        End Property

#End Region

#Region "Effects"

        Public ReadOnly Property Effects As ItemEffect()
            Get
                Return Type.Stats.Select(Function(t) t.Multiply(Level)).ToArray()
            End Get
        End Property

        Public ReadOnly Property EffectsAsString As String
            Get
                Dim eff As IEnumerable(Of ItemEffect) = Type.Stats.Select(Function(t) t.Multiply(Level))
                Return String.Join(",", eff.Select(Function(e) e.ToString()))
            End Get
        End Property

#End Region

#Region "Ancestors"

        Public ReadOnly Property AncestorsString As String
            Get
                Dim result As New stringbuilder
                For i As Integer = 0 To 13
                    If Ancestors(i) Is Nothing Then
                        result.Append("0,")
                    Else
                        result.Append(Ancestors(i).TypeId & ",")
                    End If
                Next
                Return result.ToString() & Type.Id
            End Get
        End Property

        Public ReadOnly Property AncestorsSaveString As String
            Get
                Dim result As New StringBuilder
                For i As Integer = 0 To 13
                    If Ancestors(i) Is Nothing Then
                        result.Append("0*0|")
                    Else
                        result.Append(Ancestors(i).SaveString & "|")
                    End If
                Next
                Return result.ToString() & Type.Id
            End Get
        End Property

#End Region

#Region "Mountable"

        Public ReadOnly Property IsMountable() As Byte
            Get
                If (Maturity = Type.MaxMaturity AndAlso Not IsWild) Then
                    Return 1
                Else
                    Return 0
                End If
            End Get
        End Property

#End Region

#Region "Pods"

        Public ReadOnly Property MaxPods() As Int32
            Get
                Return Type.StartPods + Type.LevelPods * Level
            End Get
        End Property

#End Region

#Region "Energy"

        Public ReadOnly Property MaxEnergy() As Int32
            Get
                Return Type.StartEnergy + Type.LevelEnergy * Level
            End Get
        End Property

#End Region

#Region "Fecondation"

        Public ReadOnly Property Fecondable() As Boolean
            Get
                Return Fecondation Is Nothing AndAlso Not IsCastrate AndAlso Love >= 7500 AndAlso Stamina >= 7500 AndAlso Reprod < 20 AndAlso Level >= 5
            End Get
        End Property

        Public ReadOnly Property PregnancyTime() As String
            Get
                Return If(IsPregnant, (Fecondation.Hours + 1).ToString(), "")
            End Get
        End Property

        Public ReadOnly Property IsPregnant() As String
            Get
                Return Fecondation IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property IsPregnancyFinish() As String
            Get
                Return IsPregnant AndAlso PregnancyTime > Type.GestationTime
            End Get
        End Property

#End Region

#Region "EnergyCost"

        Public ReadOnly Property MountEnergyCost() As Int32
            Get
                If Tired <= 170 Then
                    Return 4
                ElseIf Tired <= 180 Then
                    Return 5
                ElseIf Tired <= 200 Then
                    Return 6
                ElseIf Tired <= 210 Then
                    Return 7
                ElseIf Tired <= 220 Then
                    Return 8
                ElseIf Tired <= 230 Then
                    Return 10
                ElseIf Tired <= 240 Then
                    Return 12
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property MapEnergyCost() As Int32
            Get
                If Tired >= 220 AndAlso Tired < 230 Then
                    Return 1
                ElseIf Tired >= 230 AndAlso Tired < 240 Then
                    Return 2
                Else
                    Return 0
                End If
            End Get
        End Property

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

#Region "Network"

        Public ReadOnly Property MountInfo() As String
            Get
                Return Id & ":" & Type.Id & ":" & AncestorsString & ":" & CapacitiesAsString & ",:" & Name & ":" & Sex & ":" & String.Join(",", Experience, LowBound, UpperBound) & ":" & Level & ":" & If(IsMountable, 1, 0) & ":" & MaxPods & ":" & If(IsWild, 1, 0) & ":" & Stamina & ",10000:" & Maturity & "," & Type.MaxMaturity & ":" & Energy & "," & MaxEnergy & ":" & Serenity & ",-10000,10000:" & Love & ",10000:" & PregnancyTime & ":" & If(Fecondable, 1, 0) & ":" & EffectsAsString & ":" & Tired & ",240:" & If(IsCastrate, -1, Reprod) & ",20:"
            End Get
        End Property

        Public ReadOnly Property MountLightDisplayInfo() As String
            Get
                If HasCapacity(MountCapacity.Cameleonne) Then
                    Return Type.Id & ",-1,-1,-1"
                Else
                    Return Type.Id
                End If
            End Get
        End Property

        Public ReadOnly Property MaturityPercent() As Integer
            Get
                Return ((Maturity / CDbl(Type.MaxMaturity)) * 100)
            End Get
        End Property

        Public ReadOnly Property Size() As Integer
            Get
                Return Math.Round(45 * (1 / (MaturityPercent + 1)) + MaturityPercent)
            End Get
        End Property

        Public ReadOnly Property MountDisplayInfo() As String
            Get

                Return String.Concat(CellId, ";", Direction, ";0;", Id, ";", Name, _
                 ";-9;7002^", Size, ";", _
                OwnerName, ";", Level, ";", Type.Id)

            End Get
        End Property

        Private Sub SendReproductionCinematic()
            Paddock.Map.Send("eU;" & Id & "|4|5000")
        End Sub

        Private Sub SendEquip(ByVal client As GameClient)
            client.Send("Re+" & MountInfo)
        End Sub

        Private Sub SendUnEquip(ByVal client As GameClient)
            client.Send("Re-")
        End Sub

        Private Sub SendRide(ByVal client As GameClient)
            client.Send("Rr+")
        End Sub

        Private Sub SendUnRide(ByVal client As GameClient)
            client.Send("Rr-")
        End Sub

        Private Sub SendError(ByVal client As GameClient)
            client.Send("ReE")
        End Sub

        Private Sub SendPercent(ByVal client As GameClient)
            client.Send("Rx" & Percent)
        End Sub

        Private Sub SendName(ByVal client As GameClient)
            client.Send("Rn" & Name)
        End Sub

        Private Sub SendData(ByVal client As GameClient)
            client.Send("Rd" & MountInfo)
        End Sub

#End Region

#Region "Actions"

        Public Sub Equip(ByVal client As GameClient)
            client.Character.Mount = Me
            SendPercent(client)
            SendEquip(client)
        End Sub

        Public Sub Unequip(ByVal client As GameClient)
            client.Character.Mount = Nothing
            SendUnEquip(client)
        End Sub

        Public Sub Refresh(ByVal client As GameClient)
            SendEquip(client)
        End Sub

        Public Sub Ride(ByVal client As GameClient)
            If client.Character.State.IsMounted = True Then
                Unride(client)
            Else
                'Remove familier
                If client.Character.Items.IsObjectOnPos(ItemsHandler.Positions.FAMILIER) Then
                    Dim fam = client.Character.Items.GetObjectOnPos(ItemsHandler.Positions.FAMILIER)
                    client.Character.Items.MoveItem(client, fam.TemplateID, ItemsHandler.Positions.NONE, fam.Quantity)
                End If
                'Enough energy ?
                If Energy < MountEnergyCost Then
                    client.SendNormalMessage("Impossible votre monture n'a pas assez d'energie")
                    Return
                End If
                client.Character.State.IsMounted = True
                Energy -= MountEnergyCost
                SendRide(client)
                Refresh(client)
                client.Character.Items.SetMount(client, Me)
                client.Character.GetMap().RefreshCharacter(client)
            End If
        End Sub

        Public Sub Unride(ByVal client As GameClient)
            client.Character.State.IsMounted = False
            SendUnRide(client)
            client.Character.Items.SetMount(client, Nothing)
            client.Character.GetMap().RefreshCharacter(client)
        End Sub

        Public Sub SetPercent(ByVal client As GameClient, ByVal percent As Integer)
            If (percent < 100 And percent >= 0) Then
                Me.Percent = percent
                SendPercent(client)
            Else
                client.SendNormalMessage("Impossible de fixer le %age en dehors des bornes")
            End If
        End Sub

        Public Sub SetName(ByVal client As GameClient, ByVal name As String)
            Me.Name = name
            SendName(client)
        End Sub

        Public Sub Kill(ByVal client As GameClient)
            client.SendNormalMessage("Vous tranchez violement la tête de votre dragodinde")
            If client.Character.State.IsMounted Then
                Unride(client)
            End If
            Unequip(client)
        End Sub

        Public Sub Castre(ByVal client As GameClient)
            Me.IsCastrate = True
            SendEquip(client)
            client.SendNormalMessage("Un bon coup de machette")
        End Sub

        Public Sub Accouple(ByVal male As Mount)
            If Sex = 1 Then

                Me.SendReproductionCinematic()
                male.SendReproductionCinematic()

                Fecondation = New MountFecondation(male, Me)

                male.Reprod += 1
                Me.Reprod += 1

                If male.IsWild Then
                    male = Nothing
                End If

                male.Love -= 7500
                Me.Love -= 7500
                male.Stamina -= 7500
                Me.Stamina -= 7500
            End If
        End Sub

        Public Function GetBabies(ByVal client As GameClient) As List(Of Mount)
            If IsPregnancyFinish Then
                Dim children As New List(Of Mount)
                Dim nb As Integer = Rnd() * 16

                If nb <= 1 Then
                    children.Add(Fecondation.GenerateChild())
                    children.Add(Fecondation.GenerateChild())
                    children.Add(Fecondation.GenerateChild())
                    client.SendNormalMessage("3 beaux bébés dragodindes")
                ElseIf nb <= 5 Then
                    children.Add(Fecondation.GenerateChild())
                    children.Add(Fecondation.GenerateChild())
                    client.SendNormalMessage("2 beaux bébés dragodindes")
                ElseIf nb <= 16 Then
                    children.Add(Fecondation.GenerateChild())
                    client.SendNormalMessage("1 beau bébé dragodinde")
                End If
                Fecondation = Nothing

                For Each child As Mount In children
                    child.OwnerName = OwnerName
                Next

                Return children
            End If
            Return Nothing
        End Function

        Public Sub GetInfoInterface(ByVal client As GameClient)
            SendData(client)
        End Sub

        Public Sub Move()



        End Sub

#End Region

#Region "Save"

        Public ReadOnly Property SaveString() As String
            Get
                Return String.Join(":", _
                OwnerName, _
                Type.Id, _
                IsWild, _
                IsCastrate, _
                AncestorsSaveString, _
                Capacity, _
                Name, _
                Experience, _
                Sex, _
                Stamina, _
                Maturity, _
                Energy, _
                Serenity, _
                Love, _
                If(Fecondation IsNot Nothing, Fecondation.SaveString, ""), _
                Tired, _
                Reprod, _
                Percent)
            End Get
        End Property

#End Region

    End Class
End Namespace