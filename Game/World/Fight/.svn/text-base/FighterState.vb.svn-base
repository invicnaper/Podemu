Namespace World
    Public Class FighterState

        Private StateList As New Dictionary(Of State, Integer)

        Private ReadOnly Property GetStates() As Dictionary(Of State, Integer)
            Get
                Dim NewList As New Dictionary(Of State, Integer)
                For Each State As State In StateList.Keys
                    NewList.Add(State, StateList(State))
                Next
                Return NewList
            End Get
        End Property

        Public Enum State
            Saoul = 1
            Porteur = 3
            Pesanteur = 7
            Porte = 8
            Affaibli = 42
            Altruisme = 50
            Invisible = 600
        End Enum

        Public Sub Reset()
            StateList.Clear()
        End Sub

        Public Function Has(ByVal Effect As State) As Boolean
            If StateList.ContainsKey(Effect) Then
                Return StateList(Effect) > -1
            Else
                Return False
            End If
        End Function

        Public Sub Add(ByVal Player As Fighter, ByVal Effect As State, ByVal Turns As Integer)
            If StateList.ContainsKey(Effect) Then
                StateList(Effect) = Turns - 1
            Else
                StateList.Add(Effect, Turns - 1)
            End If
            If Effect = State.Invisible Then
                Player.Fight.Send("GA;150;" & Player.Id & ";" & Player.Id & "," & Turns)
            Else
                Player.Fight.Send("GA;950;" & Player.Id & ";" & Player.Id & "," & Effect & ",1")
            End If
        End Sub

        Public Sub Refresh(ByVal Player As Fighter)
            For Each Effect As State In GetStates().Keys
                If StateList(Effect) > -1 Then
                    StateList(Effect) -= 1
                    If StateList(Effect) = -1 Then Del(Player, Effect)
                End If
            Next
        End Sub

        Public Sub Del(ByVal Player As Fighter, ByVal Effect As State)
            If StateList.ContainsKey(Effect) Then
                StateList(Effect) = -1
            End If
            If Effect = State.Invisible Then
                Player.Fight.Send("GA;150;" & Player.Id & ";" & Player.Id)
                Player.Fight.Send("GA;4;" & Player.Id & ";" & Player.Id & "," & Player.Cell)
            Else
                Player.Fight.Send("GA;950;" & Player.Id & ";" & Player.Id & "," & Effect & ",0")
            End If
        End Sub

        Public Sub LostInvisible(ByVal Player As Fighter)
            If StateList.ContainsKey(State.Invisible) Then
                Del(Player, State.Invisible)
            End If
        End Sub

    End Class
End Namespace