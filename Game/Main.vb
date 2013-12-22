Imports Podemu.Game
Imports Podemu.Game.Actions
Imports Podemu.Utils

Module Main

    Public ServerId As Integer = 0

    Sub Main()
        Try
            Utils.Basic.StartTime = Environment.TickCount
            Utils.Config.LoadConfig()
            Utils.MyConsole.DrawTitle()
            Utils.Sql.OpenConnexion()
            If Config.GetItem("POPUP") = "Start" Then
                If MsgBox("Souhaitez-vous Fair un dons pour m'oncourager pour faire avancer pod emu?.Podemu est un emu gratuit avec plusieurs choses débug", 36, "Podemu - Dons") = MsgBoxResult.Yes Then
                    Process.Start("http://www.inivc-projet.c.la")
                End If
            End If
            If Config.GetItem("POPUP") = "poddons" Then
            End If
            If Config.GetItem("POPUP") = "" Then
                If MsgBox("Souhaitez-vous Fair un dons pour m'oncourager pour faire avancer pod emu?.Podemu est un emu gratuit avec plusieurs choses débug", 36, "Podemu - Dons") = MsgBoxResult.Yes Then
                    Process.Start("http://www.inivc-projet.c.la")
                End If
            End If
            Game.SpellsHandler.SetupSpells()
            Game.ZaapsHandler.LoadZaaps()
            Game.ZaapisManager.LoadZaapis()
            Game.Hdvs.loadhdvs()
            Game.NpcDialog.LoadQuestion()
            World.Podpconfig.loadConfig()
            Game.PrismManager.LoadPrism()
            World.DropsManager.Load()
            World.GuildHandler.LoadGuild()
            World.PerceptorManager.LoadPerceptor()
            World.MapsHandler.SetupMaps()
            Game.MonstersHandler.SetupMonsters()
            Game.MountsHandler.SetupMounts()
            Game.PaddockHandler.SetupPaddocks()
            World.SimpleManager.LoadSimpleC()
            Game.InteractiveHandler.SetupInteractives()
            Game.SentencesHandler.LoadSentences()
            ChallengeHandler.SetupChallenges()
            ActionHandler.SetupActions()
            Game.ItemsHandler.SetupItems()
            Game.GiftsHandler.LoadGifts()
            Game.NpcsHandler.SetupNpcs()
            Game.SubAreasHandler.SetupSubAreas()
            Game.QuestManager.LoadQuestObjectifs()
            Game.QuestManager.LoadQuestSteps()
            Game.DungeonsHandler.SetupDungeons()
            Game.ExperienceTable.SetupExperience()
            Game.StoreManager.LoadStore()
            Game.CharactersManager.LoadAll()
            Game.CharactersManager.LoadAll()
            World.Save.StartTimer()
            Server.GameServer.GetInstance()
            Server.UselessServer.GetInstance()
            Server.RealmLink.GetInstance()


            Utils.MyConsole.Status("Server loaded @successfully@ in '@" & (Environment.TickCount - Utils.Basic.StartTime) & "@ ms'")

            GC.Collect()
            GC.WaitForPendingFinalizers()

            Threading.Thread.Sleep(2500) ' Histoire de voir ce que l'emu ecris :)

            Console.Clear()
            Utils.MyConsole.DrawTitle()
            World.GameStats.SetupStats()

        Catch ex As Exception
            Utils.MyConsole.Err(ex.ToString, True)
        End Try

        Console.ReadLine()


    End Sub

End Module
