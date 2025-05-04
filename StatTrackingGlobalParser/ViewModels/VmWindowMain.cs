using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

                // Start parsing
                ReadFileContent();
                InterpretFileContent();
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

        #endregion

    }
}
