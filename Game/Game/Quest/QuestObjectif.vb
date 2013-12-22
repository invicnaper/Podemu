Imports Podemu.Utils
Namespace Game
    <Serializable()> _
    Public Class QuestObjectif


        Public id As Integer = 0
        Public Ordre As Integer = 0
        Public type As QuestObjectifType = QuestObjectifType.Autre
        Public arguments As String = 0
        Public IsInvisble As Boolean = False
        Public fini As Integer = 0



        Public Sub New2(ByVal _id As Integer, ByVal _type As Integer, ByVal _args As String)
            id = _id
            type = _type
            arguments = _args
            fini = 0
        End Sub
    End Class

    Public Enum QuestObjectifType
        Autre = 0
        Aller_voir = 1
        Montrer_à = 2
        Ramener_à = 3
        Découvrir_la_carte = 4
        Découvrir_la_zone = 5
        Vaincre_en_un_seul_combat = 6
        Monstre_à_vaincre = 7
        Utiliser = 8
        Retourner_voir = 9
        Escorter = 10
        Vaincre_un_joueur_en_défi = 11
        Eliminer = 13

        'added :

        Avis_de_recherche = 14

    End Enum
End Namespace
