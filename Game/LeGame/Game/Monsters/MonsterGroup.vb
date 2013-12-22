Imports Podemu.Utils

Namespace Game
    Public Class MonsterGroup

        Public MonsterList As New List(Of Monster)(8)
        Public Cell, Dir As Integer
        Public Id As Integer

        Public HasWin As Boolean
        Public SetDate As DateTime = Date.Now

        Public Items As New List(Of Item)
        Public Kamas As Long

        Public ReadOnly Property GetGameInfo() As String
            Get
                Dim Bonus As Integer = BonusPercent
                If Bonus > 1000 Then Bonus = 1000
                Dim GameInfo As String = "|+" & Cell & ";" & Dir & ";" & Bonus & ";" & Id & ";"
                Dim MonsterIDs As String = ""
                Dim MonsterSkins As String = ""
                Dim MonsterLvls As String = ""
                Dim ColorsAndCo As String = ""

                Dim First As Boolean = True
                For Each Fighter As Monster In MonsterList
                    If First Then
                        First = False
                    Else
                        MonsterIDs &= ","
                        MonsterSkins &= ","
                        MonsterLvls &= ","
                        ColorsAndCo &= ";"
                    End If
                    Dim Template As MonsterTemplate = Fighter.Level.Template
                    MonsterIDs &= Template.Id
                    MonsterSkins &= Fighter.Level.Template.Skin & "^" & Fighter.Level.Size
                    MonsterLvls &= Fighter.Level.Level
                    ColorsAndCo &= Utils.Basic.DeciToHex(Template.Color(0)) & ","
                    ColorsAndCo &= Utils.Basic.DeciToHex(Template.Color(1)) & ","
                    ColorsAndCo &= Utils.Basic.DeciToHex(Template.Color(2)) & ";"
                    ColorsAndCo &= "0,0,0,0"
                Next

                GameInfo &= MonsterIDs & ";-3;" & MonsterSkins & ";" & MonsterLvls & ";" & ColorsAndCo
                Return GameInfo
            End Get
        End Property

        Public ReadOnly Property AliveTime As Integer
            Get
                Return Date.Now.Subtract(SetDate).TotalSeconds
            End Get
        End Property

        Public ReadOnly Property BonusPercent As Integer
            Get
                Return AliveTime / Config.GetItem(Of Double)("BONUS_TIME") * If(HasWin, 2, 1)
            End Get
        End Property

        Public ReadOnly Property Level As Integer
            Get
                Return MonsterList.Sum(Function(m) m.Level.Level)
            End Get
        End Property

        Public Shared Function RandomFromList(ByVal Nbr As Integer, ByVal List As List(Of MonsterTemplate)) As MonsterGroup
            Dim MonsterGroup As New MonsterGroup()

            For i As Integer = 1 To Nbr
                Dim NewMonster As New Monster(List(Basic.Rand(0, List.Count - 1)).GetRandomLevel())
                MonsterGroup.MonsterList.Add(NewMonster)
            Next

            Return MonsterGroup
        End Function


    End Class
End Namespace