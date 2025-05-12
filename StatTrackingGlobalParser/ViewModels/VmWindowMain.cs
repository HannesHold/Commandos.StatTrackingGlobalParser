using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatTrackingGlobalParser.Models;
using StatTrackingGlobalParser.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace StatTrackingGlobalParser.ViewModels;

public partial class VmWindowMain : ObservableObject
{
    #region Constructors

    public VmWindowMain()
    {
        Title = $"Commandos.StatTrackingGlobalParser ({Assembly.GetExecutingAssembly().GetName().Version})";
    }

    #endregion

    #region Fields

    private const string KeyCompletionTime = "CompletionTime"; // Map data start identifier
    private const string KeyNone = "None"; // Map data end identifier
    private const string KeyFloatProperty = "FloatProperty"; // General float property identifier
    private const string KeyBoolProperty = "BoolProperty"; // General boolean property identifier

    private byte[]? _fileContent;
    private int _currentParserMap;
    private string? _currentParserMapName;

    private readonly int _booleanPropertyHexValueLength = 14;
    private readonly int _booleanPropertyValueSkip = 9;

    private readonly int _floatPropertyHexValueLength = 14;
    private readonly int _floatPropertyValueSkip = 10;

    #endregion

    #region Properties

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ParseCommand))]
    private string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Commandos\\Saved\\SaveGames\\StatTrackingGlobal.sav");

    [ObservableProperty]
    private ObservableCollection<FloatPropertyModel>? _floatProperties = [];

    [ObservableProperty]
    private ICollectionView? _floatPropertiesView;

    [ObservableProperty]
    private ObservableCollection<BoolPropertyModel>? _boolProperties = [];

    [ObservableProperty]
    private ICollectionView? _boolPropertiesView;

    [ObservableProperty]
    private ObservableCollection<AchievementMissionModel>? _achievements = [];

    [ObservableProperty]
    private ICollectionView? _achievementsView;

    [ObservableProperty]
    private ObservableCollection<AchievementMissionModel>? _missions = [];

    [ObservableProperty]
    private ICollectionView? _missionsView;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearQuickFilterCommand))]
    private string? _quickFilter;

    #endregion

    #region Commands

    #region ParseCommand

    [RelayCommand(CanExecute = nameof(CanParse))]
    private void Parse()
    {
        try
        {
            // Reset values
            _fileContent = null;
            _currentParserMap = 0;
            _currentParserMapName = null;

            // Initialize collections
            InitializeCollections();

            // Start parsing
            ReadFileContent();
            InterpretFileContent();

            // Populate
            PopulateAchievements();
            PopulateMissions();

            // Update views with filter
            UpdateViewsWithFilter(QuickFilter);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
        }
    }

    private bool CanParse()
    {
        return File.Exists(FilePath);
    }

    #endregion

    #region ResetCommand

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        try
        {
            // Initialize collections
            InitializeCollections();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
        }
    }

    private static bool CanReset()
    {
        return true;
    }

    #endregion

    #region ClearQuickFilterCommand

    [RelayCommand(CanExecute = nameof(CanClearQuickFilter))]
    private void ClearQuickFilter()
    {
        try
        {
            QuickFilter = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
        }
    }

    private bool CanClearQuickFilter()
    {
        return !string.IsNullOrEmpty(QuickFilter);
    }

    #endregion

    #endregion

    #region Partial methods

    partial void OnQuickFilterChanged(string? value)
    {
        UpdateViewsWithFilter(value);
    }

    #endregion

    #region Methods

    private void InitializeCollections()
    {
        FloatProperties = [];
        FloatPropertiesView = CollectionViewSource.GetDefaultView(FloatProperties);
        BoolProperties = [];
        BoolPropertiesView = CollectionViewSource.GetDefaultView(BoolProperties);
        Achievements = [];
        AchievementsView = CollectionViewSource.GetDefaultView(Achievements);
        Missions = [];
        MissionsView = CollectionViewSource.GetDefaultView(Missions);
    }

    private void ReadFileContent()
    {
        _fileContent = File.ReadAllBytes(FilePath);
    }

    private void InterpretFileContent()
    {
        if (_fileContent is null || _fileContent.Length == 0) { throw new InvalidDataException($"No file content present in file: {FilePath}"); }

        foreach (var (First, Second) in ParseHelper.GetAreaOfInterest(_fileContent, KeyCompletionTime, KeyNone))
        {
            _currentParserMap++;
            _currentParserMapName = ParseHelper.ParseNameFromStartIndex(_fileContent, First, false);
            ParseFloatProperties(First, Second);
            ParseBoolProperties(First, Second);
        }
    }

    private void ParseFloatProperties(int startIndex, int endIndex)
    {
        if (_fileContent is null || _fileContent.Length == 0) { throw new InvalidDataException($"No file content present in file: {FilePath}"); }

        var fileContentArea = _fileContent.AsSpan(startIndex, endIndex - startIndex).ToArray();

        foreach (int index in ParseHelper.PatternAt(fileContentArea, Encoding.UTF8.GetBytes(KeyFloatProperty)))
        {
            var variableName = ParseHelper.ParseNameFromStartIndex(fileContentArea, index, false);
            var hexValue = BitConverter.ToString(fileContentArea.AsSpan(index + KeyFloatProperty.Length, _floatPropertyHexValueLength).ToArray()).Replace('-', '.');
            var value = BitConverter.ToSingle(fileContentArea.AsSpan(index + KeyFloatProperty.Length, _floatPropertyHexValueLength).ToArray().Skip(_floatPropertyValueSkip).Take(4).ToArray());

            var floatPropertyModel = new FloatPropertyModel()
            {
                MapNo = _currentParserMap,
                MapName = _currentParserMapName,
                StartIndex = index,
                VariableName = variableName,
                HexValue = hexValue,
                Value = value
            };

            FloatProperties?.Add(floatPropertyModel);
        }
    }

    private void ParseBoolProperties(int startIndex, int endIndex)
    {
        if (_fileContent is null || _fileContent.Length == 0) { throw new InvalidDataException($"No file content present in file: {FilePath}"); }

        var fileContentArea = _fileContent.AsSpan(startIndex, endIndex - startIndex).ToArray();

        foreach (int index in ParseHelper.PatternAt(fileContentArea, Encoding.UTF8.GetBytes(KeyBoolProperty)))
        {
            var variableName = ParseHelper.ParseNameFromStartIndex(fileContentArea, index, false);
            var hexValue = BitConverter.ToString(fileContentArea.AsSpan(index + KeyBoolProperty.Length, _booleanPropertyHexValueLength).ToArray()).Replace('-', '.');
            var value = fileContentArea.AsSpan(index + KeyBoolProperty.Length, _booleanPropertyHexValueLength).ToArray().Skip(_booleanPropertyValueSkip).Take(1).ToArray();

            var boolPropertyModel = new BoolPropertyModel()
            {
                MapNo = _currentParserMap,
                MapName = _currentParserMapName,
                StartIndex = index,
                VariableName = variableName,
                HexValue = hexValue,
                Value = value[0] != 0x00
            };

            BoolProperties?.Add(boolPropertyModel);
        }
    }

    private void PopulateAchievements()
    {
        var achievements = new List<AchievementMissionModel>();

        foreach (var achievementNameAchievementToolTipMapping in KnownDataHelper.AchievementNameAchievementToolTipMappings)
        {
            int missionNo = 0;

            foreach (var mapNameMissionNameMapping in KnownDataHelper.MapNameMissionNameMappings)
            {
                missionNo++;

                // Special achievement only for a single mission
                if (achievementNameAchievementToolTipMapping.Key == KnownDataHelper.AchievementNameShadowTactics &&
                    mapNameMissionNameMapping.Value != KnownDataHelper.MissionNameOperationReckoning)
                {
                    continue;
                }

                var achievementModel = new AchievementMissionModel
                {
                    AchievementName = achievementNameAchievementToolTipMapping.Key,
                    AchievementToolTip = KnownDataHelper.GetAchievementNameTooltipForAchievementName(achievementNameAchievementToolTipMapping.Key),
                    MapName = mapNameMissionNameMapping.Key,
                    MissionNo = missionNo,
                    MissionName = mapNameMissionNameMapping.Value
                };

                EvaluateReachedAchievementValue(achievementModel);
                EvaluateCompletionTime(achievementModel);

                achievements.Add(achievementModel);
            }
        }

        EvaluateReachedAchievements(achievements);

        Achievements = [.. achievements];
        AchievementsView = CollectionViewSource.GetDefaultView(Achievements);
    }

    private void PopulateMissions()
    {
        var missions = new List<AchievementMissionModel>();
        int missionNo = 0;

        foreach (var mapNameMissionNameMapping in KnownDataHelper.MapNameMissionNameMappings)
        {
            missionNo++;

            foreach (var achievementNameAchievementToolTipMapping in KnownDataHelper.AchievementNameAchievementToolTipMappings)
            {
                // Special achievement only for a single mission
                if (achievementNameAchievementToolTipMapping.Key == KnownDataHelper.AchievementNameShadowTactics &&
                    mapNameMissionNameMapping.Value != KnownDataHelper.MissionNameOperationReckoning)
                {
                    continue;
                }

                var achievementMissionModel = new AchievementMissionModel
                {
                    MissionNo = missionNo,
                    MissionName = mapNameMissionNameMapping.Value,
                    MapName = mapNameMissionNameMapping.Key,
                    AchievementName = achievementNameAchievementToolTipMapping.Key,
                    AchievementToolTip = KnownDataHelper.GetAchievementNameTooltipForAchievementName(achievementNameAchievementToolTipMapping.Key)
                };

                EvaluateReachedAchievementValue(achievementMissionModel);
                EvaluateCompletionTime(achievementMissionModel);

                missions.Add(achievementMissionModel);
            }
        }

        EvaluateReachedAchievements(missions);

        Missions = [.. missions];
        MissionsView = CollectionViewSource.GetDefaultView(Missions);
    }

    private void EvaluateReachedAchievementValue(AchievementMissionModel achievementMissionModel)
    {
        BoolPropertyModel? resultBoolPropertyModel;

        switch (achievementMissionModel.AchievementName)
        {
            case KnownDataHelper.AchievementNameIfTheWorldWereAVillage:
                resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x =>
                    x.VariableName == KnownDataHelper.AchievementNameIfTheWorldWereAVillageVariableName &&
                    x.MapName == achievementMissionModel.MapName);
                achievementMissionModel.Value = resultBoolPropertyModel is not null && resultBoolPropertyModel.Value == true;

                break;
            case KnownDataHelper.AchievementNameKnockThemOutWithKindness:
                resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x =>
                    x.VariableName == KnownDataHelper.AchievementNameKnockThemOutWithKindnessVariableName &&
                    x.MapName == achievementMissionModel.MapName);
                achievementMissionModel.Value = resultBoolPropertyModel is not null && resultBoolPropertyModel.Value;

                break;
            case KnownDataHelper.AchievementNameNoStoneLeftUnkilled:
                resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x =>
                    x.VariableName == KnownDataHelper.AchievementNameNoStoneLeftUnkilledVariableName &&
                    x.MapName == achievementMissionModel.MapName);
                achievementMissionModel.Value = resultBoolPropertyModel is not null && resultBoolPropertyModel.Value;

                break;
            case KnownDataHelper.AchievementNameShadowTactics:
                resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x =>
                    x.VariableName == KnownDataHelper.AchievementNameShadowTacticsVariableName &&
                    x.MapName == achievementMissionModel.MapName);
                achievementMissionModel.Value = resultBoolPropertyModel is not null && resultBoolPropertyModel.Value == false;

                break;
            default:
                break;
        }
    }

    private void EvaluateCompletionTime(AchievementMissionModel achievementMissionModel)
    {
        var completionTime = FloatProperties?.FirstOrDefault(x => x.MapName == achievementMissionModel.MapName && x.VariableName == KeyCompletionTime)?.Value;

        if (completionTime is null)
        {
            achievementMissionModel.CompletionTime = $"{TimeSpan.FromSeconds(0)}";
        }
        else
        {
            achievementMissionModel.CompletionTime = $"{TimeSpan.FromSeconds(Convert.ToDouble(completionTime))}";
        }
    }

    private static void EvaluateReachedAchievements(List<AchievementMissionModel> achievementMissionModels)
    {
        foreach (var achievementName in KnownDataHelper.AchievementNameAchievementToolTipMappings.Keys)
        {
            int reached = 0;
            string progressInfo = string.Empty;

            switch (achievementName)
            {
                case KnownDataHelper.AchievementNameIfTheWorldWereAVillage:
                    reached = achievementMissionModels.Count(x => x.AchievementName == KnownDataHelper.AchievementNameIfTheWorldWereAVillage && x.Value);
                    progressInfo = $"{reached}/{KnownDataHelper.MapNameMissionNameMappings.Count} ({Convert.ToDouble(reached) / KnownDataHelper.MapNameMissionNameMappings.Count * 100:0.00} %)";

                    if (reached == KnownDataHelper.MapNameMissionNameMappings.Count)
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameIfTheWorldWereAVillage).ToList().ForEach(x => { x.ProgressInfo = progressInfo; x.AchievementReached = true; });
                    }
                    else
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameIfTheWorldWereAVillage).ToList().ForEach(x => { x.ProgressInfo = progressInfo; });
                    }

                    break;
                case KnownDataHelper.AchievementNameKnockThemOutWithKindness:
                    reached = achievementMissionModels.Count(x => x.AchievementName == KnownDataHelper.AchievementNameKnockThemOutWithKindness && x.Value);
                    progressInfo = $"{reached}/{KnownDataHelper.MapNameMissionNameMappings.Count} ({Convert.ToDouble(reached) / KnownDataHelper.MapNameMissionNameMappings.Count * 100:0.00} %)";

                    if (reached == KnownDataHelper.MapNameMissionNameMappings.Count)
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameKnockThemOutWithKindness).ToList().ForEach(x => { x.ProgressInfo = progressInfo; x.AchievementReached = true; });
                    }
                    else
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameKnockThemOutWithKindness).ToList().ForEach(x => { x.ProgressInfo = progressInfo; });
                    }

                    break;
                case KnownDataHelper.AchievementNameNoStoneLeftUnkilled:
                    reached = achievementMissionModels.Count(x => x.AchievementName == KnownDataHelper.AchievementNameNoStoneLeftUnkilled && x.Value);
                    progressInfo = $"{reached}/{KnownDataHelper.MapNameMissionNameMappings.Count} ({Convert.ToDouble(reached) / KnownDataHelper.MapNameMissionNameMappings.Count * 100:0.00} %)";

                    if (reached == KnownDataHelper.MapNameMissionNameMappings.Count)
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameNoStoneLeftUnkilled).ToList().ForEach(x => { x.ProgressInfo = progressInfo; x.AchievementReached = true; });
                    }
                    else
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameNoStoneLeftUnkilled).ToList().ForEach(x => { x.ProgressInfo = progressInfo; });
                    }

                    break;
                case KnownDataHelper.AchievementNameShadowTactics:
                    reached = achievementMissionModels.Count(x => x.AchievementName == KnownDataHelper.AchievementNameShadowTactics && x.Value);
                    progressInfo = $"{reached}/1 ({Convert.ToDouble(reached) / 1 * 100:0.00} %)";

                    if (reached == 1)
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameShadowTactics).ToList().ForEach(x => { x.ProgressInfo = progressInfo; x.AchievementReached = true; });
                    }
                    else
                    {
                        achievementMissionModels.Where(x => x.AchievementName == KnownDataHelper.AchievementNameShadowTactics).ToList().ForEach(x => { x.ProgressInfo = progressInfo; });
                    }

                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateViewsWithFilter(string? value)
    {       
        if (FloatPropertiesView is not null)
        {
            FloatPropertiesView.Filter = o => string.IsNullOrEmpty(value) || $"{o}".Contains(value, StringComparison.CurrentCultureIgnoreCase);
            FloatPropertiesView.Refresh();
        }

        if (BoolPropertiesView is not null)
        {
            BoolPropertiesView.Filter = o => string.IsNullOrEmpty(value) || $"{o}".Contains(value, StringComparison.CurrentCultureIgnoreCase);
            BoolPropertiesView.Refresh();
        }

        if (AchievementsView is not null)
        {
            AchievementsView.Filter = o => string.IsNullOrEmpty(value) || $"{o}".Contains(value, StringComparison.CurrentCultureIgnoreCase);
            AchievementsView.Refresh();
        }

        if (MissionsView is not null)
        {
            MissionsView.Filter = o => string.IsNullOrEmpty(value) || $"{o}".Contains(value, StringComparison.CurrentCultureIgnoreCase);
            MissionsView.Refresh();
        }
    }

    #endregion
}