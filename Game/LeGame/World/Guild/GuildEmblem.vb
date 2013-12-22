Imports Podemu.Utils
Imports Podemu.Game

Namespace World
    Public Class GuildEmblem

        Public FrontId As Integer = 0
        Public FrontColor As Integer = 0
        Public BackId As Integer = 0
        Public BackColor As Integer = 0

        Public Overrides Function ToString() As String
            Return Basic.ToBase36(FrontId) & "," & Basic.ToBase36(FrontColor) & "," & Basic.ToBase36(BackId) & "," & Basic.ToBase36(BackColor)
        End Function

        Public ReadOnly Property ToSave As String
            Get
                Return FrontId & "," & FrontColor & "," & BackId & "," & BackColor
            End Get
        End Property

    End Class
End Namespace