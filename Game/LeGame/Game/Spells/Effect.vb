﻿Namespace Game
    Public Enum Effect

        None = -1

        Teleport = 4
        PushBack = 5
        PushFront = 6
        Transpose = 8

        VolPM = 77
        VolVie = 82
        VolPA = 84

        DamageLifeEau = 85
        DamageLifeTerre = 86
        DamageLifeAir = 87
        DamageLifeFeu = 88
        DamageLifeNeutre = 89
        VolEau = 91
        VolTerre = 92
        VolAir = 93
        VolFeu = 94
        VolNeutre = 95
        DamageEau = 96
        DamageTerre = 97
        DamageAir = 98
        DamageFeu = 99
        DamageNeutre = 100
        AddArmor = 105
        AddArmorBis = 265

        AddRenvoiDamage = 107
        Heal = 108
        DamageLanceur = 109
        AddLife = 110
        AddPA = 111
        AddDamage = 112
        MultiplyDamage = 114

        AddPABis = 120
        AddAgilite = 119
        AddChance = 123
        AddDamagePercent = 138
        AddDamageCritic = 115
        AddDamagePiege = 225
        AddDamagePiegePercent = 225
        AddDamagePhysic = 142
        AddDamageMagic = 143
        AddEchecCritic = 122
        AddEsquivePA = 160
        AddEsquivePM = 161
        AddForce = 118
        AddInitiative = 174
        AddIntelligence = 126
        AddInvocationMax = 182
        AddPM = 128
        AddPO = 117
        AddPods = 158
        AddProspection = 176
        AddSagesse = 124
        AddSoins = 178
        AddVitalite = 125
        SubAgilite = 154

        SubChance = 152
        SubDamage = 164
        SubDamageCritic = 171
        SubDamageMagic = 172
        SubDamagePhysic = 173
        SubEsquivePA = 162
        SubEsquivePM = 163
        SubForce = 157
        SubInitiative = 175
        SubIntelligence = 155
        SubPAEsquive = 101
        SubPMEsquive = 127
        SubPA = 168
        SubPM = 169
        SubPO = 116
        SubPods = 159
        SubProspection = 177
        SubSagesse = 156
        SubSoins = 179
        SubVitalite = 153

        Invocation = 181

        AddReduceDamagePhysic = 183
        AddReduceDamageMagic = 184

        AddReduceDamagePourcentEau = 211
        AddReduceDamagePourcentTerre = 210
        AddReduceDamagePourcentAir = 212
        AddReduceDamagePourcentFeu = 213
        AddReduceDamagePourcentNeutre = 214
        AddReduceDamagePourcentPvPEau = 251
        AddReduceDamagePourcentPvPTerre = 250
        AddReduceDamagePourcentPvPAir = 252
        AddReduceDamagePourcentPvPFeu = 253
        AddReduceDamagePourcentPvpNeutre = 254

        AddReduceDamageEau = 241
        AddReduceDamageTerre = 240
        AddReduceDamageAir = 242
        AddReduceDamageFeu = 243
        AddReduceDamageNeutre = 244
        AddReduceDamagePvPEau = 261
        AddReduceDamagePvPTerre = 260
        AddReduceDamagePvPAir = 262
        AddReduceDamagePvPFeu = 263
        AddReduceDamagePvPNeutre = 264

        SubReduceDamagePourcentEau = 216
        SubReduceDamagePourcentTerre = 215
        SubReduceDamagePourcentAir = 217
        SubReduceDamagePourcentFeu = 218
        SubReduceDamagePourcentNeutre = 219
        SubReduceDamagePourcentPvPEau = 255
        SubReduceDamagePourcentPvPTerre = 256
        SubReduceDamagePourcentPvPAir = 257
        SubReduceDamagePourcentPvPFeu = 258
        SubReduceDamagePourcentPvpNeutre = 259
        SubReduceDamageEau = 246
        SubReduceDamageTerre = 245
        SubReduceDamageAir = 247
        SubReduceDamageFeu = 248
        SubReduceDamageNeutre = 249

        Porter = 50
        Lancer = 51
        ChangeSkin = 149
        SpellBoost = 293
        UseTrap = 400
        UseGlyph = 401
        DoNothing = 666
        DamageLife = 672
        PushFear = 783
        AddChatiment = 788
        AddState = 950
        LostState = 951
        Invisible = 150
        DeleteAllBonus = 132

        AddSpell = 604
        AddCharactForce = 607
        AddCharactSagesse = 678
        AddCharactChance = 608
        AddCharactAgilite = 609
        AddCharactVitalite = 610
        AddCharactIntelligence = 611
        AddCharactPoint = 612
        AddSpellPoint = 613

        LastEat = 808

        MountOwner = 995

        LivingGfxId = 970
        LivingMood = 971
        LivingSkin = 972
        LivingType = 973
        LivingXp = 974

        CanBeExchange = 983

    End Enum

    Public Enum Charact

        Neutre
        Terre
        Feu
        Eau
        Air

    End Enum
End Namespace