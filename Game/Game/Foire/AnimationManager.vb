Imports MySql.Data.MySqlClient
Imports Vemu_gs.Utils
Imports Vemu_gs.Utils.Basic
Imports Vemu_gs.World

Namespace Game
    Public Class AnimationManager

        Private Shared Animations As New Dictionary(Of Integer, Animation)

        Public Shared Sub LoadAnimations()


            MyConsole.StartLoading("Loading Animations (Foire du Trool) from database...")

            SyncLock Sql.OthersSync

                Animations.Clear()

                Try

                    Dim SQLText As String = "SELECT * FROM foire"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewAnim As New Animation

                        NewAnim.Id = Result("id")
                        NewAnim.IdsAnims = Result("ids_anims")
                        NewAnim.Name = Result("name")
                        NewAnim.Mapid = Result("mapid")
                        NewAnim.Cellid = Result("cellid")

                        If Not Result("gainobjet") Is Nothing Then
                            Dim d As String = Result("gainobjet")
                            Dim d2 As String() = d.Split(";")
                            For i As Integer = 0 To d2.Length - 1
                                NewAnim.GainObjects.Add(d2(i))
                            Next
                        End If
                        If Not Result("gainkamas") Is Nothing Then
                            NewAnim.GainKamas = Result("gainkamas")
                        Else
                            NewAnim.GainKamas = 0
                        End If

                        If Not Result("gainxp") Is Nothing Then
                            NewAnim.GainXp = Result("gainxp")
                        Else
                            NewAnim.GainXp = 0
                        End If

                        Try

                            Animations.Add(NewAnim.Id, NewAnim)
                        Catch ex As Exception
                            Utils.MyConsole.Err(ex.ToString, True)
                        End Try
                    End While

                    Result.Close()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't load animations : " & ex.Message, True)
                    Exit Sub
                End Try

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & Animations.Count & "@' Animations (Foire du Trool) loaded from database")

        End Sub

        Public Shared Function GetAnim(ByVal Id As Integer) As Animation
            Return Animations(Id)
        End Function

        Public Shared Function GetAnimbyMapid(ByVal mapid As Integer, ByVal Cellid As Integer)
            For Each Anim As Animation In Animations.Values
                If Anim.Mapid = mapid And Anim.Cellid = Cellid Then
                    Return Anim
                ElseIf Anim.Mapid = mapid And mapid = 6866 Then
                    Return mapid
                End If
            Next
            Return Nothing
        End Function

        Public Shared Sub Gain(ByVal Client As GameClient, ByVal Packet As String)

            Dim s1 As String() = Packet.Split("|")
            Dim idcell As Integer = s1(0)
            Dim idmap As Integer = Client.Character.MapId

            Dim Anim As Animation = GetAnimbyMapid(idmap, idcell)

            If Anim Is Nothing Then Exit Sub

            Select Case Anim.Id

                Case 1

                    Client.Character.Player.Kamas += Anim.GainKamas
                    If Not Anim.GainKamas = 0 Then
                        Client.SendNormalMessage("Vous avez gagne " & Anim.GainKamas & " kamas")
                    End If
                    Anim.GainKamas += 50
                    SaveAnim(Anim)
                Case 2
                    For Each S As String In Anim.GainObjects
                        Dim s2 As String() = S.Split(",")
                        For i As Integer = 1 To s2(1)
                            Admin.AddItem(Client, s2(0), 0)
                        Next
                        Client.SendNormalMessage("vous avez recu : " & s2(1) & " " & ItemsHandler.GetItemTemplate(s2(0)).Name)
                    Next
                Case 3

                    Client.Character.TeleportTo(3452, 195)
            End Select
            Client.Character.Save()
            Client.Character.SendAccountStats()
        End Sub

        Public Shared Sub Lost(ByVal Client As GameClient, ByVal Packet As String)

            Dim s1 As String() = Packet.Split("|")
            Dim idcell As Integer = s1(0)
            Dim idmap As Integer = Client.Character.MapId

            Dim Anim As Animation = GetAnimbyMapid(idmap, idcell)

            If Anim Is Nothing Then Exit Sub

            Select Case Anim.Id

                Case 1

                    Anim.GainKamas += 50
                    SaveAnim(Anim)

                Case 2

                Case 3
                    Client.Character.TeleportTo(3452, 195)

            End Select
            Client.Character.Save()
            Client.Character.SendAccountStats()

        End Sub

        Public Shared Sub SaveAnim(ByVal Anim As Animation)
            SyncLock Sql.OthersSync

                Dim UpdateString As String = "name=@name, ids_anims=@ids_anims, mapid=@mapid, cellid=@cellid, gainxp=@gainxp, gainkamas=@gainkamas"

                Dim SQLText As String = "UPDATE foire SET " & UpdateString & " WHERE id=@Id"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)
                Dim P As MySqlParameterCollection = SQLCommand.Parameters

                P.Add(New MySqlParameter("@Id", Anim.Id))
                P.Add(New MySqlParameter("@name", Anim.Name))
                P.Add(New MySqlParameter("@ids_anims", Anim.IdsAnims))
                P.Add(New MySqlParameter("@mapid", Anim.Mapid))
                P.Add(New MySqlParameter("@cellid", Anim.Cellid))
                P.Add(New MySqlParameter("@gainxp", Anim.GainXp))
                P.Add(New MySqlParameter("@gainkamas", Anim.GainKamas))


                SQLCommand.ExecuteNonQuery()

            End SyncLock
        End Sub
    End Class
End Namespace
