﻿Imports Podemu.Utils

Namespace Game
    Public Class NpcTemplate

        Public TemplateID As Integer
        Public Name As String
        Public Size, Skin, Sexe As Integer
        Public Color(3) As Integer
        Public ItemsArt As String
        Public ArtWork As Integer
        Public SellingList As New List(Of Integer)

        Public CachedSellingString As New CachedPattern(AddressOf GetSellingString)

        Public Function GetSellingString() As String
            Dim SellList As String = ""
            For Each ItemID As Integer In SellingList
                If ItemsHandler.ItemExist(ItemID) Then
                    SellList &= ItemsHandler.GetItemTemplate(ItemID).ToString & "|"
                End If
            Next
            Return SellList

        End Function

    End Class
End Namespace