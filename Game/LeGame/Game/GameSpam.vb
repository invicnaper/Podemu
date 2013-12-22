Namespace Game
    Public Class GameSpam

        Private QuotaRecruitment As Integer = 0
        Private QuotaCommerce As Integer = 0

        Public LastEmote As Integer = 0
        Public LastRespawn As Integer = 0

        Public ReadOnly Property TimeRecruitment() As Integer
            Get
                Return Math.Ceiling((QuotaRecruitment - Environment.TickCount) / 1000)
            End Get
        End Property

        Public ReadOnly Property TimeCommerce() As Integer
            Get
                Return Math.Ceiling((QuotaCommerce - Environment.TickCount) / 1000)
            End Get
        End Property

        Public ReadOnly Property CanRecruitment() As Boolean
            Get
                Return TimeRecruitment <= 0
            End Get
        End Property

        Public ReadOnly Property CanSeriane() As Boolean
            Get
                Return TimeRecruitment <= 0
            End Get
        End Property

        Public ReadOnly Property CanCommerce() As Boolean
            Get
                Return TimeCommerce() <= 0
            End Get
        End Property

        Public Sub SetRecruitment()
            QuotaRecruitment = Environment.TickCount + (Utils.Config.GetItem("ANTISPAM_RECRUITMENT") * 1000)
        End Sub

        Public Sub SetCommerce()
            QuotaCommerce = Environment.TickCount + (Utils.Config.GetItem("ANTISPAM_COMMERCE") * 1000)
        End Sub

    End Class
End Namespace