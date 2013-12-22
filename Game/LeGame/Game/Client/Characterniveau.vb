Namespace Game
    Public Class CharacterLevelUp

        Public Id As Integer = 0
        Private m_Exp As Long = 0

        Public Enabled As Boolean = True
        Public TrackingClient As String = ""

        Public ReadOnly Property Rank As Integer
            Get
                Dim mRank As Integer = 1
                For i As Integer = 1 To ExperienceTable.MaxLevel
                    If ExperienceTable.AtLevel(i).Character = -1 Then Exit For
                    If Exp >= ExperienceTable.AtLevel(i).Character Then
                        mRank = i
                    End If
                Next
                Return mRank
            End Get
        End Property

        Public ReadOnly Property ShowedId() As Integer
            Get
                If Enabled Then
                    Return Id
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Property Exp As Long
            Get
                Return m_Exp
            End Get
            Set(ByVal value As Long)
                m_Exp = value
                If m_Exp < 0 Then m_Exp = 0
                If m_Exp > ExperienceTable.AtLevel(200).Character Then m_Exp = ExperienceTable.AtLevel(200).Character
            End Set
        End Property

        Public ReadOnly Property ExpLost As Integer
            Get
                Return Math.Ceiling(m_Exp / 100 * 5)
            End Get
        End Property

        Public Sub AddExp(ByVal Client As GameClient, ByVal Value As Integer)

            If Value > 0 Then

                Dim LastRank As Integer = Rank
                Exp += Value
                Client.Send("Im080;" & Value)
                If Rank <> LastRank Then
                    Client.Send("Im082;" & Rank)
                End If

            End If

        End Sub

        Public Sub RemExp(ByVal Client As GameClient, ByVal Value As Integer)

            If Value > 0 Then

                Dim LastRank As Integer = Rank
                Exp -= Value
                Client.Send("Im081;" & Value)
                If Rank <> LastRank Then
                    Client.Send("Im083;" & Rank)
                End If

            End If

        End Sub

        Public Sub Enable(ByVal Client As GameClient)

            Enabled = True
            Client.Character.SendAccountStats()

        End Sub

        Public Sub Disable(ByVal Client As GameClient)

            Enabled = False
            RemExp(Client, ExpLost)
            Client.Character.SendAccountStats()

        End Sub

        Public Sub ResetAll()

            Id = 0
            m_Exp = 0
            Enabled = True

        End Sub

        Public Overrides Function ToString() As String
            Return Id & "~2," & 0 & "," & Rank & "," & Exp & ",0," & If(Enabled, 1, 0)
        End Function


    End Class
End Namespace