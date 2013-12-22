Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic

Namespace Game

    Public Class Bandit
        Public ID As String
        Public Name As String
        Public Spells As New CharacterSpells
        Public OldSpells As New CharacterSpells
        Public ItemID As New Integer
        Public Skin As New Integer
        Public OldSkin As New Integer

        Public xp As Integer = 0
        Public XPNiveau() As String
        Public Niveau As Integer = 1



        Public Sub New(ByVal ID1 As Integer)
            xp = 0
            Niveau = 1

            Dim BT As BanditTemplate

            BT = BanditManager.GetBanditTemplate(ID1)

            Name = New String(BT.Name)
            Skin = BT.Skin
            ID = ID1
            Spells = BT.Spells

            XPNiveau = BT.XPNiveau

            If Niveau = 2 Then
                Spells.UpAllSpells(1)
            ElseIf Niveau = 3 Then
                Spells.UpAllSpells(2)
            ElseIf Niveau = 4 Then
                Spells.UpAllSpells(3)
            End If

        End Sub
        Public Sub Reset()
            Try
                Spells.SetAllSpellLevel(1)
                xp = 0
                Niveau = 1
            Catch
            End Try
        End Sub

        Public Sub addXP(ByVal Client As GameClient, ByVal NbreXP As Integer)
            xp += NbreXP

            If xp > XPNiveau(3) Then
                AddNiveau(Client, 4)
            ElseIf xp > XPNiveau(2) Then
                AddNiveau(Client, 3)
            ElseIf xp > XPNiveau(1) Then
                AddNiveau(Client, 2)
            ElseIf xp > XPNiveau(0) Then
                AddNiveau(Client, 1)
            End If
        End Sub

        Public Sub AddNiveau(ByVal Client As GameClient, ByVal NbreNiveau As Integer)
            Dim AvantNiveau As Integer = 0
            Niveau += NbreNiveau
            Spells.UpAllSpells(Niveau - AvantNiveau)
            Client.Send("SLo=")
            Client.Send(Client.Character.Spells.GetAllSpellList)
        End Sub

    End Class
End Namespace

