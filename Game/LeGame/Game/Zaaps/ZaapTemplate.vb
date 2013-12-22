Namespace Game
    Public Class ZaapTemplate

        Public Id As Integer
        Public MapID As Integer
        Public CellID As Integer
        Public DestinationIds As New List(Of Integer)
        Public HasMonster As Boolean

        Public ReadOnly Property GetMap As World.Map
            Get
                Return World.MapsHandler.GetMap(MapID)
            End Get
        End Property

    End Class
End Namespace
