Imports System.Text
Imports Podemu.Utils.Basic

Namespace Game
    Public Class PricedItem : Inherits Item

        Public Price As UInteger



        Public Overrides Function ToString() As String

            Return UniqueID & ";" & Quantity & ";" & TemplateID & ";" & EffectsInfos() & ";" & Price

        End Function

        Shadows Function ToSaveString() As String

            Return DeciToHex(TemplateID) & "~" & DeciToHex(Quantity) & _
                "~" & Price & "~" & EffectsInfos() & "~" & Args

        End Function

        Shadows Function IsSuperposable(ByVal Item As PricedItem) As Boolean
            Return TemplateID = Item.TemplateID AndAlso EffectsInfos() = Item.EffectsInfos() AndAlso Args = Item.Args AndAlso Price = Item.Price
        End Function


    End Class
End Namespace