namespace StatTrackingGlobalParser.Models;

public class AchievementModel : BaseModel
{
    #region Overrides

    public override string ToString()
    {
        return $"[AchievementName='{AchievementName}', MapName='{MapName}', MissionName='{MissionName}', Value='{Value}']";
    }

    #endregion

    #region Properties

    public string? AchievementToolTip { get; set; }

    #endregion
}
