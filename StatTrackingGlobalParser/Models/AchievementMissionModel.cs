namespace StatTrackingGlobalParser.Models;

public class AchievementMissionModel
{
    #region Overrides

    public override string ToString()
    {
        return $"[AchievementName='{AchievementName}', MapName='{MapName}', MissionNo='{MissionNo}', MissionName='{MissionName}', Value='{Value}', ProgressInfo='{ProgressInfo}']";
    }

    #endregion

    #region Properties

    public string? AchievementName { get; set; }

    public string? MapName { get; set; }

    public int MissionNo { get; set; }

    public string? MissionName { get; set; }

    public bool Value { get; set; }

    public string? CompletionTime { get; set; }

    public string? ProgressInfo { get; set; }

    public string? AchievementToolTip { get; set; }

    public bool AchievementReached { get; set; }   

    #endregion
}