Imports Podemu.World.Maps
Imports Podemu.Game
Imports Podemu.World

Namespace Utils
    Public Class Compressor

        Private Shared ZKARRAY As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_"

        Private Shared Function hashCodes(ByVal a As String) As Integer
            Return ZKARRAY.IndexOf(a)
        End Function

        Public Shared Sub DecompressMapData(ByVal Map As Map, ByVal MapData As String)

            Dim CellCount = 0
            Dim _loc11 As Integer = MapData.Length
            Dim _loc14 As Integer = 0

            While (_loc14 < _loc11)
                UncompressCell(CellCount, Map, MapData.Substring(_loc14, 10))
                _loc14 += 10
                CellCount += 1
            End While

        End Sub


        Private Shared Sub UncompressCell(ByVal Id As Integer, ByVal Map As Map, ByVal sData As String)

            Dim _loc6 As String = sData
            Dim _loc7 As Integer = _loc6.Length - 1
            Dim _loc8(_loc6.Length) As Integer

            While (_loc7 >= 0)
                _loc8(_loc7) = hashCodes(_loc6(_loc7))
                _loc7 -= 1
            End While

            Dim Los = (_loc8(0) And 1)
            Dim GL = (_loc8(1) And 15)
            Dim Movement = ((_loc8(2) And 56) >> 3)
            Dim GS = ((_loc8(4) And 60) >> 2)
            Dim IsInteractive = ((_loc8(7) And 2) >> 1)
            Dim obj2 As Integer = ((_loc8(0) And 2) << 12) + ((_loc8(7) And 1) << 12) + (_loc8(8) << 6) + _loc8(9)

            Map.CellInfos.Add(Id, New CellInfo(Los, Movement, GL, GS))

            If IsInteractive Then
                If (Game.InteractiveHandler.IsInteractive(obj2)) Then
                    Try
                        Dim template = Game.InteractiveHandler.GetTemplate(obj2, Map, Id)
                        Map.InteractiveObjects.Add(Id, template)
                    Catch ex As Exception
                    End Try
                Else
                    MyConsole.Err("Found interactive with gfx " & obj2 & " on Cell " & Id & " without matching interactive template", False)
                End If
            End If

        End Sub

    End Class
End Namespace