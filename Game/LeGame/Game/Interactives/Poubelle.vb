﻿Imports Podemu.Utils

Namespace Game.Interactives
    Public Class Poubelle : Inherits InteractiveObject
        Implements IStorage

        Public Storage As New Storages.Storage()

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {153}))

        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)
            Select Case SkillId

                Case 153
                    BeginTrade(Client)

            End Select
        End Sub


        Public Sub BeginTrade(ByVal Client As GameClient) Implements IStorage.BeginTrade
            Storage.BeginTrade(Client)
        End Sub

        Public Sub EndTrade(ByVal Client As GameClient) Implements IStorage.EndTrade
            Storage.EndTrade(Client)
        End Sub

        Public Sub SetKamas(ByVal Client As GameClient, ByVal Kamas As Integer) Implements IStorage.SetKamas
            Storage.SetKamas(Client, Kamas)
        End Sub

        Public Sub AddItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer) Implements IStorage.AddItem
            Storage.AddItem(Client, ItemId, Quantity)
        End Sub

        Public Sub RemoveItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer) Implements IStorage.RemoveItem
            Storage.RemoveItem(Client, ItemId, Quantity)
        End Sub


    End Class
End Namespace