Namespace Game
    Public Class LoginKey

        Public AccountName As String = ""
        Public TimeStamp As Long = 0
        Public Key As String = ""
        Public SqlInfos As LoginInfos

        Public Sub New(ByVal Account As String, ByVal ClientKey As String, ByVal Infos As LoginInfos)
            AccountName = Account
            TimeStamp = Environment.TickCount
            Key = ClientKey
            SqlInfos = Infos
        End Sub

        Public ReadOnly Property IsOutdated() As Boolean
            Get
                Return (Environment.TickCount > (TimeStamp + 10000))
            End Get
        End Property

    End Class
End Namespace