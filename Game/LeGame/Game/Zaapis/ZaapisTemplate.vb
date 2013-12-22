Namespace Game
    Public Class ZaapisTemplate
        Public Shared ListOfZaapis As New Dictionary(Of Integer, ZaapisTemplate)

        Public Shared Function GetTemplate(ByVal MapId As Integer, ByVal CellId As Integer) As ZaapisTemplate
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId AndAlso z.Value.CellID = CellId).Value
        End Function

        Public Shared Function GuessTemplate(ByVal MapId As Integer) As ZaapisTemplate
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value
        End Function

        Public Shared Function HasZaapi(ByVal MapId As Integer) As Boolean
            Return ListOfZaapis.FirstOrDefault(Function(z) z.Value.MapID = MapId).Value IsNot Nothing
        End Function

        Public Shared Function GetTemplate(ByVal Id As Integer) As ZaapisTemplate
            If ListOfZaapis.ContainsKey(Id) Then Return ListOfZaapis(Id)
            Return Nothing
        End Function

        Public Shared Function GetTemplates(ByVal MapId As Integer) As IEnumerable(Of ZaapisTemplate)
            Return ListOfZaapis.Where(Function(z) z.Value.MapID = MapId).Select(Function(k) k.Value)
        End Function
        Public MapID As Integer
        Public CellID As Integer
        Public DestinationIds As New List(Of Integer)
        Public HasMonster As Boolean
        Public Zone As Integer

        Public ReadOnly Property GetMap As World.Map
            Get
                Return World.MapsHandler.GetMap(MapID)
            End Get
        End Property

    End Class
End Namespace
