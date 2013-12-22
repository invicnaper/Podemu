
Namespace Game
    Class CloneObject

        Private Shared _clone As Quest
        Private Shared _clone1 As QuestObjectif
        Private Shared _clone2 As QuestStep

        Shared Property clone(ByVal quest As Quest) As Quest
            Get
                Return _clone
            End Get
            Set(ByVal value As Quest)
                _clone = value
            End Set
        End Property

        Shared Property clone(ByVal questObjectif As QuestObjectif) As QuestObjectif
            Get
                Return _clone1
            End Get
            Set(ByVal value As QuestObjectif)
                _clone1 = value
            End Set
        End Property

        Shared Property clone(ByVal questStep As QuestStep) As QuestStep
            Get
                Return _clone2
            End Get
            Set(ByVal value As QuestStep)
                _clone2 = value
            End Set
        End Property

    End Class
End Namespace
