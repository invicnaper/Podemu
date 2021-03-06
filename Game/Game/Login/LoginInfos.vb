﻿Imports MySql.Data.MySqlClient
Imports Podemu.Utils

Namespace Game
    Public Class LoginInfos

        Public Sub New(ByVal Id As Integer, ByVal Characters As String, ByVal GmLevel As Integer, ByVal Points As Integer, ByVal SubscriptionDate As String, ByVal Gifts As String)

            Me.Id = Id
            ParseCharacters(Characters)
            If Not CharactersList.ContainsKey(ServerId) Then CharactersList.Add(ServerId, New List(Of String))
            Me.CharacterNumber = CharactersList(ServerId).Count
            Me.GmLevel = GmLevel
            Me.Points = Points
            Me.OriginalPoints = Points
            Me.SubscriptionDate = DateTime.Parse(SubscriptionDate)

            For Each gift As String In Gifts.Split(";")
                If gift <> "" Then
                    Me.Gifts.Add(GiftsHandler.GetTemplate(CInt(gift)))
                End If
            Next
        End Sub

        Public Sub Save()

            If Points <> OriginalPoints Then

                SyncLock Sql.AccountsSync

                    Dim SQLText As String = "UPDATE player_accounts SET points=@Points WHERE id=@Id"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Accounts)
                    Dim P As MySqlParameterCollection = SQLCommand.Parameters

                    P.Add(New MySqlParameter("@Id", Id))
                    P.Add(New MySqlParameter("@Points", Points))

                    SQLCommand.ExecuteNonQuery()

                End SyncLock

            End If

        End Sub

        Private Sub ParseCharacters(ByVal Characters As String)

            Dim Dico As New Dictionary(Of Integer, List(Of String))

            If Characters <> "" Then

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
                    CharacterTotalNumber += 1
                Next

            End If

            CharactersList = Dico

        End Sub

        Public Sub AddCharacter(ByVal CharacterName As String)
            If Not CharactersList(ServerId).Contains(CharacterName) Then
                CharactersList(ServerId).Add(CharacterName)
                CharacterNumber += 1
                CharacterTotalNumber += 1
            End If
        End Sub

        Public Sub DelCharacter(ByVal CharacterName As String)
            If CharactersList(ServerId).Contains(CharacterName) Then
                CharactersList(ServerId).Remove(CharacterName)
                CharacterNumber -= 1
                CharacterTotalNumber -= 1
            End If
        End Sub

        Public Id As Integer = -1
        Public RealPassword As String = ""

        Public CharactersList As New Dictionary(Of Integer, List(Of String))
        Public CharacterNumber As Integer = 0
        Public CharacterTotalNumber As Integer = 0

        Public GmLevel As Integer = 0
        Public Points As Integer = 0
        Public OriginalPoints As Integer = 0

        Public Gifts As New List(Of GiftTemplate)

        Public SubscriptionDate As DateTime

        Public HasSignalUnregister As Boolean

        Public ReadOnly Property SubscriptionTime As Long
            Get
                If Config.GetItem(Of Boolean)("ENABLE_SUBSCRIPTION") Then
                    Return SubscriptionDate.Subtract(Now).TotalMilliseconds
                Else
                    Return OneYear
                End If
            End Get
        End Property

        Const OneYear As Long = 31536000

        Public ReadOnly Property IsRegister As Boolean
            Get
                Return SubscriptionTime > 0
            End Get
        End Property

    End Class
End Namespace