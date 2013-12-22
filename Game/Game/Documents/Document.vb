Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic
Namespace Game
    Public Class Document

        Public id As Integer = 0
        Public _date As String = ""


        Public Sub Open(ByVal Client As GameClient)
            Client.Send("dC1" & id & "_" & _Date)
        End Sub

    End Class
End Namespace