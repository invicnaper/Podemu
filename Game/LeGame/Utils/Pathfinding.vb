Imports System.Text
Imports Podemu.World

Namespace Utils
    Public Class Pathfinding

        Private openlist As New List(Of Short)(500)
        Private closelist As New List(Of Short)(500)
        Private Plist(1025) As Integer
        Private Flist(1025) As Integer
        Private Glist(1025) As Integer
        Private Hlist(1025) As Integer

        Private FourDir, isFight As Boolean
        Private nombreDePM As Integer

        Private Sub loadCell(ByVal CellCount As Integer)

            For i = 0 To CellCount
                Plist(i) = 0
                Flist(i) = 0
                Glist(i) = 0
                Hlist(i) = 0
            Next

        End Sub

        Private Sub loadSprites(ByVal Map As Map, ByVal MyFight As Fight, ByVal CellCount As Integer)

            For i As Integer = 1 To CellCount
                If Not Map.IsCellReachable(i) Then
                    closelist.Add(i)
                End If
            Next

            If MyFight Is Nothing Then Exit Sub

            For Each Fighter As Fighter In MyFight.Fighters
                If Not Fighter.Dead AndAlso Not Fighter.Abandon Then
                    closelist.Add(Fighter.Cell)
                End If
            Next
       
        End Sub

        Public Function Pathing(ByVal Map As Map, ByVal nCellBegin As Integer, ByVal nCellEnd As Integer, Optional ByVal FourDir As Boolean = False, Optional ByVal numberPM As Integer = 9999, Optional ByVal IsInFight As Boolean = False, Optional ByVal MyFight As Fight = Nothing) As String

            Try

                Dim CellCount As Short = Map.CellInfos.Count

                loadCell(cellCount)
                loadSprites(Map, MyFight, cellCount)
                closelist.Remove(nCellBegin)
                closelist.Remove(nCellEnd)

                Me.FourDir = FourDir
                Me.isFight = IsInFight

                nombreDePM = numberPM
                Dim returnPath As String = findpath(Map, nCellBegin, nCellEnd)

                Return cleanPath(returnPath)

            Catch ex As Exception
                Return ""
            End Try

        End Function

        Private Function findpath(ByVal Map As Map, ByVal cell1 As Integer, ByVal cell2 As Integer) As String

            Dim current As Integer
            Dim i As Integer = 0

            openlist.Add(cell1)

            Do Until openlist.Contains(cell2)
                i += 1
                If i > 1000 Then Return ""

                current = getFpoint()
                If current = cell2 Then Exit Do
                closelist.Add(current)
                openlist.Remove(current)

                For Each cell As Integer In getChild(Map, current)

                    If closelist.Contains(cell) = False Then

                        If openlist.Contains(cell) Then

                            If Glist(current) + 5 < Glist(cell) Then
                                Plist(cell) = current
                                Glist(cell) = Glist(current) + 5
                                Hlist(cell) = Cells.GoalDistance(Map, cell, cell2)
                                Flist(cell) = Glist(cell) + Hlist(cell)
                            End If

                        Else
                            openlist.Add(cell)
                            openlist.Item(openlist.Count - 1) = cell
                            Glist(cell) = Glist(current) + 5
                            Hlist(cell) = Cells.GoalDistance(Map, cell, cell2)
                            Flist(cell) = Glist(cell) + Hlist(cell)
                            Plist(cell) = current
                        End If

                    End If

                Next
            Loop

            Return (getParent(Map, cell1, cell2))

        End Function

        Private Function getParent(ByVal Map As Map, ByVal cell1 As Integer, ByVal cell2 As Integer) As String

            If cell1 = 0 Or cell2 = 0 Then
                Return ""
            End If

            Dim current As Integer = cell2
            Dim pathCell As New List(Of Short)
            pathCell.Add(current)

            Do Until current = cell1
                pathCell.Add(Plist(current))
                current = Plist(current)
            Loop

            Return getPath(Map, pathCell)
        End Function

        Private Function getPath(ByVal Map As Map, ByVal pathCell As List(Of Short)) As String
            pathCell.Reverse()
            Dim pathing As New StringBuilder
            Dim current As Integer
            Dim child As Integer
            Dim PMUsed As Integer = 0
            For i = 0 To pathCell.Count - 2
                PMUsed += 1
                If (PMUsed > nombreDePM) Then Return pathing.ToString()
                current = pathCell(i)
                child = pathCell(i + 1)
                pathing.Append(getOrientation(Map, current, child) & Cells.GetCellChars(CInt(child)))
            Next

            Return pathing.ToString()
        End Function

        Private Function getChild(ByVal Map As Map, ByVal cell As Integer) As List(Of Short)

            Dim x As Integer = Cells.GetX(Map, cell)
            Dim y As Integer = Cells.GetY(Map, cell)
            Dim children As New List(Of Short)
            Dim temp As Integer
            Dim locx As Integer, locy As Integer

            If FourDir = False Then

                temp = cell - 29
                locx = Cells.GetX(Map, temp)
                locy = Cells.GetY(Map, temp)
                If temp > 1 And temp < 1024 And locx = x - 1 And locy = y - 1 AndAlso closelist.Contains(temp) = False Then
                    children.Add(temp)
                End If

                temp = cell + 29
                locx = Cells.GetX(Map, temp)
                locy = Cells.GetY(Map, temp)
                If temp > 1 And temp < 1024 And locx = x + 1 And locy = y + 1 AndAlso closelist.Contains(temp) = False Then
                    children.Add(temp)
                End If

            End If

            temp = cell - 15
            locx = Cells.GetX(Map, temp)
            locy = Cells.GetY(Map, temp)
            If temp > 1 And temp < 1024 And locx = x - 1 And locy = y AndAlso closelist.Contains(temp) = False Then
                children.Add(temp)
            End If

            temp = cell + 15
            locx = Cells.GetX(Map, temp)
            locy = Cells.GetY(Map, temp)
            If temp > 1 And temp < 1024 And locx = x + 1 And locy = y AndAlso closelist.Contains(temp) = False Then
                children.Add(temp)
            End If

            temp = cell - 14
            locx = Cells.GetX(Map, temp)
            locy = Cells.GetY(Map, temp)
            If temp > 1 And temp < 1024 And locx = x And locy = y - 1 AndAlso closelist.Contains(temp) = False Then
                children.Add(temp)
            End If

            temp = cell + 14
            locx = Cells.GetX(Map, temp)
            locy = Cells.GetY(Map, temp)
            If temp > 1 And temp < 1024 And locx = x And locy = y + 1 AndAlso closelist.Contains(temp) = False Then
                children.Add(temp)
            End If

            If FourDir = False Then

                temp = cell - 1
                locx = Cells.GetX(Map, temp)
                locy = Cells.GetY(Map, temp)
                If temp > 1 And temp < 1024 And locx = x - 1 And locy = y + 1 AndAlso closelist.Contains(temp) = False Then
                    children.Add(temp)
                End If

                temp = cell + 1
                locx = Cells.GetX(Map, temp)
                locy = Cells.GetY(Map, temp)
                If temp > 1 And temp < 1024 And locx = x + 1 And locy = y - 1 AndAlso closelist.Contains(temp) = False Then
                    children.Add(temp)
                End If

            End If

            Return children

        End Function

        Private Function getFpoint() As Integer

            Dim x As Integer = 9999
            Dim cell As Integer

            For Each item As Integer In openlist
                If closelist.Contains(item) = False Then
                    If Flist(item) < x Then
                        x = Flist(item)
                        cell = item
                    End If
                End If
            Next

            Return cell
        End Function

        Public Function getOrientation(ByVal Map As Map, ByVal cell1 As Integer, ByVal cell2 As Integer) As String

            Return Cells.GetDirChar(Cells.GetDirection(Map, cell1, cell2))

        End Function

        Private Function cleanPath(ByVal path As String) As String

            Dim cleanedPath As New StringBuilder()

            If (path.Length > 3) Then
                For i As Integer = 1 To path.Length Step 3
                    If (Mid(path, i, 1) <> Mid(path, i + 3, 1)) Then
                        cleanedPath.Append(Mid(path, i, 3))
                    End If
                Next
            Else
                Return path
            End If

            Return cleanedPath.ToString()

        End Function

    End Class
End Namespace