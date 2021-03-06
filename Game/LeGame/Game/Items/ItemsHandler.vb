﻿Imports Podemu.Game.Actions
Imports MySql.Data.MySqlClient
Imports Podemu.Utils
Imports Podemu.Utils.Basic

Namespace Game
    Public Class ItemsHandler

        Private Shared ItemsTemplates As New Dictionary(Of Integer, ItemTemplate)(10000)
        Private Shared SetsTemplates As New Dictionary(Of Integer, ItemSetTemplate)(200)

        Private Shared UniqueActualID As Integer = 0

        Public Shared Sub SetupItems()

            MyConsole.StartLoading("Loading items from database...")

            SyncLock Sql.OthersSync

                ItemsTemplates.Clear()

                Try

                    Dim SQLText As String = "SELECT * FROM items_data"
                    Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                    Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                    While Result.Read

                        Dim NewItem As New ItemTemplate

                        NewItem.ID = Result("ID")

                        NewItem.Name = Result("Name")
                        NewItem.Type = Result("Type")
                        NewItem.Level = Result("Level")
                        NewItem.Pods = Result("Weight")
                        NewItem.TwoHand = Result("TwoHands")
                        NewItem.Forgemageable = Result("Forgemageable")
                        NewItem.IsBuff = Result("IsBuff")
                        NewItem.Usable = Result("Usable")
                        NewItem.Targetable = Result("Targetable")
                        NewItem.Price = Result("Price")

                        Dim UseEffects = Result("UseEffects")
                        If UseEffects <> "" Then
                            NewItem.UseEffects = New SerializedActionList(UseEffects)
                        End If

                        Try
                            NewItem.Conditions = New ItemConditions(Result("Conditions"))

                            Dim WeaponInfos As String = Result("WeaponInfo")
                            If WeaponInfos <> "" Then
                                Dim WeaponData() As String = WeaponInfos.Split(",")
                                NewItem.BonusCC = WeaponData(0)
                                NewItem.CostPA = WeaponData(1)
                                NewItem.MinPO = WeaponData(2)
                                NewItem.MaxPO = WeaponData(3)
                                NewItem.TauxCC = WeaponData(4)
                                NewItem.TauxEC = WeaponData(5)
                            End If

                            Dim Effects() As String = Result("Stats").ToString.Split(",")

                            For Each Effect As String In Effects

                                If Effect = "" Then Continue For

                                Dim EffectData() As String = Effect.Split("#")

                                Dim NewEffect As New ItemEffect()

                                NewEffect.EffectID = HexToDeci(EffectData(0))
                                If EffectData.Length > 1 Then
                                    NewEffect.Value1 = HexToDeci(EffectData(1))
                                End If
                                If EffectData.Length > 2 Then
                                    NewEffect.Value2 = HexToDeci(EffectData(2))
                                End If
                                If EffectData.Length > 3 Then
                                    NewEffect.Value3 = HexToDeci(EffectData(3))
                                End If
                                If EffectData.Length > 4 Then
                                    NewEffect.EffectStr = EffectData(4)
                                End If

                                NewItem.EffectList.Add(NewEffect)

                            Next

                            ItemsTemplates.Add(NewItem.ID, NewItem)
                        Catch
                            Utils.MyConsole.Err(NewItem.ID, True)
                        End Try
                    End While

                    Result.Close()

                Catch ex As Exception
                    Utils.MyConsole.Err("Can't load items : " & ex.Message, True)
                    Exit Sub
                End Try

            End SyncLock

            MyConsole.StopLoading()
            MyConsole.Status("'@" & ItemsTemplates.Count & "@' items loaded from database")

            SetupItemSets()

        End Sub

        Public Shared Sub SetupItemSets()

            SyncLock Sql.OthersSync

                SetsTemplates.Clear()

                Dim SQLText As String = "SELECT * FROM items_pano"
                Dim SQLCommand As New MySqlCommand(SQLText, Sql.Others)

                Dim Result As MySqlDataReader = SQLCommand.ExecuteReader

                While Result.Read

                    Dim NewTemplate As New ItemSetTemplate

                    NewTemplate.Id = Result("id")
                    NewTemplate.Nom = Result("nom")

                    Dim ListItems As String = Result("items")
                    Dim ItemsArray() As String = ListItems.Split(",")
                    For Each ItemToAdd As Integer In ItemsArray
                        NewTemplate.ListItems.Add(ItemToAdd)
                        If ItemExist(ItemToAdd) Then
                            GetItemTemplate(ItemToAdd).Pano = NewTemplate.Id
                        End If
                    Next

                    For Num As Integer = 2 To 8
                        Dim Effects As String = Result("effects" & Num)
                        Dim NewEffectList As New List(Of ItemSetEffect)
                        If Effects <> "" Then
                            Dim EffectsArray() As String = Effects.Split(";")
                            For Each Effect As String In EffectsArray
                                Dim NewEffect As New ItemSetEffect
                                Dim EffectData() As String = Effect.Split(",")
                                NewEffect.EffectID = EffectData(0)
                                NewEffect.Value1 = EffectData(1)
                                NewEffectList.Add(NewEffect)
                            Next
                        End If
                        NewTemplate.EffectList.Add(Num, NewEffectList)
                    Next

                    SetsTemplates.Add(NewTemplate.Id, NewTemplate)

                End While

                Result.Close()

            End SyncLock

            MyConsole.Status("'@" & SetsTemplates.Count & "@' itemsets loaded from database")

        End Sub
      
        Public Shared Function GetRandomeItemsTemplatebytype(ByVal type As Integer, ByVal maxlvl As Integer) As List(Of ItemTemplate)
            Dim random As New Random
            Dim count As Integer = 0
            Dim listoftemplate As New List(Of ItemTemplate)
            Dim i As Integer = 0
            For i = 0 To 11000
                If ItemExist(i) Then
                    If ItemsTemplates.Item(i).Type = type And ItemsTemplates.Item(i).Level <= maxlvl Then
                        listoftemplate.Add(ItemsTemplates(i))
                    End If
                End If
            Next i
            count = count + 1

            Return listoftemplate
        End Function

        Public Shared ReadOnly Property GetUniqueID() As Integer
            Get
                If UniqueActualID = 0 Then UniqueActualID = 1000
                UniqueActualID += 1
                Return UniqueActualID
            End Get
        End Property

        Public Shared Function ItemExist(ByVal ID As Integer) As Boolean
            Return ItemsTemplates.ContainsKey(ID)
        End Function

        Public Shared Function GetIdTemplate(ByVal ID As Integer) As Integer

        End Function

        Public Shared Function GetItemTemplate(ByVal ID As Integer) As ItemTemplate
            Return ItemsTemplates(ID)
        End Function

        Public Shared Function SetExist(ByVal ID As Integer) As Boolean
            Return SetsTemplates.ContainsKey(ID)
        End Function

        Public Shared Function GetSetTemplate(ByVal ID As Integer) As ItemSetTemplate
            Return SetsTemplates(ID)
        End Function

        Public Enum Positions

            NONE = -1
            AMULETTE = 0
            ARME = 1
            ANNEAU1 = 2
            CEINTURE = 3
            ANNEAU2 = 4
            BOTTES = 5
            COIFFE = 6
            CAPE = 7
            FAMILIER = 8
            DOFUS1 = 9
            DOFUS2 = 10
            DOFUS3 = 11
            DOFUS4 = 12
            DOFUS5 = 13
            DOFUS6 = 14
            BOUCLIER = 15
            MOUNT = 16

            BAR1 = 23
            BAR2 = 24
            BAR3 = 25
            BAR4 = 26
            BAR5 = 27
            BAR6 = 28
            BAR7 = 29
            BAR8 = 30
            BAR9 = 31
            BAR10 = 32
            BAR11 = 33
            BAR12 = 34
            BAR13 = 35
            BAR14 = 36

        End Enum

        Public Enum Types

            AMULETTE = 1
            ARC = 2
            BAGUETTE = 3
            BATON = 4
            DAGUES = 5
            EPEE = 6
            MARTEAU = 7
            PELLE = 8
            ANNEAU = 9
            CEINTURE = 10
            BOTTES = 11
            POTION = 12
            PARCHO_EXP = 13
            DONS = 14
            RESSOURCE = 15
            COIFFE = 16
            CAPE = 17
            FAMILIER = 18
            HACHE = 19
            OUTIL = 20
            PIOCHE = 21
            FAUX = 22
            DOFUS = 23
            QUETES = 24
            DOCUMENT = 25
            FM_POTION = 26
            TRANSFORM = 27
            BOOST_FOOD = 28
            BENEDICTION = 29
            MALEDICTION = 30
            RP_BUFF = 31
            PERSO_SUIVEUR = 32
            PAIN = 33
            CEREALE = 34
            FLEUR = 35
            PLANTE = 36
            BIERE = 37
            BOIS = 38
            MINERAIS = 39
            ALLIAGE = 40
            POISSON = 41
            BONBON = 42
            POTION_OUBLIE = 43
            POTION_METIER = 44
            POTION_SORT = 45
            FRUIT = 46
            OS = 47
            POUDRE = 48
            COMESTI_POISSON = 49
            PIERRE_PRECIEUSE = 50
            PIERRE_BRUTE = 51
            FARINE = 52
            PLUME = 53
            POIL = 54
            ETOFFE = 55
            CUIR = 56
            LAINE = 57
            GRAINE = 58
            PEAU = 59
            HUILE = 60
            PELUCHE = 61
            POISSON_VIDE = 62
            VIANDE = 63
            VIANDE_CONSERVEE = 64
            QUEUE = 65
            METARIA = 66
            LEGUME = 68
            VIANDE_COMESTIBLE = 69
            TEINTURE = 70
            EQUIP_ALCHIMIE = 71
            OEUF_FAMILIER = 72
            MAITRISE = 73
            FEE_ARTIFICE = 74
            PARCHEMIN_SORT = 75
            PARCHEMIN_CARAC = 76
            CERTIFICAT_CHANIL = 77
            RUNE_FORGEMAGIE = 78
            BOISSON = 79
            OBJET_MISSION = 80
            SAC_DOS = 81
            BOUCLIER = 82
            PIERRE_AME = 83
            CLEFS = 84
            PIERRE_AME_PLEINE = 85
            POPO_OUBLI_PERCEP = 86
            PARCHO_RECHERCHE = 87
            PIERRE_MAGIQUE = 88
            CADEAUX = 89
            FANTOME_FAMILIER = 90
            DRAGODINDE = 91
            BOUFTOU = 92
            OBJET_ELEVAGE = 93
            OBJET_UTILISABLE = 94
            PLANCHE = 95
            ECORCE = 96
            CERTIF_MONTURE = 97
            RACINE = 98
            FILET_CAPTURE = 99
            SAC_RESSOURCE = 100
            ARBALETE = 102
            PATTE = 103
            AILE = 104
            OEUF = 105
            OREILLE = 106
            CARAPACE = 107
            BOURGEON = 108
            OEIL = 109
            GELEE = 110
            COQUILLE = 111
            PRISME = 112
            OBJET_VIVANT = 113
            ARME_MAGIQUE = 114
            FRAGM_AME_SHUSHU = 115
            POTION_FAMILIER = 116

        End Enum

        Public Shared Function TestPosition(ByVal ItemType As Integer, ByVal Usable As Boolean, ByVal Position As Integer) As Boolean

            Select Case Position

                Case Positions.NONE
                    Return True

                Case Positions.ARME
                    Return (ItemType = Types.ARC) Or _
                        (ItemType = Types.BAGUETTE) Or _
                        (ItemType = Types.BATON) Or _
                        (ItemType = Types.DAGUES) Or _
                        (ItemType = Types.EPEE) Or _
                        (ItemType = Types.MARTEAU) Or _
                        (ItemType = Types.PELLE) Or _
                        (ItemType = Types.HACHE) Or _
                        (ItemType = Types.OUTIL) Or _
                        (ItemType = Types.PIOCHE) Or _
                        (ItemType = Types.FAUX)

                Case Positions.ANNEAU1, _
                    Positions.ANNEAU2
                    Return (ItemType = Types.ANNEAU)

                Case Positions.CEINTURE
                    Return (ItemType = Types.CEINTURE)

                Case Positions.BOTTES
                    Return (ItemType = Types.BOTTES)

                Case Positions.COIFFE
                    Return (ItemType = Types.COIFFE)

                Case Positions.CAPE
                    Return (ItemType = Types.CAPE)

                Case Positions.FAMILIER
                    Return (ItemType = Types.FAMILIER)

                Case Positions.DOFUS1, _
                    Positions.DOFUS2, _
                    Positions.DOFUS3, _
                    Positions.DOFUS4, _
                    Positions.DOFUS5, _
                    Positions.DOFUS6
                    Return (ItemType = Types.DOFUS)

                Case Positions.BOUCLIER
                    Return (ItemType = Types.BOUCLIER)

                Case Positions.AMULETTE
                    Return (ItemType = Types.AMULETTE)

                Case Positions.BAR1, Positions.BAR2, _
                    Positions.BAR3, Positions.BAR4, _
                    Positions.BAR5, Positions.BAR6, _
                    Positions.BAR7, Positions.BAR8, _
                    Positions.BAR9, Positions.BAR10, _
                    Positions.BAR11, Positions.BAR12, _
                    Positions.BAR13, Positions.BAR14
                    Return Usable

            End Select

            Return False

        End Function

    End Class
End Namespace