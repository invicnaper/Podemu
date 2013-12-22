Namespace Game.Actions
    Public MustInherit Class BaseAction

        Public ReadOnly ExpectedParams As List(Of String)

        Sub New(ByVal ParamArray params As String())
            ExpectedParams = params.ToList()
        End Sub

        Public MustOverride Sub Process(ByVal User As GameClient, ByVal TargetCell As String, ByVal Params As Dictionary(Of String, String))

    End Class
End Namespace