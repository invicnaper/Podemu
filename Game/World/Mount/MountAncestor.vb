﻿Imports Podemu.Utils
Imports Podemu.Game

Namespace World
    Public Class MountAncestor

        Public TypeId As Integer
        Public Capacity As Int32

#Region "Ctor"

        Public Sub New(ByVal data As String)

            Dim sData = data.Split("*")
            TypeId = sData(0)
            Capacity = sData(1)

        End Sub

        Public Sub New(ByVal TypeId As Integer, ByVal Capacity As Integer)

            Me.TypeId = TypeId
            Me.Capacity = Capacity

        End Sub

#End Region

#Region "Capacities"

        Public Function HasCapacity(ByVal capacity As MountCapacity) As Boolean
            Return (Me.Capacity & capacity) = capacity
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

#Region "Save"

        Public ReadOnly Property SaveString() As String
            Get
                Return String.Concat(TypeId, "*", Capacity)
            End Get
        End Property

#End Region

    End Class
End Namespace