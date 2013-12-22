Imports Podemu.Utils.Basic

Namespace Game
    Public Class CharacterQuests

        Private QuestList As New List(Of Quest)
        Private QuestTermineList As New List(Of Quest)


#Region "Autre"

        Public Sub init(ByVal Chaine As String)
            If Not Chaine = "" Or Chaine Is Nothing Then

                Dim s1 As String() = Chaine.Split("|")
                For i As Integer = 0 To s1.Length - 1
                    Dim s2 As String() = s1(i).Split(";")
                    Dim newQuest As New Quest
                    newQuest = CloneObject.clone(QuestManager.GetQuest(s2(0)))
                    newQuest.NowStep = newQuest.GetStepByOrdre(s2(2))
                    If s2(1) = 1 Then
                        QuestTermineList.Add(newQuest)
                    Else
                        QuestList.Add(newQuest)
                    End If
                Next

            End If
        End Sub


        Public Sub AddQuest(ByVal Client As GameClient, ByVal id As Integer)
            If Not Contain(id) And Not ContainTermine(id) Then
                Dim newQuest As New Quest
                newQuest = CloneObject.clone(QuestManager.GetQuest(id))
                QuestList.Add(newQuest)
                Client.SendNormalMessage("Vous commencez une nouvelle quête : " & newQuest.Name)
            ElseIf Contain(id) Then
                Client.SendNormalMessage("Vous êtes déjà entrain de faire cette quête")
            ElseIf ContainTermine(id) Then
                Dim Q As Quest = GetPersoQuestTermine(id)
                QuestTermineList.Remove(Q)
                AddQuest(Client, id)
            End If
            Client.Character.Save()
        End Sub

        Public Sub RemplirObjectif(ByVal Client As GameClient, ByVal Questid As Integer, ByVal ObjectifId As Integer)
            If Contain(Questid) Then
                GetPersoQuest(Questid).RemplirObjectif(Client, ObjectifId)
            End If
        End Sub

        Public Sub RemplirObjectifMap(ByVal Client As GameClient, ByVal Mapid As Integer)
            For Each Q As Quest In QuestList
                Q.NowStep.TryObjectifMap(Client, Mapid, Q)
            Next
        End Sub

        Public Sub RemplirObjectifAvis(ByVal Client As GameClient, ByVal Monsterid As Integer)
            For Each Q As Quest In QuestList
                Q.NowStep.TryObjectifAvis(Client, Monsterid, Q)
            Next
        End Sub


        Public Sub PasseTerminerQuest(ByVal Client As GameClient, ByVal Q As Quest)
            QuestList.Remove(Q)
            QuestTermineList.Add(Q)
            Client.SendNormalMessage("Bravo ! Vous avez fini la quête : " & Q.Name)
        End Sub

#End Region

#Region "Packets"

        Public Function GetPacketList() As String
            Dim Packet As String = "QL1"
            Dim compte As Integer = 2
            If QuestList.Count >= 1 Then
                Packet += QuestList(0).id & ";0;1"
                If QuestList.Count > 1 Then
                    For i As Integer = 1 To QuestList.Count - 1
                        Packet += "|" & QuestList(i).id & ";0;" & compte
                        compte += 1
                    Next
                End If
            End If

            If QuestTermineList.Count >= 1 Then
                Packet += "|" & QuestTermineList(0).id & ";1;" & compte
                compte += 1
                If QuestTermineList.Count > 1 Then
                    For i As Integer = 1 To QuestTermineList.Count - 1
                        Packet += "|" & QuestTermineList(i).id & ";1;" & compte
                        compte += 1
                    Next
                End If
            End If

            Return Packet

        End Function

        Public Function GetPacketStep(ByVal Questid As Integer) As String
            'QSidquete|idstep|idobjectif1,fini?;idobjectif2,fini?;...|idstepavant1;idstepavant2;...|idst epapres1;idstepapres2;...|dialogid;x,x
            Dim Q As Quest = GetPersoQuest(Questid)
            If Q Is Nothing Then Return ""
            Dim Packet As String = "QS"
            Packet += Q.id & "|" & Q.NowStep.id & "|"
            Packet += Q.NowStep.Objectifs(0).id & "," & Q.NowStep.Objectifs(0).fini
            If Q.NowStep.Objectifs.Count > 1 Then
                For i As Integer = 1 To Q.NowStep.Objectifs.Count - 1
                    If Not Q.NowStep.Objectifs(i).IsInvisble Then
                        Packet += ";" & Q.NowStep.Objectifs(i).id & "," & Q.NowStep.Objectifs(i).fini
                    End If
                Next
            End If
            Packet += "|" & GetStepsAvant(Q) & "|" & GetStepsApres(Q)
            If Not Q.NowStep.Dialogue = 0 Then
                Packet += "|" & Q.NowStep.Dialogue
            End If

            Return Packet
        End Function

