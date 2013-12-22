Imports MySql.Data.MySqlClient
Imports Vemu_rs.Utils

Namespace Realm
    Public Class LoginInfos

        Public Sub New(ByVal AccountName As String)
            ReloadAll(AccountName)
        End Sub

        Public Sub ReloadAll(ByVal AccountName As String)

            SyncLock Sql.AccountsSync

                Dim Result As MySqlDataReader = Nothing

                Try

                    Dim SQLText As String = "SELECT * FROM player_accounts WHERE username=@AccountName"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                    SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", AccountName))

                    Result = SQLCommand.ExecuteReader

                    If Result.Read Then

                        Me.AccountExist = True
                        Me.Id = Result("id")
                        Me.RealPassword = Result("password")
                        Me.GmLevel = Result("gmlevel")
                        Dim Characters As String = Result("characters").ToString
                        If Characters <> "" Then Me.CharactersList = ParseCharacters(Characters)
                        Me.BaseCharString = Characters
                        Me.Banned = (Result("banned") <> 0)
                        Me.Points = Result("points")
                        Me.SubscriptionDate = Result.GetDateTime("subscriptionDate")
                        Me.Gifts = Result.GetString("gifts")
                        Me.OriginalPoints = Points

                    End If

                Catch ex As Exception
                    Utils.MyConsole.Err(ex.ToString)
                End Try

                If Not Result Is Nothing AndAlso Not Result.IsClosed Then Result.Close()

            End SyncLock

        End Sub

        Private Function ParseCharacters(ByVal Characters As String) As Dictionary(Of Integer, List(Of String))

            Dim Dico As New Dictionary(Of Integer, List(Of String))

            Dim AllData() As String = Characters.Split("|")
            For Each Data As String In AllData
                If Data.Contains(",") Then
                    Dim CharData() As String = Data.Split(",")
                    If Not Dico.ContainsKey(CharData(1)) Then Dico.Add(CharData(1), New List(Of String))
                    Dico(CharData(1)).Add(CharData(0))
                Else
                    If Not Dico.ContainsKey(1) Then Dico.Add(1, New List(Of String))
                    Dico(1).Add(Data)
                End If
            Next

            Return Dico

        End Function

        Public Id As Integer = -1
        Public AccountExist As Boolean = False
        Public RealPassword As String = ""
        Public Banned As Boolean = False

        Public CharactersList As New Dictionary(Of Integer, List(Of String))
        Public BaseCharString As String = ""

        Public GmLevel As Integer = 0
        Public Points As Integer = 0
        Public OriginalPoints As Integer = 0

        Public Gifts As String
        Public SubscriptionDate As DateTime


        Public ReadOnly Property SubscriptionTime As Long
            Get
                Return SubscriptionDate.Subtract(Now).TotalMilliseconds
            End Get
        End Property


        Public Shared Sub UpdateGifts(ByVal AccountName As String, ByVal Gift As Integer)

            SyncLock Sql.AccountsSync

                Try
                    Dim account As LoginInfos = New LoginInfos(AccountName)
                    Dim giftList As List(Of Integer) = account.Gifts.Split(";").Select(Function(s) CInt(s)).ToList()
                    giftList.Remove(giftList.FirstOrDefault(Function(g) g = Gift))
                    Dim SQLText As String = "UPDATE player_accounts SET gifts=@gifts WHERE username=@AccountName"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                    SQLCommand.Parameters.Add(New MySqlParameter("@AccountName", AccountName))

                    SQLCommand.Parameters.Add(New MySqlParameter("@gifts", String.Join(";", giftList)))

                    SQLCommand.ExecuteScalar()

                Catch ex As Exception
                    Utils.MyConsole.Err(ex.ToString)
                End Try

            End SyncLock

        End Sub
    End Class
End Namespace