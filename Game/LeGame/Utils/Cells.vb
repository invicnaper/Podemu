Imports System.Text
Imports System.Drawing
Imports Podemu.Game
Imports Podemu.World

Namespace Utils
    Public Class Cells

#Region " Cells "

        Private Shared hash As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_"

        Public Shared Function GetCellNum(ByVal CellChars As String) As Integer

            Dim NumChar1 As Integer = hash.IndexOf(CellChars(0)) * hash.Length
            Dim NumChar2 As Integer = hash.IndexOf(CellChars(1))

            Return NumChar1 + NumChar2

        End Function

        Public Shared Function GetCellChars(ByVal CellNum As Integer) As String

            Dim CharCode2 As Integer = (CellNum Mod hash.Length)
            Dim CharCode1 As Integer = (CellNum - CharCode2) / hash.Length

            Return hash(CharCode1) & hash(CharCode2)

        End Function

        Public Shared Function GetDirChar(ByVal DirNum As Integer) As String

            If DirNum >= hash.Length Then Return ""
            Return hash(DirNum)

        End Function

        Public Shared Function GetDirNum(ByVal DirChar As String) As Integer

            Return hash.IndexOf(DirChar)

        End Function

        Public Shared Function GetX(ByVal tMap As Map, ByVal laCase As Integer) As Integer

            Dim _loc5 As Integer = Math.Floor(laCase / (tMap.Width * 2 - 1))
            Dim _loc6 As Integer = laCase - _loc5 * (tMap.Width * 2 - 1)
            Dim _loc7 As Integer = _loc6 Mod tMap.Width
            Dim _loc8 As Integer = (laCase - (tMap.Width - 1) * (_loc5 - _loc7)) / tMap.Width
            Return (_loc8)

        End Function

        Public Shared Function GetY(ByVal tMap As Map, ByVal laCase As Integer) As Integer

            Dim _loc5 As Integer = Math.Floor(laCase / (tMap.Width * 2 - 1))
            Dim _loc6 As Integer = laCase - _loc5 * (tMap.Width * 2 - 1)
            Dim _loc7 As Integer = _loc6 Mod tMap.Width
            Dim _loc8 As Integer = _loc5 - _loc7
            Return (_loc8)

        End Function

        Public Class ZPoint
            Public X As Integer
            Public Y As Integer
            Public Z As Integer

            Sub New(Optional ByVal X As Integer = 0, Optional ByVal Y As Integer = 0, Optional ByVal Z As Integer = 0)
                Me.X = X
                Me.Y = Y
                Me.Z = Z
            End Sub
        End Class

        Public Shared Function GetPoint(ByVal tMap As Map, ByVal laCase As Integer) As ZPoint

            Dim _loc5 As Integer = Math.Floor(laCase / (tMap.Width * 2 - 1))
            Dim _loc6 As Integer = laCase - _loc5 * (tMap.Width * 2 - 1)
            Dim _loc7 As Integer = _loc6 Mod tMap.Width
            Dim x As Integer = (laCase - (tMap.Width - 1) * (_loc5 - _loc7)) / tMap.Width
            Dim y As Integer = _loc5 - _loc7

            Return New ZPoint(x, y)
        End Function

        Public Shared Function GetCellByCoord(ByVal Map As Map, ByVal x As Integer, ByVal y As Integer) As Short
            Return (x * Map.Width + y * (Map.Width - 1))
        End Function

        Public Shared Function GoalDistance(ByVal tMap As Map, ByVal pos1 As Integer, ByVal pos2 As Integer) As Integer

            Dim _loc7 As Integer = Math.Abs(GetX(tMap, pos1) - GetX(tMap, pos2))
            Dim _loc8 As Integer = Math.Abs(GetY(tMap, pos1) - GetY(tMap, pos2))

            Return _loc7 + _loc8

        End Function

        Public Shared Function GoalDistanceEstimate(ByVal tMap As Map, ByVal pos1 As Integer, ByVal pos2 As Integer) As Double

            Dim _loc7 = Math.Abs(GetX(tMap, pos1) - GetX(tMap, pos2))
            Dim _loc8 = Math.Abs(GetY(tMap, pos1) - GetY(tMap, pos2))

            Return Math.Sqrt(Math.Pow(_loc7, 2) + Math.Pow(_loc8, 2))

        End Function

        Public Shared Function InLine(ByVal tMap As Map, ByVal Cell1 As Integer, ByVal Cell2 As Integer) As Boolean

            Dim IsX As Boolean = GetX(tMap, Cell1) = GetX(tMap, Cell2)
            Dim IsY As Boolean = GetY(tMap, Cell1) = GetY(tMap, Cell2)

            Return IsX Or IsY

        End Function

        Public Shared Function GetDirection(ByVal tMap As Map, ByVal Cell As Integer, ByVal Cell2 As Integer) As Integer

            Dim MapWidth As Integer = tMap.Width
            Dim ListChange() As Integer = {1, MapWidth, MapWidth * 2 - 1, MapWidth - 1, -1, -MapWidth, -MapWidth * 2 + 1, -(MapWidth - 1)}
            Dim Result = Cell2 - Cell

            For i As Integer = 7 To 0 Step -1
                If Result = ListChange(i) Then Return i
            Next

            Dim ResultX As Integer = GetX(tMap, Cell2) - GetX(tMap, Cell)
            Dim ResultY As Integer = GetY(tMap, Cell2) - GetY(tMap, Cell)

            If ResultX = 0 Then
                If ResultY > 0 Then Return 3
                Return 7
            ElseIf ResultX > 0 Then
                Return 1
            Else
                Return 5
            End If

            Return -1

        End Function

        Public Shared Function GetOppositeDirection(ByVal Direction As Integer) As Integer
            Return If(Direction >= 4, Direction - 4, Direction + 4)
        End Function

        Public Shared Function NextCell(ByVal tMap As Map, ByVal Cell As Integer, ByVal Direction As Integer) As Integer

            Select Case Direction

                Case 0
                    Return Cell + 1
                Case 1
                    Return Cell + tMap.Width
                Case 2
                    Return Cell + (tMap.Width * 2) - 1
                Case 3
                    Return Cell + tMap.Width - 1
                Case 4
                    Return Cell - 1
                Case 5
                    Return Cell - tMap.Width
                Case 6
                    Return Cell - (tMap.Width * 2) + 1
                Case 7
                    Return Cell - tMap.Width + 1
                Case Else
                    Return -1

            End Select

        End Function

        Public Shared Function NearestCells(ByVal tMap As Map, ByVal Cell As Integer) As List(Of Integer)
            Dim cells As New List(Of Integer)(4)

            For i As Integer = 1 To 7 Step 2
                Dim nCell = NextCell(tMap, Cell, i)
                If tMap.IsCellReachable(nCell) Then
                    cells.Add(nCell)
                End If
            Next

            Return cells

        End Function

        Public Shared Function NearerCell(ByVal tMap As Map, ByVal Cell As Integer) As Integer

            While (True)
                For i As Integer = 0 To 7
                    Dim nCell = Cells.NextCell(tMap, Cell, i)
                    If tMap.IsCellReachable(nCell) Then Return nCell
                Next
                Cell = Cells.NextCell(tMap, Cell, Basic.Rand(0, 7))
            End While

        End Function

        Public Shared Function CheckView(ByVal Fight As Fight, ByVal Cell1 As Integer, ByVal Cell2 As Integer, ByVal Fighters As IEnumerable(Of Fighter)) As Boolean

            Dim _loc5 = GetPoint(Fight.Map, Cell1)
            Dim _loc6 = GetPoint(Fight.Map, Cell2)

            Dim _loc7 = Fight.Map.CellInfos(Cell1)
            Dim _loc8 = Fight.Map.CellInfos(Cell2)

            Dim _loc9 = 0
            Dim _loc10 = 0

            _loc5.Z = _loc7.CellHeight + _loc9
            _loc6.Z = _loc8.CellHeight + _loc10

            Dim _loc11 = _loc6.Z - _loc5.Z
            Dim _loc12 = Math.Max(Math.Abs(_loc5.Y - _loc6.Y), Math.Abs(_loc5.X - _loc6.X))
            Dim _loc13 = (_loc5.Y - _loc6.Y) / (_loc5.X - _loc6.X)
            Dim _loc14 = _loc5.Y - _loc13 * _loc5.X
            Dim _loc15 = If(_loc6.X - _loc5.X < 0, -1, 1)
            Dim _loc16 = If(_loc6.Y - _loc5.Y < 0, -1, 1)
            Dim _loc17 = _loc5.Y
            Dim _loc18 = _loc5.X
            Dim _loc19 = _loc6.X * _loc15
            Dim _loc20 = _loc6.Y * _loc16
            Dim _loc26 = 0
            Dim _loc27 = _loc5.X + 0.5 * _loc15

            While (_loc27 * _loc15 <= _loc19)

                Dim _loc25 = _loc13 * _loc27 + _loc14

                Dim _loc21 = 0
                Dim _loc22 = 0

                If _loc16 > 0 Then
                    _loc21 = Math.Round(_loc25)
                    _loc22 = Math.Ceiling(_loc25 - 0.5)
                Else
                    _loc21 = Math.Ceiling(_loc25 - 0.5)
                    _loc22 = Math.Round(_loc25)
                End If

                _loc26 = _loc17

                While (_loc26 * _loc16 <= _loc22 * _loc16)

                    If Not checkCellView(Fight, _loc27 - _loc15 / 2, _loc26, False, _loc5, _loc6, _loc11, _loc12, Fighters) Then
                        Return False
                    End If

                    _loc26 += _loc16

                End While

                _loc17 = _loc21
                _loc27 += _loc15

            End While

            _loc26 = _loc17

            While (_loc26 * _loc16 <= _loc6.Y * _loc16)

                If Not checkCellView(Fight, _loc27 - 0.5 * _loc15, _loc26, False, _loc5, _loc6, _loc11, _loc12, Fighters) Then
                    Return False
                End If

                _loc26 += _loc16

            End While

            If Not checkCellView(Fight, _loc27 - 0.5 * _loc15, _loc26 - _loc16, True, _loc5, _loc6, _loc11, _loc12, Fighters) Then
                Return False
            End If

            Return True

        End Function

        Public Shared Function CheckView(ByVal Fight As Fight, ByVal Cell1 As Integer, ByVal Cell2 As Integer) As Boolean

            Dim _loc5 = GetPoint(Fight.Map, Cell1)
            Dim _loc6 = GetPoint(Fight.Map, Cell2)

            Dim _loc7 = Fight.Map.CellInfos(Cell1)
            Dim _loc8 = Fight.Map.CellInfos(Cell2)

            Dim _loc9 = 0
            Dim _loc10 = 0

            _loc5.Z = _loc7.CellHeight + _loc9
            _loc6.Z = _loc8.CellHeight + _loc10

            Dim _loc11 = _loc6.Z - _loc5.Z
            Dim _loc12 = Math.Max(Math.Abs(_loc5.Y - _loc6.Y), Math.Abs(_loc5.X - _loc6.X))
            Dim _loc13 = (_loc5.Y - _loc6.Y) / (_loc5.X - _loc6.X)
            Dim _loc14 = _loc5.Y - _loc13 * _loc5.X
            Dim _loc15 = If(_loc6.X - _loc5.X < 0, -1, 1)
            Dim _loc16 = If(_loc6.Y - _loc5.Y < 0, -1, 1)
            Dim _loc17 = _loc5.Y
            Dim _loc18 = _loc5.X
            Dim _loc19 = _loc6.X * _loc15
            Dim _loc20 = _loc6.Y * _loc16
            Dim _loc26 = 0
            Dim _loc27 = _loc5.X + 0.5 * _loc15

            While (_loc27 * _loc15 <= _loc19)

                Dim _loc25 = _loc13 * _loc27 + _loc14

                Dim _loc21 = 0
                Dim _loc22 = 0

                If _loc16 > 0 Then
                    _loc21 = Math.Round(_loc25)
                    _loc22 = Math.Ceiling(_loc25 - 0.5)
                Else
                    _loc21 = Math.Ceiling(_loc25 - 0.5)
                    _loc22 = Math.Round(_loc25)
                End If

                _loc26 = _loc17

                While (_loc26 * _loc16 <= _loc22 * _loc16)

                    If Not checkCellView(Fight, _loc27 - _loc15 / 2, _loc26, False, _loc5, _loc6, _loc11, _loc12) Then
                        Return False
                    End If

                    _loc26 += _loc16

                End While

                _loc17 = _loc21
                _loc27 += _loc15

            End While

            _loc26 = _loc17

            While (_loc26 * _loc16 <= _loc6.Y * _loc16)

                If Not checkCellView(Fight, _loc27 - 0.5 * _loc15, _loc26, False, _loc5, _loc6, _loc11, _loc12) Then
                    Return False
                End If

                _loc26 += _loc16

            End While

            If Not checkCellView(Fight, _loc27 - 0.5 * _loc15, _loc26 - _loc16, True, _loc5, _loc6, _loc11, _loc12) Then
                Return False
            End If

            Return True

        End Function

        Public Shared Function checkCellView(ByVal Fight As Fight, ByVal x As Integer, ByVal y As Integer, ByVal bool As Boolean, ByVal p1 As ZPoint, ByVal p2 As ZPoint, ByVal zDiff As Integer, ByVal d As Integer) As Boolean

            Dim num = GetCellByCoord(Fight.Map, x, y)
            Dim data = Fight.Map.CellInfos(num)

            Dim a = GetCellByCoord(Fight.Map, p1.X, p1.Y)
            Dim b = GetCellByCoord(Fight.Map, p2.X, p2.Y)

            Dim c = GetX(Fight.Map, num)
            Dim dd = GetX(Fight.Map, num)

            Dim _loc12 = Math.Max(Math.Abs(p1.Y - y), Math.Abs(p1.X - x))
            Dim _loc13 = _loc12 / d * zDiff + p1.Z

            Dim height = data.CellHeight

            Dim _loc15 = If(Fight.GetFighterFromCell(num) Is Nothing OrElse (_loc12 = 0 OrElse (bool OrElse p2.X = x AndAlso p2.Y = y)), False, True)

            If data.LoS AndAlso (height <= _loc13 AndAlso Not _loc15) Then
                Return (True)
            Else

                If (bool) Then
                    Return True
                End If

                Return False
            End If

        End Function

        Public Shared Function checkCellView(ByVal Fight As Fight, ByVal x As Integer, ByVal y As Integer, ByVal bool As Boolean, ByVal p1 As ZPoint, ByVal p2 As ZPoint, ByVal zDiff As Integer, ByVal d As Integer, ByVal Fighters As IEnumerable(Of Fighter)) As Boolean

            Dim num = GetCellByCoord(Fight.Map, x, y)
            Dim data = Fight.Map.CellInfos(num)

            Dim a = GetCellByCoord(Fight.Map, p1.X, p1.Y)
            Dim b = GetCellByCoord(Fight.Map, p2.X, p2.Y)

            Dim c = GetX(Fight.Map, num)
            Dim dd = GetX(Fight.Map, num)

            Dim _loc12 = Math.Max(Math.Abs(p1.Y - y), Math.Abs(p1.X - x))
            Dim _loc13 = _loc12 / d * zDiff + p1.Z

            Dim height = data.CellHeight

            Dim _loc15 = If(Fighters.FirstOrDefault(Function(f) f.Cell = num) Is Nothing OrElse (_loc12 = 0 OrElse (bool OrElse p2.X = x AndAlso p2.Y = y)), False, True)

            If data.LoS AndAlso (height <= _loc13 AndAlso Not _loc15) Then
                Return (True)
            Else

                If (bool) Then
                    Return True
                End If

                Return False
            End If

        End Function

