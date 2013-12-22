Namespace Game
    Public Class CharacterSpells

        Private Class SpellStruct

            Sub New(ByVal mId As Integer, ByVal mLevel As Integer, ByVal mPosition As Integer)
                ID = mId
                Level = mLevel
                Position = mPosition
            End Sub

            Public ID As Integer
            Public Level As Integer
            Public Position As Integer

        End Class

        Private SpellList As New List(Of SpellStruct)

        Public Sub ClearSpells()
            SpellList.Clear()
        End Sub

        Public Function HaveSpell(ByVal ID As Integer) As Boolean

            For Each thisSpell As SpellStruct In SpellList
                If thisSpell.ID = ID Then Return True
            Next
            Return False

        End Function

        Public Function GetSpellLevel(ByVal ID As Integer) As Integer

            For Each thisSpell As SpellStruct In SpellList
                If thisSpell.ID = ID Then Return thisSpell.Level
            Next
            Return 0

        End Function

        Public Sub AddSpell(ByVal Id As Integer, ByVal Level As Integer, Optional ByVal Position As Integer = 25)

            If HaveSpell(Id) Then Exit Sub

            If Level < 1 Then Level = 1
            If Level > 6 Then Level = 6

            If Position > 25 Then Position = 25
            If Position < 1 Then Position = 25

            SpellList.Add(New SpellStruct(Id, Level, Position))

        End Sub

        Public Function UpSpell(ByVal Id As Integer) As Boolean

            If Not HaveSpell(Id) Then Return False
            Dim ActualLevel As Integer = GetSpellLevel(Id)
            If ActualLevel = 6 Then Return False

            For Each thisSpell As SpellStruct In SpellList
                If thisSpell.ID = Id Then thisSpell.Level += 1
            Next

            Return True

        End Function

        Public Sub ChangePosition(ByVal Id As Integer, ByVal NewPosition As Integer)

            If Not HaveSpell(Id) Then Exit Sub

            For Each thisSpell As SpellStruct In SpellList
                If thisSpell.Position = NewPosition Then thisSpell.Position = 25
                If thisSpell.ID = Id Then thisSpell.Position = NewPosition
            Next

        End Sub

        Public Function GetAllSpellList() As String

            Dim ReturnPacket As String = "SL"

            For Each thisSpell As SpellStruct In SpellList
                ReturnPacket &= thisSpell.ID & "~" & thisSpell.Level & "~" & _
                    Utils.Cells.GetDirChar(thisSpell.Position) & ";"
            Next

            Return ReturnPacket

        End Function

        Public Function GetAllSpellSave() As String

            Dim ReturnData As String = ""
            Dim First As Boolean = True

            For Each thisSpell As SpellStruct In SpellList
                If (First) Then
                    First = False
                Else
                    ReturnData &= "|"
                End If
                ReturnData &= thisSpell.ID & "," & thisSpell.Level & "," & thisSpell.Position
            Next

            Return ReturnData

        End Function

    End Class
End Namespace