#End Region

#Region "Propriétés"

        Public Function GetSAveString() As String
            Dim Packet As String = ""
            If QuestList.Count >= 1 Then
                Packet += QuestList(0).id & ";0;" & QuestList(0).NowStep.Ordre
                If QuestList.Count > 1 Then
                    For i As Integer = 1 To QuestList.Count - 1
                        Packet += "|" & QuestList(i).id & ";0;" & QuestList(i).NowStep.Ordre
                    Next
                End If
            End If
            If QuestTermineList.Count >= 1 Then
                Packet += "|" & QuestTermineList(0).id & ";1;" & QuestTermineList(0).NowStep.Ordre
                If QuestTermineList.Count > 1 Then
                    For i As Integer = 1 To QuestTermineList.Count - 1
                        Packet += "|" & QuestTermineList(i).id & ";1;" & QuestTermineList(i).NowStep.Ordre
                    Next
                End If
            End If

            Return Packet
        End Function


        Public Function GetPersoQuest(ByVal id As Integer) As Quest
            For Each Q As Quest In QuestList
                If Q.id = id Then
                    Return Q
                End If
            Next
            Return Nothing
        End Function

        Public Function GetPersoQuestTermine(ByVal id As Integer) As Quest
            For Each Q As Quest In QuestTermineList
                If Q.id = id Then
                    Return Q
                End If
            Next
            Return Nothing
        End Function

        Private Function GetStepsAvant(ByVal Q As Quest) As String
            If Not Q.NowStep.Ordre = 0 Then

                Dim S As String = Q.GetStepByOrdre(0).id
                For i As Integer = 1 To Q.Steps.Count - 1
                    If Q.GetStepByOrdre(i).Ordre >= Q.NowStep.Ordre Then

                    Else
                        S += ";" & Q.Steps(i).id
                    End If
                Next
                Return S

            End If
            Return ""
        End Function


        Private Function GetStepsApres(ByVal Q As Quest) As String
            If Not Q.NowStep.Ordre + 1 = Q.Steps.Count Then

                Dim S As String = Q.GetStepByOrdre(Q.NowStep.Ordre + 1).id
                If Q.Steps.Count - Q.NowStep.Ordre > 2 Then
                    For i As Integer = 1 To Q.Steps.Count - 1
                        If Q.GetStepByOrdre(i).Ordre <= Q.NowStep.Ordre Then

                        Else
                            S += ";" & Q.Steps(i).id
                        End If
                    Next
                End If
                Return S

            End If
            Return ""
        End Function

        Public Function Contain(ByVal id As Integer) As Boolean
            For Each Q As Quest In QuestList
                If Q.id = id Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function ContainTermine(ByVal id As Integer) As Boolean
            For Each Q As Quest In QuestTermineList
                If Q.id = id Then
                    Return True
                End If
            Next
            Return False
        End Function



#End Region

    End Class
End Namespace