#End Region

#Region " Paths "

        Private Shared Function IsValidCell(ByVal Client As GameClient, ByVal Cell As Integer) As Boolean

            If Client.Character.State.InBattle Then
                If Not Client.Character.State.GetFight.GetFighterFromCell(Cell) Is Nothing Then
                    Return False
                End If
            End If

            Return Client.Character.GetMap.IsCellReachable(Cell)

        End Function

        Private Shared Function IsValidLine(ByVal Client As GameClient, ByVal LastCell As Integer, ByVal Cell As String) As Boolean

            Dim Direction As Integer = Cells.GetDirNum(Cell(0))
            Dim ToCell As Integer = Cells.GetCellNum(Cell.Substring(1))

            If Client.Character.State.InBattle Then
                If Not Cells.InLine(Client.Character.GetMap, LastCell, ToCell) Then Return False

                Dim RealDirection As Integer = Cells.GetDirection(Client.Character.GetMap, LastCell, ToCell)
                If RealDirection <> Direction Then Return False
            End If

            Dim Lenght As Integer = 0
            If Client.Character.State.InBattle Then
                Lenght = GoalDistance(Client.Character.GetMap, LastCell, ToCell)
            Else
                If Cells.InLine(Client.Character.GetMap, LastCell, ToCell) Then
                    Lenght = GoalDistanceEstimate(Client.Character.GetMap, LastCell, ToCell)
                Else
                    Lenght = (GoalDistanceEstimate(Client.Character.GetMap, LastCell, ToCell) / 1.4)
                End If
            End If

            Dim ActualCell As Integer = LastCell
            For i As Integer = 1 To Lenght

                ActualCell = Cells.NextCell(Client.Character.GetMap, ActualCell, Direction)
                If Not IsValidCell(Client, ActualCell) Then Return False

            Next

            Return True

        End Function

        Public Shared Function IsValidPath(ByVal Client As GameClient, ByVal Path As String) As Boolean

            If Path.Length = 0 Then Return False
            If (Path.Length Mod 3) <> 0 Then Return False

            Dim LastCell As Integer = Client.Character.MapCell
            If Client.Character.State.InBattle Then LastCell = Client.Character.State.GetFighter.Cell

            For i As Integer = 0 To Path.Length - 1 Step 3

                Dim ActualCell As String = Path.Substring(i, 3)
                If Not IsValidLine(Client, LastCell, ActualCell) Then Return False
                LastCell = Cells.GetCellNum(ActualCell.Substring(1))

            Next

            Return True

        End Function

        Private Shared Function RemakeLine(ByVal Client As GameClient, ByVal LastCell As Integer, ByVal Cell As String, ByVal FinalCell As Integer, ByVal IsInFight As Boolean) As String

            Dim Direction As Integer = Cells.GetDirNum(Cell(0))
            Dim ToCell As Integer = Cells.GetCellNum(Cell.Substring(1))

            Dim Lenght As Integer = 0
            If Client.Character.State.InBattle Then
                Lenght = GoalDistance(Client.Character.GetMap, LastCell, ToCell)
            Else
                If InLine(Client.Character.GetMap, LastCell, ToCell) Then
                    Lenght = GoalDistanceEstimate(Client.Character.GetMap, LastCell, ToCell)
                Else
                    Lenght = (GoalDistanceEstimate(Client.Character.GetMap, LastCell, ToCell) / 1.4)
                End If
            End If

            Dim BackCell As Integer = LastCell
            Dim ActualCell As Integer = LastCell
            For i As Integer = 1 To Lenght

                ActualCell = Cells.NextCell(Client.Character.GetMap, ActualCell, Direction)

                'on coupe le chemin ici
                If Not IsValidCell(Client, ActualCell) Then
                    Return Cells.GetDirChar(Direction) & Cells.GetCellChars(BackCell) & ",0"
                End If

                If Not IsInFight AndAlso Client.Character.GetMap.MonsterOnCell(ActualCell) Then
                    If i = 1 Then
                        Return ",0"
                    Else
                        Return Cells.GetDirChar(Direction) & Cells.GetCellChars(BackCell) & ",0"
                    End If
                End If

                'If Client.Character.GetMap.IsInteractiveObject(ActualCell, Not (FinalCell = ActualCell)) OrElse _
                '  ((FinalCell = ActualCell) AndAlso Client.Character.GetMap.IsMonsterOnCell(ActualCell)) Then
                'If i = 1 Then
                'Return ",0"
                'Else
                'Return Cells.GetDirChar(Direction) & Cells.GetCellChars(BackCell) & ",0"
                'End If
                'End If


                If Client.Character.State.InBattle Then
                    If Not Client.Character.State.GetFight.GetFighterFromCell(ActualCell) Is Nothing Then
                        If i = 1 Then
                            Return ",0"
                        Else
                            Return Cells.GetDirChar(Direction) & Cells.GetCellChars(BackCell) & ",0"
                        End If
                    ElseIf Not Client.Character.State.GetFight.GetEnnemyNearCell(Client.Character.State.GetFighter, ActualCell) Is Nothing Then
                        Return Cells.GetDirChar(Direction) & Cells.GetCellChars(ActualCell) & ",0"
                    ElseIf Client.Character.State.GetFight.WalkOnTrap(ActualCell) Then
                        Return Cells.GetDirChar(Direction) & Cells.GetCellChars(ActualCell) & ",0"
                    End If
                End If

                BackCell = ActualCell

            Next

            Return Cell & ",1"

        End Function

        Public Shared Function RemakePath(ByVal Client As GameClient, ByVal Path As String, ByVal IsInFight As Boolean) As String

            Dim NewPath As New StringBuilder

            Dim LastCell As Integer = Client.Character.MapCell
            Dim NewCell As Integer = GetCellNum(Mid(Path, Path.Length - 1, 2))

            If Client.Character.State.InBattle Then LastCell = Client.Character.State.GetFighter.Cell
            If Not Client.Character.State.InBattle Then Client.Character.State.OriginalCell = NewCell

            For i As Integer = 0 To Path.Length - 1 Step 3

                Dim ActualCell As String = Path.Substring(i, 3)
                Dim NewLine As String = RemakeLine(Client, LastCell, ActualCell, NewCell, IsInFight)
                Dim LineData() As String = NewLine.Split(",")
                NewPath.Append(LineData(0))

                If LineData(1) = 0 Then Return NewPath.ToString()

                LastCell = Cells.GetCellNum(ActualCell.Substring(1))
            Next

            Return NewPath.ToString()
        End Function

        Public Shared Function GetPathLenght(ByVal Client As GameClient, ByVal Path As String) As Integer

            Dim LastCell As Integer = Client.Character.MapCell
            If Client.Character.State.InBattle Then LastCell = Client.Character.State.GetFighter.Cell
            Dim Lenght As Integer = 0

            For i As Integer = 0 To Path.Length - 1 Step 3

                Dim ActualCell As Integer = Cells.GetCellNum(Path.Substring(i, 3).Substring(1))
                Lenght += Cells.GoalDistance(Client.Character.GetMap, LastCell, ActualCell)
                LastCell = ActualCell

            Next
            Return Lenght

        End Function

        Public Shared Function GetPathLenght(ByVal StartCell As Integer, ByVal Map As Map, ByVal Path As String) As Integer

            Dim LastCell As Integer = StartCell
            Dim Lenght As Integer = 0

            For i As Integer = 0 To Path.Length - 1 Step 3

                Dim ActualCell As Integer = Cells.GetCellNum(Path.Substring(i, 3).Substring(1))
                Lenght += Cells.GoalDistance(Map, LastCell, ActualCell)
                LastCell = ActualCell

            Next
            Return Lenght

        End Function

#End Region

    End Class
End Namespace
