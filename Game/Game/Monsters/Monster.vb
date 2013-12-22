Namespace Game
    Public Class Monster

        Public Level As MonsterLevel

        Sub New(ByVal Level As MonsterLevel)

            Me.Level = Level

            Stats.Monster = True
            Stats.MyMonster = Me

            Stats.Life = Level.VieMaximum
            Stats.PA = Level.MaxPA
            Stats.PM = Level.MaxPM
            Stats.Level = Level.Level
            Stats.Stats.Base = Level.Base
            Stats.Stats.Armor.Resistances = Level.Resistances

        End Sub

        Public Id As Integer
        Public Stats As New StatsPlayer()

        Public Function GetBattleGameInfo(ByVal Cell As Integer, ByVal Team As Integer) As String

            Dim packetGM As String = "|+"

            packetGM &= Cell & ";" & 1 & ";0;" & Id & ";" & Level.TemplateID
            packetGM &= ";-2;" & Level.Template.Skin & "^" & Level.Size & ";" & Level.Grade & ";"
            For i As Integer = 0 To 2
                packetGM &= Utils.Basic.DeciToHex(Level.Template.Color(i)) & ";"
            Next
            packetGM &= "0,0,0,0" & ";" & Stats.Life & ";" & Stats.PA & ";" & Stats.PM
            packetGM &= ";0;0;0;0;0;0;0;" & Team

            Return packetGM

        End Function

    End Class
End Namespace