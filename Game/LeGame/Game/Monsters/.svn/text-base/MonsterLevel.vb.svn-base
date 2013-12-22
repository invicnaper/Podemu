Namespace Game
    Public Class MonsterLevel

        Public VieMaximum As Integer
        Public MaxPA, MaxPM As Integer
        Public TemplateID As Integer
        Public Level As Integer
        Public Size As Integer
        Public Grade As Integer
        Public Base As New StatsBase

        Public Resistances As New StatsResistance

        Public SpellList As New List(Of SpellLevel)

        Public ReadOnly Property Template() As MonsterTemplate
            Get
                Return MonstersHandler.GetTemplate(TemplateID)
            End Get
        End Property

    End Class
End Namespace