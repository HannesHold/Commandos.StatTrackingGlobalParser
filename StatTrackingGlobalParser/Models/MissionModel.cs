namespace StatTrackingGlobalParser.Models;

public class MissionModel : BaseModel
{
    #region Overrides

    public override string ToString()
    {
        return $"[MissionName='{MissionName}', MapName='{MapName}', AchievementName='{AchievementName}', Value='{Value}']";
    }

    #endregion

    #region Properties

    public string? AchievementToolTip { get; set; }

    #endregion
}
