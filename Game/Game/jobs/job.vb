Namespace Game
    Public Class job

        Private _jobSkill As String

        Private Property JobSkill(JobID As Integer) As String
            Get
                Return _jobSkill
            End Get
            Set(value As String)
                _jobSkill = value
            End Set
        End Property

        Public Shared Sub LearnJob(ByVal TheClient As GameClient, ByVal ExtraData As String)
            Dim JobID As Integer = ExtraData
            Select Case JobID

                'JobID;SkillID~NbCases~?~?~PourcentageDeRéussite
                'JobID;Niveau;XpMin;Xp;XpMax
                '?|CasesMin|CasesMax

                Case 25 'Boulanger
                    TheClient.Send((JobID)) '"JS|25;27~2~1~0~50,109~3~0~0~100")
                    TheClient.Send("JX|25;1;0;0;50;")
                    TheClient.Send("JO4|0|2")

            End Select
            TheClient.Send("Im02;" & JobID)
        End Sub

End Class
End Namespace
