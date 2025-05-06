using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatTrackingGlobalParser.Interfaces;
using StatTrackingGlobalParser.Models;
using StatTrackingGlobalParser.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace StatTrackingGlobalParser.ViewModels
{
    public partial class VmWindowMain : ObservableObject
    {
        #region Constructors

        public VmWindowMain()
        {
            _title = $"Commandos.StatTrackingGlobalParser ({Assembly.GetExecutingAssembly().GetName().Version})";
        }

        #endregion

        #region Fields

        private const string KeyCompletionTime = "CompletionTime"; // Map data start identifier
        private const string KeyNone = "None"; // Map data end identifier
        private const string KeyBoolProperty = "BoolProperty"; // General boolean property identifier

        private byte[]? _fileContent;
        private int _currentParserMap;
        private string? _currentParserMapName;

        private readonly int _hexValueLength = 14;
        private readonly int _valueSkip = 9;

        #endregion

        #region Properties

        [ObservableProperty]
        private string? _title;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ParseCommand))]
        private string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Commandos\\Saved\\SaveGames\\StatTrackingGlobal.sav");

        [ObservableProperty]
        private ObservableCollection<BoolPropertyModel>? _boolProperties = [];

        [ObservableProperty]
        private ObservableCollection<AchievementModel>? _achievements = [];

        [ObservableProperty]
        private ObservableCollection<MissionModel>? _missions = [];

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanParse))]
        private void Parse()
        {
            try
            {
                // Reset values
                _fileContent = null;
                _currentParserMap = 0;
                _currentParserMapName = null;

                // Clear collections
                BoolProperties?.Clear();
                Achievements?.Clear();
                Missions?.Clear();

                // Start parsing
                ReadFileContent();
                InterpretFileContent();

                // Populate
                PopulateAchievements();
                PopulateMissions();
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

        #region Methods

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
                ParseBoolProperties(First, Second);
            }
        }

        private void ParseBoolProperties(int startIndex, int endIndex)
        {
            if (_fileContent is null || _fileContent.Length == 0) { throw new InvalidDataException($"No file content present in file: {FilePath}"); }

            var fileContentArea = _fileContent.AsSpan(startIndex, endIndex - startIndex).ToArray();

            foreach (int index in ParseHelper.PatternAt(fileContentArea, Encoding.UTF8.GetBytes(KeyBoolProperty)))
            {
                var variableName = ParseHelper.ParseNameFromStartIndex(fileContentArea, index, false);
                var hexValue = BitConverter.ToString(fileContentArea.AsSpan(index + KeyBoolProperty.Length, _hexValueLength).ToArray()).Replace('-', '.');
                var value = fileContentArea.AsSpan(index + KeyBoolProperty.Length, _hexValueLength).ToArray().Skip(_valueSkip).Take(1).ToArray();

                var boolPropertyModel = new BoolPropertyModel()
                {
                    Map = _currentParserMap,
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
            foreach (var achievementNameAchievementToolTipMapping in KnownDataHelper.AchievementNameAchievementToolTipMappings)
            {
                foreach (var mapNameMissionNameMapping in KnownDataHelper.MapNameMissionNameMappings)
                {
                    // Special achievement only for a single mission
                    if (achievementNameAchievementToolTipMapping.Key == KnownDataHelper.AchievementNameShadowTactics &&
                        mapNameMissionNameMapping.Value != KnownDataHelper.MissionNameOperationReckoning)
                    {
                        continue;
                    }

                    var achievementModel = new AchievementModel
                    {
                        AchievementName = achievementNameAchievementToolTipMapping.Key,
                        AchievementToolTip = KnownDataHelper.GetAchievementNameTooltipForAchievementName(achievementNameAchievementToolTipMapping.Key),
                        MapName = mapNameMissionNameMapping.Key,
                        MissionName = mapNameMissionNameMapping.Value
                    };

                    EvaluateReachedAchievementValue(achievementModel);

                    Achievements?.Add(achievementModel);
                }
            }
        }

        private void PopulateMissions()
        {
            foreach (var mapNameMissionNameMapping in KnownDataHelper.MapNameMissionNameMappings)
            {
                foreach (var achievementNameAchievementToolTipMapping in KnownDataHelper.AchievementNameAchievementToolTipMappings)
                {
                    // Special achievement only for a single mission
                    if (achievementNameAchievementToolTipMapping.Key == KnownDataHelper.AchievementNameShadowTactics &&
                        mapNameMissionNameMapping.Value != KnownDataHelper.MissionNameOperationReckoning)
                    {
                        continue;
                    }

                    var missionModel = new MissionModel
                    {
                        MissionName = mapNameMissionNameMapping.Value,
                        MapName = mapNameMissionNameMapping.Key,
                        AchievementName = achievementNameAchievementToolTipMapping.Key,
                        AchievementToolTip = KnownDataHelper.GetAchievementNameTooltipForAchievementName(achievementNameAchievementToolTipMapping.Key)
                    };

                    EvaluateReachedAchievementValue(missionModel);

                    Missions?.Add(missionModel);
                }
            }
        }

        private void EvaluateReachedAchievementValue(IModelHelper modelHelper)
        {
            BoolPropertyModel? resultBoolPropertyModel;

            switch (modelHelper.GetAchievementName())
            {
                case KnownDataHelper.AchievementNameKnockThemOutWithKindness:
                    resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x => x.VariableName == KnownDataHelper.AchievementNameKnockThemOutWithKindnessVariableName &&
                                                                        x.MapName == modelHelper.GetMapName());

                    modelHelper.SetValue(resultBoolPropertyModel is not null && resultBoolPropertyModel.Value);
                    break;
                case KnownDataHelper.AchievementNameNoStoneLeftUnkilled:
                    resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x => x.VariableName == KnownDataHelper.AchievementNameNoStoneLeftUnkilledVariableName &&
                                                                        x.MapName == modelHelper.GetMapName());

                    modelHelper.SetValue(resultBoolPropertyModel is not null && resultBoolPropertyModel.Value);
                    break;
                case KnownDataHelper.AchievementNameShadowTactics:
                    resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x => x.VariableName == KnownDataHelper.AchievementNameShadowTacticsVariableName &&
                                                    x.MapName == modelHelper.GetMapName());

                    modelHelper.SetValue(resultBoolPropertyModel is not null && resultBoolPropertyModel.Value == false);
                    break;
                case KnownDataHelper.AchievementNameIfTheWorldWereAVillage:
                    resultBoolPropertyModel = BoolProperties?.FirstOrDefault(x => x.VariableName == KnownDataHelper.AchievementNameIfTheWorldWereAVillageVariableName &&
                                                    x.MapName == modelHelper.GetMapName());

                    modelHelper.SetValue(resultBoolPropertyModel is not null && resultBoolPropertyModel.Value == true);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
