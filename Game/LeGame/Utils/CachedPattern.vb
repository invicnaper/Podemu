Namespace Utils
    Public Class CachedPattern

        Private m_needRefresh As Boolean = True
        Private m_provider As Func(Of String)
        Private m_cachedString As String

        Public Sub New(ByVal StringProvider As Func(Of String))
            m_provider = StringProvider
        End Sub

        Public ReadOnly Property Value As String
            Get
                If m_needRefresh Then
                    RefreshNow()
                End If
                Return m_cachedString
            End Get
        End Property

        Private Sub Refresh()
            m_cachedString = m_provider()
            m_needRefresh = False
        End Sub

        Public Sub NeedRefresh()
            m_needRefresh = True
        End Sub

        Public Sub RefreshNow()
            Refresh()
        End Sub


    End Class
End Namespace