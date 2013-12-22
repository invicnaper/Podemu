﻿Imports Podemu.Utils
Imports Podemu.Game

Namespace World
    Public Class MountFecondation

        Public FecondationDate As DateTime
        Public Ancestors(14) As MountAncestor

#Region "Ctor"

        Public Sub New(ByVal data As String)
            Dim sData = data.Split(",")

            FecondationDate = DateTime.Parse(sData(0))

            Ancestors = (sData(1).Split("|").Select(Function(a) New MountAncestor(a))).ToArray()

        End Sub

        Public Sub New(ByVal Male As Mount, ByVal Female As Mount)

            Dim aMale = Male.Ancestors
            Dim aFemale = Female.Ancestors

            Ancestors(0) = New MountAncestor(Male.Type.Id, Male.Capacity)
            Ancestors(1) = New MountAncestor(Female.Type.Id, Female.Capacity)

            'Take the first 6 ancestors of each
            Dim count As Integer = 2
            For i As Integer = 0 To 1
                For j As Integer = 0 To (2 * i) + 1
                    Ancestors(count) = aMale(j + 2 * i)
                    count += 1
                Next
                For j As Integer = 0 To (2 * i) + 1
                    Ancestors(count) = aFemale(j + 2 + 2 * i)
                    count += 1
                Next
            Next

            FecondationDate = Date.Now
        End Sub

#End Region

#Region "Capacities"

        Public Function ToFlag(ByVal capacities As List(Of Integer)) As Integer
            Dim result As Integer = 0
            For Each capa As Integer In capacities
                result = result Or capa
            Next
            Return result
        End Function

#End Region

#Region "Save"

        Public ReadOnly Property SaveString() As String
            Get
                Return String.Join(",", _
                FecondationDate.ToString(), _
                String.Join("|", Ancestors.SelectMany(Function(a) a.SaveString)))
            End Get
        End Property

#End Region

#Region "Time"

        Public ReadOnly Property Hours() As Integer
            Get
                Return DateTime.Now.Subtract(FecondationDate).TotalHours
            End Get
        End Property

#End Region

#Region "Child"

        Public Function GenerateChild() As Mount
            Dim types As New List(Of Integer)
            Dim capacities As New List(Of Integer)

            'For i As Integer = 0 To Ancestors.Count
            '    'Type
            '    For j As Integer = 0 To Rnd() * 3 * i
            '        types.Add(Ancestors(i).TypeId)
            '    Next

            '    'Capacity
            '    For j As Integer = 0 To Rnd() * 3 * i
            '        capacities.AddRange(Ancestors(i).Capacities)
            '    Next
            'Next

            Dim childType As Integer = 36
            Dim childSex = Rnd()
            Dim childCapacities As New List(Of Integer)

            'For i = 0 To Rnd() * 3
            '    childCapacities.Add(capacities(Rnd() * capacities.Count))
            'Next

            Dim childStamina As Integer = Rnd() * 10000
            Dim childLove As Integer = Rnd() * 10000
            Dim childSerenity As Integer = Rnd() * 10000 * (Rnd() - 1)

            Return New Mount("", childType, False, ToFlag(childCapacities), childSex, childStamina, childSerenity, childLove, Ancestors)

        End Function

#End Region

    End Class
End Namespace