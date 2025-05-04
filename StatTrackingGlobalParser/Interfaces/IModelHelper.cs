namespace StatTrackingGlobalParser.Interfaces;

public interface IModelHelper
{
    public string? GetAchievementName();

    public string? GetMapName();

    public string? GetMissionName();

    public void SetValue(bool value);
}
