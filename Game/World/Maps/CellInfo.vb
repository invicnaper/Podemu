Namespace World.Maps

    Public Class CellInfo

        Public LoS As Boolean
        Public Movement As MovementEnum
        Public GroundLevel As Integer
        Public GroundSlope As Integer

        Sub New(ByVal Los As Boolean, ByVal Movement As MovementEnum, ByVal GL As Integer, ByVal GS As Integer)
            Me.LoS = Los
            Me.Movement = Movement
            Me.GroundLevel = GL
            Me.GroundSlope = GS
        End Sub


        Public Function IsReachable() As Boolean
            Return Movement > 1
        End Function


        Public ReadOnly Property CellHeight As Double
            Get
                Dim a As Double = If(GroundSlope = 0 OrElse GroundSlope = 1, 0, 0.5)
                Dim b As Double = If(GroundLevel = 0, 0, GroundLevel - 7)
                Return a + b
            End Get
        End Property


    End Class
End Namespace