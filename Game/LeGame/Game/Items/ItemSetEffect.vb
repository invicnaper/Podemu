﻿Imports Podemu.Utils.Basic

Namespace Game
    Public Class ItemSetEffect
        Inherits ItemEffect

        Public Overrides Function ToString() As String

            Dim ValueString As String = IIf(Value1 <= 0, "", DeciToHex(Value1))
            Dim Value2String As String = IIf(Value2 <= 0, "", DeciToHex(Value2))
            Return DeciToHex(EffectID) & "#" & ValueString & "#" & _
                Value2String

        End Function

    End Class
End Namespace
