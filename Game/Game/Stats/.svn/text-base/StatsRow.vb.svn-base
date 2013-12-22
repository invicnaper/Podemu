Namespace Game
    Public Class StatsRow

        Public Base As Integer = 0
        Public Items As Integer = 0
        Public Dons As Integer = 0
        Public Boosts As Integer = 0

        Public ReadOnly Property Total() As Integer
            Get
                Return If(BaseTotal > 0, BaseTotal, 0)
            End Get
        End Property

        Public ReadOnly Property TotalWithoutBoosts() As Integer
            Get
                Return Base + Items + Dons
            End Get
        End Property

        Public ReadOnly Property BaseTotal() As Integer
            Get
                Return Base + Items + Dons + Boosts
            End Get
        End Property

        Public ReadOnly Property BridTotal() As Integer
            Get
                Return If(BaseTotal > 50, 50, BaseTotal)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Base & "," & Items & "," & Dons & "," & Boosts
        End Function

        Public Sub New()
        End Sub

        Public Sub New(ByVal Value As Integer)
            Base = Value
        End Sub

    End Class
End Namespace