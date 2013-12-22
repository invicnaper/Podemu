Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class Coffre

        Public Shared Function LoadItemsInBank() As String

            Dim List As String = ""
            Dim SQLTexte As String = "SELECT * FROM player_characters WHERE items"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                Dim ItemID As String = Hex(Result("items"))
                Dim Quantite As String = Hex(Result("quantite"))
                If List = "" Then
                    List = "O" & Hex(Result("guid")) & "~" & ItemID & "~" & Quantite & "~" & Result("position") & "~" & Result("stats") & ";"
                Else
                    List &= ";O" & Hex(Result("guid")) & "~" & ItemID & "~" & Quantite & "~" & Result("position") & "~" & Result("stats")
                End If
            End While

            Result.Close()

            'TheClient.Send("EsKO+" & ItemID & "|" & Quantite & "|" & ItemID)
            'List &= "O" & Item.ItemId & "~" & Item.ItemId & "~" & Item.Nombre & "~" & Item.Guid & "~" 'nID, nUnicID, nQuantity, nPosition, sEffects
            'List &= ";;O" & Item.ItemId & "~" & Item.ItemId & "~" & Item.Nombre & "~" & Item.Guid & "~"

            Return List
        End Function

        Public Sub Banque(ByVal TheClient As GameClient, ByVal Name As String)
            Dim SQLText As String = "UPDATE player_accounts SET banque_items = '" & GetListAccount(TheClient) & "' WHERE username = '" & Name & "'"
            Dim SQLCommand As New MySqlCommand(SQLText)
            SQLCommand.ExecuteNonQuery()
        End Sub

        Public Sub BanqueItems(ByVal TheClient As GameClient, ByVal Name As String)
            Dim SQLText As String = "UPDATE player_accounts SET banque_items = '' WHERE username = '" & Name & "'"
            Dim SQLCommand As New MySqlCommand(SQLText)
            SQLCommand.ExecuteNonQuery()
            SQLText = "UPDATE player_accounts SET banque_items = '" & GetListAccount(TheClient) & "' WHERE username = '" & Name & "'"
            SQLCommand = New MySqlCommand(SQLText)
            SQLCommand.ExecuteNonQuery()
        End Sub

        Public Sub AddItem(ByVal TheClient As GameClient, ByVal ItemID As Integer, ByVal Quantité As Integer)
            Dim Guid As Integer = 0
            Dim Quantité2 As Integer = 0
            Dim Stats As String = ""
            Dim SQLTexte As String = "INSERT INTO items WHERE itemid = '" & ItemID & "' AND perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                Guid = Result("guid")
                Quantité2 = Result("quantite")
                Stats = Result("stats")
            End While

            Result.Close()
            Quantité += Quantité2
            AddItemInBanque(TheClient, Guid, ItemID, Quantité, Stats)
        End Sub

        Public Sub DelItem(ByVal TheClient As GameClient, ByVal ItemID As Integer)
            Dim SQLTexte As String = "DELETE FROM items WHERE itemid = '" & ItemID & "' AND perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            SQLCommand.ExecuteNonQuery()
        End Sub

        Public Sub UpItem(ByVal TheClient As GameClient, ByVal ItemID As Integer, ByVal Quantity As Integer)
            Dim SQLTexte As String = "UPDATE items SET quantite = '" & Quantity & "' WHERE itemid = '" & ItemID & "' AND perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            SQLCommand.ExecuteNonQuery()
        End Sub

        Public Function GetListAccount(ByVal TheClient As GameClient) As String
            Dim List As String = ""
            Dim SQLTexte As String = "SELECT * FROM items WHERE perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                If List = "" Then
                    List &= Result("guid")
                Else
                    List &= ";" & Result("guid")
                End If
            End While

            Result.Close()
            Return List
        End Function

        Public Function GetQuantité(ByVal TheClient As GameClient, ByVal ItemID As Integer) As Integer
            Dim Quantité As Integer = 0
            Dim SQLTexte As String = "SELECT * FROM items WHERE perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                If Result("itemid") = ItemID Then
                    Quantité = Result("quantite")
                End If
            End While

            Result.Close()
            Return Quantité
        End Function

        Public Function GetGuid(ByVal TheClient As GameClient, ByVal ItemID As Integer) As Integer
            Dim SQLTexte As String = "SELECT * FROM items WHERE perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                If Result("itemid") = ItemID Then
                    Dim Guid As Integer = Result("guid")
                    Result.Close()
                    Return Guid
                End If
            End While

            Result.Close()
            Return Nothing
        End Function

        Public Function GetItemID(ByVal TheClient As GameClient, ByVal Guid As Integer) As Integer
            Dim SQLTexte As String = "SELECT * FROM items WHERE perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                If Result("guid") = Guid Then
                    Dim ItemID As Integer = Result("itemid")
                    Result.Close()
                    Return ItemID
                    Exit Function
                End If
            End While

            Result.Close()
            Return Nothing
        End Function

        Public Function GetExist(ByVal TheClient As GameClient, ByVal ItemID As Integer) As Integer
            Dim SQLTexte As String = "SELECT * FROM items WHERE perso = '" & TheClient.Character.ID & "'"
            Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
            Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

            While Result.Read
                If Result("itemid") = ItemID Then
                    Result.Close()
                    Return True
                End If
            End While

            Result.Close()
            Return False
        End Function

        Public Sub onItems(ByVal TheClient As GameClient, ByVal ExtraData As String)

                    Select Mid(ExtraData, 1, 1)
                Case "+" 'On met dans la banque
                    Dim Infos() As String = ExtraData.Split("|")
                    Dim UniqueID As Integer = CInt(Mid(Infos(0), 2))
                    Dim ItemID As Integer = TheClient.Character.Items.GetItemIdByUniqueId(UniqueID)
                    Dim ItemIDh As String = Hex(ItemID)
                    Dim Guid As Integer = GetGuid(TheClient, ItemID)
                    Dim Quantité As Integer = Infos(1)
                    TheClient.Send("EsKO+" & Hex(Guid) & "|" & GetQuantité(TheClient, ItemID) + Quantité & "|" & ItemID & "|" & IIf(GetExist(TheClient, ItemID), "1", "-1")) '10457")
                    AddItem(TheClient, ItemID, Quantité)
                    TheClient.Character.Items.DeleteItem(TheClient, UniqueID, Quantité)
                    BanqueItems(TheClient, TheClient.AccountName)

                Case "-" 'On met dans l'inventaire
                    Dim Data() As String = ExtraData.Split("|")
                    Dim Guid As Integer = CInt(Mid(Data(0), 2))
                    Dim Quantité As Integer = CInt(Data(1))
                    Dim ItemID As Integer = GetItemID(TheClient, Guid)
                    Dim TempItem As Item = ItemsHandler.GetItemTemplate(ItemID).GenerateItemFromBank(Quantité)
                    TheClient.Character.Items.AddItem(TheClient, TempItem)
                    Dim NewQuantity As Integer = GetQuantité(TheClient, ItemID) - Quantité
                    If NewQuantity = 0 Then
                        TheClient.Send("EsKO-" & Hex(Guid))
                        DelItem(TheClient, ItemID)
                    Else
                        TheClient.Send("EsKO+" & Hex(Guid) & "|" & GetQuantité(TheClient, ItemID) - Quantité & "|" & ItemID & "|" & IIf(GetExist(TheClient, ItemID), "1", "-1")) '10457")
                        UpItem(TheClient, ItemID, GetQuantité(TheClient, ItemID) - Quantité)
                    End If
                    BanqueItems(TheClient, TheClient.AccountName)
            End Select
        End Sub

        Public Sub AddItemInBanque(ByVal TheClient As GameClient, ByVal Guid As Integer, ByVal ItemID As Integer, ByVal Quantite As Integer, ByVal Stats As String)
            If GetExist(TheClient, ItemID) Then 'Si il existe déjà
                Dim SQLTexte As String = "UPDATE items SET quantite = '" & Quantite & "' WHERE guid = '" & Guid & "'"
                Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
                SQLCommand.ExecuteNonQuery()
            Else 'Sinon
                Dim SQLTexte As String = "INSERT INTO items VALUES('', '" & ItemID & "', '" & Quantite & "', '-1', '" & Stats & "', '" & TheClient.Character.ID & "')"
                Dim SQLCommand As New MySqlCommand(SQLTexte, Sql.Accounts)
                SQLCommand.ExecuteNonQuery()
            End If
        End Sub

    End Class
End Namespace
