using StatTrackingGlobalParser.Interfaces;

namespace StatTrackingGlobalParser.Models;

public class BaseModel : IModelHelper
{
    #region IModelHelper

    public string? GetAchievementName()
    {
        return AchievementName;
    }

    public string? GetMapName()
    {
        return MapName;
    }

    public string? GetMissionName()
    {
        return MissionName;
    }

    public void SetValue(bool value)
    {
        Value = value;
    }

    #endregion

    #region Properties

    public string? AchievementName { get; set; }

    public string? MapName { get; set; }

    public string? MissionName { get; set; }

    public bool Value { get; set; }

    #endregion
}
