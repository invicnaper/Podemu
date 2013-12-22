Imports Podemu.Utils
Namespace Game
    <Serializable()> _
    Public Class QuestStep

        Public id As Integer = 0
        Public Ordre As Integer = 0
        Public Objectifs As New List(Of QuestObjectif)
        Public GainObjects As New List(Of String)
        Public GainKamas As Integer = 0
        Public GainXp As Integer = 0
        Public Dialogue As Integer = 0


        Public Sub RemplirObjectif(ByVal Client As GameClient, ByVal ObjectifId As Integer, ByVal Quest As Quest)
            For Each Obj As QuestObjectif In Objectifs
                If Not Obj.id = ObjectifId Then Continue For
                If Obj.IsInvisble Then Continue For
                Select Case Obj.type

                    Case 0
                        CompleteObjectif(Obj, Client, Quest)
                    Case 1
                        CompleteObjectif(Obj, Client, Quest)
                    Case 3
                        Dim s1 As String() = Obj.arguments.Split(",")
                        Dim iditem As Integer = s1(0)
                        Dim nbrefois As Integer = s1(1)

                        Dim nbreditem As Integer = Client.Character.Items.compteItem(iditem)

                        If nbreditem >= nbrefois Then
                            Client.Character.Items.DeleteItem2(Client, iditem, nbrefois)
                            Client.Character.SendAccountStats()
                            Client.Character.Save()
                            CompleteObjectif(Obj, Client, Quest)
                        Else
                            Client.SendNormalMessage("Vous n'avez pas les objets requis")
                        End If
                    Case 14
                        CompleteObjectif(Obj, Client, Quest)
                End Select

            Next
            Client.Character.Save()
        End Sub

        Private Sub CompleteObjectif(ByVal obj As QuestObjectif, ByVal Client As GameClient, ByVal Quest As Quest)
            obj.fini = 1
            Dim encore As Boolean = False
            For Each obje As QuestObjectif In Objectifs
                If obje.fini = 0 Then encore = True
            Next
            If Not encore Then
                'go to next step
                Quest.NextStep(Client)
            Else
                If Objectifs(obj.Ordre + 1).IsInvisble Then
                    Objectifs(obj.Ordre + 1).IsInvisble = False
                End If
                Client.SendNormalMessage("Quête mise à jour : " & Quest.Name)
            End If
            Client.Character.Save()
        End Sub

        Public Sub TryObjectifMap(ByVal Client As GameClient, ByVal Mapid As Integer, ByVal Quest As Quest)
            For Each Obj As QuestObjectif In Objectifs
                If Obj.type = 4 Then
                    If Obj.arguments = Mapid Then
                        CompleteObjectif(Obj, Client, Quest)
                    End If
                End If
            Next
        End Sub

        Public Sub TryObjectifAvis(ByVal Client As GameClient, ByVal Monsterid As Integer, ByVal Quest As Quest)
            For Each Obj As QuestObjectif In Objectifs
                If Obj.type = 14 Then
                    If Obj.arguments = Monsterid Then
                        'Client.Character.MobsSuiveurs.AddMobSuiveur(Client, Monsterid)
                        CompleteObjectif(Obj, Client, Quest)
                        Client.Character.GetMap.RefreshCharacter(Client)
                    End If
                End If
            Next
        End Sub


    End Class

End Namespace