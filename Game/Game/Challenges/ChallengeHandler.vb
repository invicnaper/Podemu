﻿Imports Podemu.Game.Interactives
Imports MySql.Data.MySqlClient
Imports System.Reflection
Imports Podemu.Utils
Imports System.Linq

Namespace Game
    Public Class ChallengeHandler

        Public Shared TemplateList As New Dictionary(Of Integer, Type)()

#Region "Getter"

        Public Shared Function GetRandomChallenge(ByVal Fight As World.Fight) As Challenge
            Dim c = TemplateList.Select(Function(t) t.Value).ToArray()
            Dim rnd = Basic.Rand(0, c.Count - 1)
            Return Activator.CreateInstance(c(rnd), Fight)
        End Function

        Public Shared Function GetChallenge(ByVal Id As Integer, ByVal Fight As World.Fight) As Challenge
            If TemplateList(Id) IsNot Nothing Then
                Return Activator.CreateInstance(TemplateList(Id), Fight)
            End If
            Return Nothing
        End Function

#End Region

#Region "Loading"

        Public Shared Sub SetupChallenges()

            TemplateList.Add(1, GetType(Zombie))
            TemplateList.Add(2, GetType(Statue))
            TemplateList.Add(3, GetType(DesigneVolontaire))
            TemplateList.Add(4, GetType(Sursis))
            TemplateList.Add(5, GetType(Econome))
            TemplateList.Add(6, GetType(Versatile))
            TemplateList.Add(9, GetType(Barbare))

            TemplateList.Add(21, GetType(Circulez))
            TemplateList.Add(23, GetType(PerduDeVue))
            TemplateList.Add(33, GetType(Survivant))
            TemplateList.Add(36, GetType(Hardi))
            TemplateList.Add(37, GetType(Collant))
            TemplateList.Add(39, GetType(Anachorete))
            TemplateList.Add(41, GetType(Petulant))
            TemplateList.Add(43, GetType(Abnegation))
        End Sub

#End Region

    End Class
End Namespace