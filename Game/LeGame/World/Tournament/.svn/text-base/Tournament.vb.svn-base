Namespace World
    Public Class Tournament

        'Lanceur du Tournoi
        Public Launcher As Game.GameClient

        'Etat
        Public State As TournamentState

        'Compte a rebour
        Private CompteArebours As Timers.Timer
        Private Count As Integer

        'Lists des maps sur lequels se passe le tournoi
        Public Maps As List(Of Map)

        'List de inscrits au tournoi
        Public RegisterClient As List(Of Game.GameClient) = New List(Of Game.GameClient)

        'List des participants au tournoi
        Public Challengers As List(Of Challenger) = New List(Of Challenger)

        Sub New(ByVal _launcher As Game.GameClient, ByVal _time As Integer, ByVal _maps As List(Of Map))
            Launcher = _launcher
            Maps = _maps
            CompteArebours = New Timers.Timer(_time * 1000)
            AddHandler CompteArebours.Elapsed, AddressOf TimerHandler
            CompteArebours.Start()
        End Sub

        Private Sub TimerHandler(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
            If Count <> 0 Then
                Count -= 1
                Chat.SendAdministratorMessage(Launcher, "Le Tournoi commence dans " & Count * CompteArebours.Interval / 1000 & " secondes, préparez vous..")
            Else
                RemoveHandler CompteArebours.Elapsed, AddressOf TimerHandler
                Chat.SendAdministratorMessage(Launcher, "Le Tournoi commence, let's go !")
                StartTournament()
            End If
        End Sub

        Public Sub Participate(ByVal Client As Game.GameClient)
            If State = TournamentState.WAITING Then
                RegisterClient.Add(Client)
                Client.Character.State.inTournament = True
            Else
                Client.Send("Impossible de rejoindre un Tournoi déja lancé")
            End If
        End Sub

        Public Sub CancelParticipation(ByVal Client As Game.GameClient)
            If State = TournamentState.WAITING Then
                RegisterClient.Remove(Client)
                Client.Send("Vous avez annulé votre participation au tourno")
            Else
                Dim Challenger As Challenger = GetChallenger(Client)
                Challengers.Remove(Challenger)
                Client.Character.TeleportTo(Challenger.BaseMap.Id, Challenger.BaseMap.GetRandomCell(False))
                Client.Send("Vous lâcheté à pris le dessus et vous avez fuis le tournoi")
            End If
        End Sub

        Public Sub StartTournament()
            Chat.SendAdministratorMessage(Launcher, "Le Tournoi commence, affutez vos armes !")
            State = TournamentState.START
            GetPlayers()
            SharePlayerOnMap()
            For Each Challenger As Challenger In Challengers
                If Challenger.State = ChallengerState.WAITING Then
                    GetFight(Challenger)
                End If
            Next
        End Sub

        Private Sub GetPlayers()
            For Each Client As Game.GameClient In RegisterClient
                If Client.Character.State.Occuped() Then
                    Client.Send("Impossible de rejoindre le terrain de tournoi car vous êtes occupé")
                    Client.Character.State.InTournament = False
                Else
                    Client.Send("C'est parti pour le terrain de tournoi")
                    Challengers.Add(New Challenger(Client, Client.Character.GetMap))
                End If
            Next
        End Sub

        Public Sub SharePlayerOnMap()
            For Each Challenger As Challenger In Challengers
                Dim map As Map = GetRandomMap()
                Dim cell As Integer = map.GetRandomCell(False)
                Challenger.Client.Character.TeleportTo(map.Id, cell)
            Next
            Chat.SendAdministratorMessage(Launcher, "Vous êtes à présent tous réuni dans les terrains de tournoi, ça va pouvoir commencer !")
        End Sub

        Public Sub FinishFight(ByVal Client As Game.GameClient, ByVal win As Boolean)
            Dim challenger As Challenger = getChallenger(Client)

            If win Then
                challenger.Round += 1
                challenger.Victory += 1
                challenger.Opponent = Nothing
                challenger.State = ChallengerState.WAITING
                challenger.Client.Send("Et un de plus !")
                GetFight(challenger)
            Else
                challenger.Client.Send("Le tournoi s'arrète içi pour vous..")
                Challengers.Remove(challenger.Opponent)
                challenger.Client.Character.TeleportTo(challenger.BaseMap.Id, challenger.BaseMap.GetRandomCell(False))
                Client.Character.State.InTournament = False
            End If

        End Sub


        Public Sub GetFight(ByVal Challenger As Challenger)

            For Each belowChallenger As Challenger In Challengers
                While getChallengerBelow(belowChallenger.Round) = 0 Or getWaitingChallenger(belowChallenger.Round, belowChallenger) Is Nothing
                    belowChallenger.Round += 1
                End While
            Next

replay:
            If Challengers.Count = 1 Then
                Finished(Challenger)
                Exit Sub
            End If

            Dim WaitingChallenger As Challenger = getWaitingChallenger(Challenger.Round, Challenger)

            If WaitingChallenger IsNot Nothing Then
                StartFight(Challenger, WaitingChallenger)
                Exit Sub
            End If

            If getChallengerBelow(Challenger.Round) = 0 Then
                Challenger.Round += 1
                GoTo replay
            End If

            Challengers.Sort(New Sorter())

        End Sub


        Public Sub Finished(ByVal Challenger As Challenger)
            Challenger.Client.Character.TeleportTo(Challenger.BaseMap.Id, Challenger.BaseMap.GetRandomCell(False))
            Chat.SendAdministratorMessage(Launcher, "Le tournoi est maintenant fini est notre grand gagnant est :")
            Chat.SendAdministratorMessage(Launcher, Challenger.Client.Character.Name & " avec " & Challenger.Victory & " victoire")
            Chat.SendAdministratorMessage(Launcher, "Félécitation à lui qui rejoins désormais le panthéon de héros, et remporte 500pts")
        End Sub


        Public Sub StartFight(ByVal Challenger1 As Challenger, ByVal Challenger2 As Challenger)
            Challenger1.Opponent = Challenger2
            Challenger2.Opponent = Challenger1
            Challenger1.State = ChallengerState.FIGHTING
            Challenger2.State = ChallengerState.FIGHTING
            Challenger1.Client.Character.GetMap.Send("GA;906;" & Challenger1.Client.Character.ID & ";" & Challenger2.Client.Character.ID)
            FightsHandler.AddFight(New Fight(Challenger1.Client, Challenger2.Client, Fight.FightType.Agression))
        End Sub


        Public Function getWaitingChallenger(ByVal round As Integer, ByVal player As Challenger) As Challenger
            For Each Challenger As Challenger In Challengers
                If Challenger.State = ChallengerState.WAITING And Not Challenger.Equals(player) And Challenger.Round = round Then
                    Return Challenger
                End If
            Next
            Return Nothing
        End Function

        Public Function getChallenger(ByVal round As Integer) As Integer
            Dim i As Integer = 0
            For Each Challenger As Challenger In Challengers
                If Challenger.Round = round Then
                    i += 1
                End If
            Next
            Return i
        End Function

        Public Function getChallengerBelow(ByVal round As Integer) As Integer
            Dim i As Integer = 0
            For Each Challenger As Challenger In Challengers
                If Challenger.Round < round Then
                    i += 1
                End If
            Next
            Return i
        End Function

        Public Function getChallengerAbove(ByVal round As Integer) As Integer
            Dim i As Integer = 0
            For Each Challenger As Challenger In Challengers
                If Challenger.Round > round Then
                    i += 1
                End If
            Next
            Return i
        End Function


        Public Function GetRandomMap() As Map
            Randomize()
            Dim n As Integer = Math.Round(Rnd() * Maps.Count)
            Return Maps(n)
        End Function

        Public Function GetChallenger(ByVal _client As Game.GameClient) As Challenger
            For Each Challenger As Challenger In Challengers
                If Challenger.Client.Equals(_client) Then
                    Return Challenger
                End If
            Next
            Return Nothing
        End Function


    End Class
End Namespace
