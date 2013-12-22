Imports Podemu.Game
Imports Podemu.Utils
Imports MySql.Data.MySqlClient
Namespace World
    Public Class DropsManager

#Region "List"
        Public Shared ListOfDrops As New Dictionary(Of Integer, List(Of Drops))
#End Region

#Region "Function and Action"

        Public Shared Function ExistDrops(ByVal MonsterID As Integer) As Boolean
            Return ListOfDrops.ContainsKey(MonsterID)
        End Function



#End Region

#Region "SQL"
        Public Shared Sub Load()
            MyConsole.StartLoading("Loading drops from database...")
            SyncLock Sql.OthersSync

                Dim SQLText As String = "SELECT * FROM drops"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewDrop As New Drops

                    NewDrop.Mob = Result("mob")
                    NewDrop.Item = Result("item")
                    NewDrop.Seuil = Result("seuil")
                    NewDrop.Max = Result("max")
                    NewDrop.Taux = Result("taux")

                    If Not ExistDrops(NewDrop.Mob) Then
                        ListOfDrops.Add(NewDrop.Mob, New List(Of Drops))
                    End If
                    ListOfDrops(NewDrop.Mob).Add(NewDrop)

                End While

                Result.Close()

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ListOfDrops.Count & "@' drops loaded from database")

        End Sub
#End Region

#Region "Drops Processor"

        Public Shared Sub Merge(ByVal Items As List(Of Item))

            Dim ToRemove As New Dictionary(Of Integer, Item)

            For Each Item In Items

                If Not ToRemove.ContainsKey(Item.UniqueID) Then
                    For Each OtherItem In Items.Where(Function(i) i.UniqueID <> Item.UniqueID)

                        If Not ToRemove.ContainsKey(Item.UniqueID) AndAlso Item.IsSuperposable(OtherItem) Then
                            Item.Quantity += OtherItem.Quantity
                            ToRemove.Add(OtherItem.UniqueID, OtherItem)
                        End If

                    Next
                End If
            Next

            For Each Item In ToRemove
                Items.Remove(Item.Value)
            Next

        End Sub

        Public Shared Function GetDrops(ByVal PP As Integer, ByVal Monster As MonsterTemplate, ByVal Rate As Double) As List(Of Item)

            Dim loots As New List(Of Item)

            If ExistDrops(Monster.Id) Then

                For Each drop As Drops In ListOfDrops(Monster.Id)

                    For i As Integer = 1 To drop.Max
                        If TryDrop(PP, drop, Rate) Then
                            Dim Item = ItemsHandler.GetItemTemplate(drop.Item).GenerateItem(1)
                            Item.UniqueID = ItemsHandler.GetUniqueID
                            loots.Add(Item)
                        End If

                    Next

                Next
            End If

            Return loots
        End Function

        Public Shared Function TryDrop(ByVal PP As Integer, ByVal MyDrops As Drops, ByVal Rate As Double) As Boolean

            If MyDrops.Seuil > PP Then
                Return False
            End If

            Dim Chance As Integer = Basic.Rand(0, 100) * Rate
            If Chance > (100 - MyDrops.Taux) Then
                Return True
            End If

            Return False
        End Function

#End Region

    End Class
End Namespace