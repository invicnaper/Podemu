Namespace World
    Public Class MagicalForgeManager

        Public Shared Sub ChangeElement(ByVal Item As Game.Item, ByVal Client As Game.GameClient)
            Try
                'Gestion un peu bizzare mais qui fonctionne :)
                Dim WeaponEffect As Game.ItemEffect = Nothing
                WeaponEffect.EffectID = -1
                For Each MyEffect As Game.ItemEffect In Item.GetEffects
                    Client.SendNormalMessage("<b>EffectID</b> : " & MyEffect.EffectID)
                    If MyEffect.IsWeaponEffect() Then
                        WeaponEffect.EffectID = MyEffect.EffectID
                        Exit For
                    End If
                Next
                'If Not WeaponEffect.EffectID = -1 Then
                'Item.EffectList.Remove(WeaponEffect)
                'Item.EffectList.Add(MyNewEffect)
                'End If
            Catch ex As Exception
                Utils.MyConsole.Err(ex.ToString)
            End Try
        End Sub

    End Class
End Namespace
