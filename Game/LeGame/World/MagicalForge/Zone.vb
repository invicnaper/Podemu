Imports Podemu.Game
Imports Podemu.World

Namespace Utils
    Public Class Zone

        Private Shared Function GetAdjacentCells(ByVal Map As Map, ByVal Cell As Integer) As List(Of Integer)
            Dim CellList As New List(Of Integer)
            For i As Integer = 1 To 7 Step 2
                CellList.Add(Cells.NextCell(Map, Cell, i))
            Next
            Return CellList
        End Function

        Private Shared Function GetLineCells(ByVal Fight As Fight, ByVal Cell As Integer, ByVal Direction As Integer, ByVal Lenght As Integer) As List(Of Fighter)

            Dim PlayerList As New List(Of Fighter)

            For i As Integer = 0 To If(Lenght > Fight.Map.MaxCell, Fight.Map.MaxCell, Lenght)
                Dim Fighter As Fighter = Fight.GetFighterFromCell(Cell)
                If Not Fighter Is Nothing Then PlayerList.Add(Fighter)
                Cell = Cells.NextCell(Fight.Map, Cell, Direction)
            Next

            Return PlayerList

        End Function

        Private Shared Function GetCircleCells(ByVal Map As Map, ByVal Cell As Integer, ByVal Radius As Integer) As List(Of Integer)

            Dim CellList As New List(Of Integer)

            CellList.Add(Cell)

            For i As Integer = 0 To Radius
                Dim CopyList() As Integer = CellList.ToArray
                For Each CellCopy As Integer In CopyList
                    CellList.AddRange(From Item As Integer In GetAdjacentCells(Map, CellCopy) Select Item Where Not CellList.Contains(Item))
                Next
            Next

            Return CellList

        End Function

        Private Shared Function GetCircleCells(ByVal Fight As Fight, ByVal Cell As Integer, ByVal Radius As Integer) As List(Of Fighter)

            If Radius >= Fight.Map.MaxCell Then Return (From Item In Fight.Fighters Where Not Item.Dead Select Item).ToList

            Dim PlayerList As New List(Of Fighter)

            For Each iCell As Integer In GetCircleCells(Fight.Map, Cell, Radius)
                Dim Fighter As Fighter = Fight.GetFighterFromCell(iCell)
                If Not Fighter Is Nothing Then PlayerList.Add(Fighter)
            Next

            Return PlayerList

        End Function

        Private Shared Function GetCrossCells(ByVal Fight As Fight, ByVal Cell As Integer, ByVal Radius As Integer) As List(Of Fighter)

            If Radius >= Fight.Map.MaxCell Then Return (From Item In Fight.Fighters Where Not Item.Dead Select Item).ToList

            Dim PlayerList As New List(Of Fighter)

            For Each iCell As Integer In GetCircleCells(Fight.Map, Cell, Radius)
                If Cells.InLine(Fight.Map, Cell, iCell) Then
                    Dim Fighter As Fighter = Fight.GetFighterFromCell(iCell)
                    If Not Fighter Is Nothing Then PlayerList.Add(Fighter)
                End If
            Next

            Return PlayerList

        End Function

        Private Shared Function GetSingleCell(ByVal Fight As Fight, ByVal Cell As Integer) As List(Of Fighter)

            Dim PlayerList As New List(Of Fighter)
            Dim Fighter As Fighter = Fight.GetFighterFromCell(Cell)
            If Not Fighter Is Nothing Then PlayerList.Add(Fighter)
            Return PlayerList

        End Function

        Private Shared Function GetTLineCells(ByVal Fight As Fight, ByVal Cell As Integer, ByVal Direction As Integer, ByVal Lenght As Integer) As List(Of Fighter)

            Dim LineDirection As Integer = If(Direction <= 5, Direction + 2, Direction - 6)

            Dim PlayerList As New List(Of Fighter)
            PlayerList.AddRange(GetLineCells(Fight, Cell, LineDirection, Lenght))
            PlayerList.AddRange(GetLineCells(Fight, Cell, Cells.GetOppositeDirection(LineDirection), Lenght))

            Return PlayerList

        End Function

        Public Shared Function GetCells(ByVal Fight As Fight, ByVal Cell As Integer, ByVal CurrentCell As Integer, ByVal Zone As String) As List(Of Fighter)

            Select Case Zone(0)

                Case "C"
                    Return GetCircleCells(Fight, Cell, Cells.GetDirNum(Zone(1)) - 1)

                Case "X"
                    Return GetCrossCells(Fight, Cell, Cells.GetDirNum(Zone(1)) - 1)

                Case "T"
                    Return GetTLineCells(Fight, Cell, Cells.GetDirection(Fight.Map, CurrentCell, Cell), Cells.GetDirNum(Zone(1)))

                Case "L"
                    Return GetLineCells(Fight, Cell, Cells.GetDirection(Fight.Map, CurrentCell, Cell), Cells.GetDirNum(Zone(1)))

                Case "P"
                    Return GetSingleCell(Fight, Cell)

                Case Else
                    Return GetSingleCell(Fight, Cell)

            End Select

        End Function

    End Class
End Namespace
