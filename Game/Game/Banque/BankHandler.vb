Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.World
Imports Podemu.Game
Imports System.Text

Public Class BankHandler

    Public Shared Sub LoadBank(ByVal Client As GameClient)

        Try

            SyncLock Sql.AccountsSync
                Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

                Using Result = SQLCommand.ExecuteReader

                    If Result.Read Then

                        Dim itemsBank As String = Result("bank")

                        BankParser.ListOfItems.Clear()

                        If itemsBank <> "" Then
                            Dim ItemsSplit() As String = itemsBank.Split(";")
                            For i As Integer = 0 To ItemsSplit.Length - 1
                                Dim ItemSplit() As String = ItemsSplit(i).Split("~")
                                LoadItem(ItemSplit(0), ItemSplit(1), ItemSplit(2), ItemSplit(3), ItemSplit(4))
                            Next
                        End If
                    End If

                End Using

            End SyncLock

        Catch ex As Exception
        End Try

    End Sub

    Public Shared Function LoadBankKamas(ByVal Client As GameClient) As String

        Dim Data1 As String = ""

        SyncLock Sql.AccountsSync
            Dim SQLText As String = "SELECT * FROM player_accounts WHERE username='" & Client.AccountName & "'"
            Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)

            Dim Result = SQLCommand.ExecuteReader
            If Result.Read Then
                Data1 = Result("bankKamas")
            Else
                Data1 = ""
            End If

            If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

        End SyncLock
        Return Data1
    End Function

    Private Shared ReadOnly Property GetItemsSave(ByVal ListOfItems As List(Of Item)) As String
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

    Public Shared Sub AddBanqueKamas(ByVal KamasNumber As Integer, ByVal Client As GameClient)

        Dim SQLText As String = "UPDATE player_accounts SET bankKamas = bankKamas + " & KamasNumber & " WHERE username = '" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub SuprBanqueKamas(ByVal KamasNumber As Integer, ByVal Client As GameClient)

        Dim SQLText As String = "UPDATE player_accounts SET bankKamas = bankKamas " & KamasNumber & " WHERE username = '" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Public Shared Sub SendListOfItems(ByVal Client As GameClient, ByVal MyList As List(Of Item))
        Dim SQLText As String = "UPDATE player_accounts SET bank = '" & GetItemsSave(BankParser.ListOfItems) & "' WHERE username = '" & Client.AccountName & "'"
        Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
        SQLCommand.ExecuteNonQuery()
    End Sub

    Private Shared Sub LoadItem(ByVal TemplateID As String, ByVal Quantity As String, ByVal Position As String, ByVal Effects As String, ByVal Args As String)

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

        BankParser.ListOfItems.Add(NewItem)

    End Sub

End Class
