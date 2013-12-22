Imports Podemu.Utils

Namespace Game
    Public Class MobSuiveur

        Public Skin As Integer = 0
        Public Taille As Integer = 100
        Public Item As Item
        Public Mob As MonsterTemplate
        Public Tours As Integer = 0


        Public Function GetString() As String
            Dim S As String = Skin & "^" & Taille
            Return S
        End Function

    End Class
End Namespace