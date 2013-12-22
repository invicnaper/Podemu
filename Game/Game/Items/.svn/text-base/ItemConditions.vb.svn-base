Namespace Game
    Public Class ItemConditions

#Region " Sous classe "

        Private Class ItemCondition

            Public Type As String
            Public Spliter As String
            Public Value As String
            Public SousConditions As List(Of ItemCondition)

        End Class

#End Region

#Region " Création "

        Private ConditionList As New List(Of ItemCondition)

        Public Sub New(ByVal StringConditions As String)

            StringConditions = StringConditions.Replace("(", "").Replace(")", "")

            Dim ConditionsData() As String = StringConditions.Split("&")

            For Each Condition As String In ConditionsData

                If Condition <> "" Then

                    If Condition.Contains("|") Then

                        Dim SurCondition As New ItemCondition
                        Dim SousConditions As New List(Of ItemCondition)
                        Dim SousConditionsData() As String = Condition.Split("|")

                        For Each SousCondition As String In SousConditionsData

                            Dim NewSousCondition As ItemCondition = GetConditionFromData(Condition)
                            If Not NewSousCondition Is Nothing Then
                                SousConditions.Add(NewSousCondition)
                            End If

                        Next

                        SurCondition.SousConditions = SousConditions

                        ConditionList.Add(SurCondition)

                    End If

                    Dim NewCondition As ItemCondition = GetConditionFromData(Condition)
                    If Not NewCondition Is Nothing Then
                        ConditionList.Add(NewCondition)
                    End If

                End If

            Next

        End Sub

        Private Function GetConditionFromData(ByVal Condition As String) As ItemCondition

            Dim Spliter As String = ""

            Select Case True

                Case Condition.Contains("<")
                    Spliter = "<"
                Case Condition.Contains(">")
                    Spliter = ">"
                Case Condition.Contains("=")
                    Spliter = "="
                Case Condition.Contains("!")
                    Spliter = "!"
                Case Condition.Contains("X")
                    Spliter = "X"
                Case Condition.Contains("~")
                    Spliter = "~"

            End Select

            If Spliter <> "" Then
                Dim ConditionData() As String = Condition.Split(Spliter)
                Dim NewCondition As New ItemCondition
                NewCondition.Type = ConditionData(0)
                NewCondition.Spliter = Spliter
                NewCondition.Value = If(ConditionData(1).Contains(","), ConditionData(1).Split(",")(0), ConditionData(1))
                Return NewCondition
            End If

            Return Nothing

        End Function

#End Region

#Region " Vérification "

        Private Sub UnknownCondition(ByVal ContitionType As String)

        End Sub

        Private Function VerifyCondition(ByVal Character As Character, ByVal Condition As ItemCondition) As Boolean

            Dim ToCompare As String = ""

            Select Case Condition.Type(0)

                Case "C"

                    Select Case Condition.Type(1)

                        Case "a"
                            ToCompare = Character.Player.Stats.Base.Agilite.Base
                        Case "i"
                            ToCompare = Character.Player.Stats.Base.Intelligence.Base
                        Case "c"
                            ToCompare = Character.Player.Stats.Base.Chance.Base
                        Case "s"
                            ToCompare = Character.Player.Stats.Base.Force.Base
                        Case "v"
                            ToCompare = Character.Player.Stats.Base.Vitalite.Base
                        Case "w"
                            ToCompare = Character.Player.Stats.Base.Sagesse.Base

                        Case "A"
                            ToCompare = Character.Player.Stats.Base.Agilite.Total
                        Case "I"
                            ToCompare = Character.Player.Stats.Base.Intelligence.Total
                        Case "C"
                            ToCompare = Character.Player.Stats.Base.Chance.Total
                        Case "S"
                            ToCompare = Character.Player.Stats.Base.Force.Total
                        Case "V"
                            ToCompare = Character.Player.Stats.Base.Vitalite.Total
                        Case "W"
                            ToCompare = Character.Player.Stats.Base.Sagesse.Total

                        Case Else
                            UnknownCondition(Condition.Type)

                    End Select

                Case "P"

                    Select Case Condition.Type(1)

                        Case "G"
                            ToCompare = Character.Classe

                        Case "N"
                            ToCompare = Character.Name

                        Case "L"
                            ToCompare = Character.Player.Level

                        Case "K"
                            ToCompare = Character.Player.Kamas

                        Case "P"
                            ToCompare = Character.Player.Alignment.Rank

                        Case "s"
                            ToCompare = Character.Player.Alignment.Id

                        Case Else
                            UnknownCondition(Condition.Type)

                    End Select

                Case "B"

                    Select Case Condition.Type(1)

                        Case "I"
                            ToCompare = Character.ID

                        Case Else
                            UnknownCondition(Condition.Type)

                    End Select

                Case Else
                    UnknownCondition(Condition.Type)

            End Select

            Dim Result As Boolean = True

            If ToCompare <> "" Then

                Select Case Condition.Spliter

                    Case "<"
                        If Not CLng(ToCompare) < CLng(Condition.Value) Then Return False

                    Case ">"
                        If Not CLng(ToCompare) > CLng(Condition.Value) Then Return False

                    Case "=", "~"
                        If Not ToCompare = Condition.Value Then Return False

                    Case "!"
                        If Not ToCompare <> Condition.Value Then Return False

                End Select

            End If

            Return True

        End Function

        Private Function VerifySousConditions(ByVal Character As Character, ByVal SurCondition As ItemCondition) As Boolean

            For Each Condition As ItemCondition In SurCondition.SousConditions
                If VerifyCondition(Character, Condition) Then Return True
            Next

            Return False

        End Function

        Public Function VerifyConditions(ByVal Character As Character) As Boolean

            For Each Condition As ItemCondition In ConditionList

                If Condition.SousConditions Is Nothing Then
                    If Not VerifyCondition(Character, Condition) Then
                        Return False
                    End If
                Else
                    If Not VerifySousConditions(Character, Condition) Then
                        Return False
                    End If
                End If

            Next

            Return True

        End Function

#End Region

    End Class
End Namespace