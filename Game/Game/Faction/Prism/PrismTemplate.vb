Namespace Game
    Public Class PrismTemplate

        Public MapID As Integer
        Public CellID As Integer
        Public DestinationIds As New List(Of Integer)
        Public HasMonster As Boolean
        Public Faction As Integer
        Public Zone As Integer

        'Public Sub New(ByVal MapId As Integer, ByVal CellId As Integer, ByVal DestinationsIds As List(Of Integer), ByVal HasMonster As Boolean, ByVal Faction As Integer, ByVal Zone As Integer)

        '    Me.MapID = MapId
        '    Me.CellID = CellId
        '    Me.DestinationIds = DestinationsIds
        '    Me.HasMonster = HasMonster
        '    Me.Faction = Faction
        '    Me.Zone = Zone

        'End Sub

        Public ReadOnly Property GetMap As World.Map
            Get
                Return World.MapsHandler.GetMap(MapID)
            End Get
        End Property

        Public ReadOnly Property PatternPrismView(ByVal MyPrism As PrismTemplate) As String
            Get

                Return String.Concat(
                    "GM|+", MyPrism.CellID, ";1;0;-13;", _
                 IIf(MyPrism.Faction = 1, 1111, 1112), ";-10;", IIf(MyPrism.Faction = 1, 8101, 8100), "^", _
                 Utils.Config.GetItem("PRISM_SIZE"), ";3;3;", MyPrism.Faction)

            End Get
        End Property

    End Class
End Namespace
