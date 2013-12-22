
Namespace World
    Class Levels

        Private Shared _containsKey As Boolean

        Shared Property ContainsKey(ByVal p1 As Object) As Boolean
            Get
                Return _containsKey
            End Get
            Set(ByVal value As Boolean)
                _containsKey = value
            End Set
        End Property

    End Class
End Namespace
