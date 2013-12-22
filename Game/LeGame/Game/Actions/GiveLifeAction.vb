Imports Podemu.Utils

Namespace Game.Actions
    Public Class GiveLifeAction : Inherits BaseAction


        Sub New()
            MyBase.New("life", "anim")
        End Sub

        Public Overrides Sub Process(ByVal User As GameClient, ByVal TargetCell As String, ByVal Params As System.Collections.Generic.Dictionary(Of String, String))

            Dim Life = Params("life")

            If Life.Contains("at") Then
                Life = Basic.Rand(Life.Split("at")(0), Life.Split("at")(1))
            End If

            User.Character.Player.Life += Life

            If Params("anim") = "bierre" Then
                User.Character.PlayEmote(16)
            ElseIf Params("anim") = "pain" Then
                User.Character.PlayEmote(17)
            End If

            User.Character.SendAccountStats()

            If Params.ContainsKey("mess") Then
                User.SendNormalMessage(Params("mess"))
            End If

        End Sub



    End Class
End Namespace