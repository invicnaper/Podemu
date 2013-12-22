﻿Imports System.Text
Imports Podemu.Utils

Namespace Game.Interactives
    Public Class Phenix : Inherits InteractiveObject

        Public Sub New(ByVal Map As World.Map, ByVal CellId As Integer)

            MyBase.New(Map, CellId, New List(Of Integer)(New Integer() {0}))

        End Sub

        Public Overrides Sub Use(ByVal Client As GameClient, ByVal SkillId As Integer)

            If SkillId = 0 And Client.Character.State.IsGhost Then

                Client.Character.State.IsGhost = False
                Client.Character.IsDead = False
                Client.Character.Player.Energy = 2000

                Client.Character.Skin = Client.Character.Classe & Client.Character.Sexe
                Client.Character.Restriction.SetValue(CharacterRestriction.RestrictionEnum.IsSlow, False)
                Client.Character.Restriction.SetValue(CharacterRestriction.RestrictionEnum.ForceWalk, False)

                Map.RefreshCharacter(Client)

            End If

        End Sub


    End Class
End Namespace