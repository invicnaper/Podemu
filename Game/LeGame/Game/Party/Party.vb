Namespace Game
    Public Class Party

        Public CharacterList As New List(Of GameClient)
        Public Owner As Integer = -1
        Public OwnerName As String = ""

        Public Sub New(ByVal Player1 As GameClient, ByVal Player2 As GameClient)

            Player1.Character.State.InParty = True
            Player1.Character.State.GetParty = Me
            Player2.Character.State.InParty = True
            Player2.Character.State.GetParty = Me

            Owner = Player1.Character.ID
            OwnerName = Player1.Character.Name
            CharacterList.Add(Player1)
            CharacterList.Add(Player2)

            Send("PCK" & OwnerName)
            Send("PL" & Owner)
            Send("PM" & PartyPattern)

        End Sub

        Private ReadOnly Property GetCharacters() As List(Of GameClient)
            Get
                Dim NewList As New List(Of Game.GameClient)
                NewList.AddRange(CharacterList)
                Return NewList
            End Get
        End Property

        Public Function GetCharacter(ByVal Id As Integer) As GameClient
            For Each Player As GameClient In GetCharacters
                If Player.Character.ID = Id Then
                    Return Player
                End If
            Next
            Return Nothing
        End Function

        Private ReadOnly Property PartyPattern() As String
            Get
                Return "+" & String.Join("|", (From Player In GetCharacters Select Player.Character.PatternOnParty).ToArray())
            End Get
        End Property

        Public Sub Send(ByVal Packet As String)
            For Each Player As GameClient In GetCharacters()
                Player.Send(Packet)
            Next
        End Sub

        Public Sub AddCharacter(ByVal Client As GameClient)

            Client.Character.State.InParty = True
            Client.Character.State.GetParty = Me

            Client.Send("PCK" & OwnerName)
            Client.Send("PL" & Owner)
            Client.Send("PM" & PartyPattern)
            CharacterList.Add(Client)
            Send("PM+" & Client.Character.PatternOnParty)

        End Sub

        Public Sub ChangeOwner(ByVal Id As Integer, ByVal Name As String)
            Owner = Id
            OwnerName = Name
            Send("PL" & Id)
        End Sub

        Public Sub DelCharacter(ByVal Client As GameClient, Optional ByVal KickId As String = "")
            If CharacterList.Contains(Client) Then

                Client.Character.State.InParty = False
                Client.Character.State.GetParty = Nothing

                CharacterList.Remove(Client)
                Send("PM-" & Client.Character.ID)

                Client.Send("PV" & KickId)

                If CharacterList.Count = 1 Then
                    Delete()
                ElseIf Client.Character.ID = Owner Then
                    ChangeOwner(CharacterList(0).Character.ID, CharacterList(0).Character.Name)
                End If

            End If
        End Sub

        Public Sub Delete()
            For Each Player As GameClient In GetCharacters()
                Player.Character.State.InParty = False
                Player.Character.State.GetParty = Nothing
            Next
            Send("PV")
            Owner = -1
            CharacterList.Clear()
        End Sub

    End Class
End Namespace