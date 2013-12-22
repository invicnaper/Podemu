Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic
Namespace Game
    Public Class Animation

        Public Id As Integer = 0
        Public IdsAnims As String = ""
        Public Name As String = ""
        Public Mapid As Integer = 0
        Public Cellid As Integer = 0
        Public GainObjects As New List(Of String)
        Public GainKamas As Integer = 0
        Public GainXp As Integer = 0


        Public Function GetIdAnim() As Integer

            Select Case Id

                Case 1
                    Dim s1 As String() = IdsAnims.Split(";")

                    Dim MyRandomize As Integer = 0
                    Randomize()
                    MyRandomize = Int(1000 * Rnd())
                    If MyRandomize = 500 Then
                        Return s1(0)
                    Else
                        Return s1(1)
                    End If
                Case 3
                    Threading.Thread.Sleep(3000)
                    Return IdsAnims
                Case Else
                    Return IdsAnims

            End Select

        End Function
    End Class
End Namespace
