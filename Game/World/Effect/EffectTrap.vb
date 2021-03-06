﻿Imports Podemu.Game
Imports Podemu.Utils

Namespace World
    Public Class EffectTrap

        Public Owner As Fighter
        Public Spell As SpellLevel

        Public Sub New(ByVal Player As Fighter, ByVal OriginalSpell As Integer, ByVal SpellId As Integer, ByVal SpellLevel As Integer, ByVal Cell As Integer)
            Owner = Player
            Spell = SpellsHandler.GetSpell(SpellId).AtLevel(SpellLevel)
            Lenght = Cells.GetDirNum((SpellsHandler.GetSpell(OriginalSpell).AtLevel(SpellLevel).TypePortee).Substring(1, 1))
            Me.Cell = Cell
            Me.OriginalSpell = OriginalSpell
        End Sub

        Public Sub New(ByVal Player As Fighter, ByVal OriginalSpell As Integer, ByVal SpellId As Integer, ByVal SpellLevel As Integer, ByVal Cell As Integer, ByVal GlyphColor As Integer, ByVal Turn As Integer)
            Owner = Player
            Spell = SpellsHandler.GetSpell(SpellId).AtLevel(SpellLevel)
            Lenght = Cells.GetDirNum((SpellsHandler.GetSpell(OriginalSpell).AtLevel(SpellLevel).TypePortee).Substring(1, 1))
            Me.Cell = Cell
            Me.Turn = Turn
            Glyph = True
            Color = GlyphColor
        End Sub

        Public Cell As Integer
        Public Lenght As Integer
        Public OriginalSpell As Integer

        Public Glyph As Boolean = False
        Public Color As Integer = 0
        Public Turn As Integer = 0

        Public Sub Use(ByVal Fight As Fight)
            Fight.LaunchSpell(Owner, Spell, Cell, True)
        End Sub

        Public Sub Use(ByVal Fight As Fight, ByVal Cell As Integer)
            Fight.LaunchSpell(Owner, Spell, Cell, True)
        End Sub

    End Class
End Namespace