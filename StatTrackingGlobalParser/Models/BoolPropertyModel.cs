namespace StatTrackingGlobalParser.Models;

public class BoolPropertyModel
{
    #region Overrides

    public override string ToString()
    {
        return $"[Map='{Map}', MapName='{MapName}', StartIndex='{StartIndex}', VariableName='{VariableName}', HexValue='{HexValue}', Value='{Value}']";
    }

    #endregion

    #region Properties

    public int Map { get; set; }

    public string? MapName { get; set; }

    public int StartIndex { get; set; }

    public string? VariableName { get; set; }

    public string? HexValue { get; set; }

    public bool Value { get; set; }

    #endregion
}
