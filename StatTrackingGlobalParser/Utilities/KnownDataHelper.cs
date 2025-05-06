namespace StatTrackingGlobalParser.Utilities;

public static class KnownDataHelper
{
    #region Constants

    public const string NoDataFound = "No data found";

    public const string MapNameBritishCompound = "BritishCompound";
    public const string MapNameWeatherstation = "Weatherstation";
    public const string MapNameFortCapuzzo1 = "FortCapuzzo1";
    public const string MapNameTrainBridge = "TrainBridge";
    public const string MapNameLighthouse = "Lighthouse";
    public const string MapNameChannelIslands = "ChannelIslands";
    public const string MapNameGermanMine = "GermanMine";
    public const string MapNameLofoten = "Lofoten";
    public const string MapNameHarbour = "Harbour";
    public const string MapNameGermanAirfield = "GermanAirfield";
    public const string MapNameGermanCompound = "GermanCompound";
    public const string MapNameHeavyWater = "HeavyWater";
    public const string MapNameSondergeschutzDora = "SondergeschutzDora";
    public const string MapNameOccupiedCastle = "OccupiedCastle";

    public const string MissionNameOperationDaybreak = "Operation Daybreak";
    public const string MissionNameOperationFalchion = "Operation Falchion";
    public const string MissionNameOperationNightfall = "Operation Nightfall";
    public const string MissionNameOperationCleaver = "Operation Cleaver";
    public const string MissionNameOperationRapture = "Operation Rapture";
    public const string MissionNameOperationBlindfold = "Operation Blindfold";
    public const string MissionNameOperationMagpie = "Operation Magpie";
    public const string MissionNameOperationTrident = "Operation Trident";
    public const string MissionNameOperationAries = "Operation Aries";
    public const string MissionNameOperationHarbinger = "Operation Harbinger";
    public const string MissionNameOperationReunite = "Operation Reunite";
    public const string MissionNameOperationWaveBreaker = "Operation Wave Breaker";
    public const string MissionNameOperationGuardian = "Operation Guardian";
    public const string MissionNameOperationReckoning = "Operation Reckoning";

    public const string AchievementNameIfTheWorldWereAVillage = "If The World Were A Village";
    public const string AchievementNameKnockThemOutWithKindness = "Knock Them Out With Kindness";
    public const string AchievementNameNoStoneLeftUnkilled = "No Stone Left Unkilled";
    public const string AchievementNameShadowTactics = "Shadow Tactics";

    public const string AchievementNameIfTheWorldWereAVillageToolTip = "Set off a Global Alarm during every Operation at least once";
    public const string AchievementNameKnockThemOutWithKindnessToolTip = "Complete all Operations without killing any German Enemies";
    public const string AchievementNameNoStoneLeftUnkilledToolTip = "Kill 2369 German Enemies across all Operations";
    public const string AchievementNameShadowTacticsToolTip = "Complete 'Operation Reckoning' without raising an Alarm on Soldier or Veteran Difficulty";

    public const string AchievementNameIfTheWorldWereAVillageVariableName = "bWasGlobalAlarmSetOff";
    public const string AchievementNameKnockThemOutWithKindnessVariableName = "bWasNoEnemyKilled";
    public const string AchievementNameNoStoneLeftUnkilledVariableName = "bWereAllEnemiesKilled";
    public const string AchievementNameShadowTacticsVariableName = "bWasGlobalAlarmSetOff";   

    #endregion

    #region Properties

    public static readonly IReadOnlyDictionary<string, string> MapNameMissionNameMappings = new Dictionary<string, string>()
    {
        { MapNameBritishCompound, MissionNameOperationDaybreak },
        { MapNameWeatherstation, MissionNameOperationFalchion },
        { MapNameFortCapuzzo1, MissionNameOperationNightfall },
        { MapNameTrainBridge, MissionNameOperationCleaver },
        { MapNameLighthouse, MissionNameOperationRapture },
        { MapNameChannelIslands, MissionNameOperationBlindfold },
        { MapNameGermanMine, MissionNameOperationMagpie },
        { MapNameLofoten, MissionNameOperationTrident },
        { MapNameHarbour, MissionNameOperationAries },
        { MapNameGermanAirfield, MissionNameOperationHarbinger },
        { MapNameGermanCompound, MissionNameOperationReunite },
        { MapNameHeavyWater, MissionNameOperationWaveBreaker },
        { MapNameSondergeschutzDora, MissionNameOperationGuardian },
        { MapNameOccupiedCastle, MissionNameOperationReckoning }
    };

    public static readonly IReadOnlyDictionary<string, string> AchievementNameAchievementToolTipMappings = new Dictionary<string, string>()
    {
        { AchievementNameIfTheWorldWereAVillage, AchievementNameIfTheWorldWereAVillageToolTip },
        { AchievementNameKnockThemOutWithKindness, AchievementNameKnockThemOutWithKindnessToolTip },
        { AchievementNameNoStoneLeftUnkilled, AchievementNameNoStoneLeftUnkilledToolTip },
        { AchievementNameShadowTactics, AchievementNameShadowTacticsToolTip }       
    };

    #endregion

    #region Methods

    public static string GetMissionNameForMapName(string mapName)
    {
        if (MapNameMissionNameMappings.ContainsKey(mapName))
        {
            return MapNameMissionNameMappings[mapName];
        }

        return NoDataFound;
    }

    public static string GetAchievementNameTooltipForAchievementName(string achievementName)
    {
        if (AchievementNameAchievementToolTipMappings.ContainsKey(achievementName))
        {
            return AchievementNameAchievementToolTipMappings[achievementName];
        }

        return NoDataFound;
    }

    #endregion
}
