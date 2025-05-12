namespace StatTrackingGlobalParser.Models;

public class FloatPropertyModel
{
    #region Overrides

    public override string ToString()
    {
        return $"[MapNo='{MapNo}', MapName='{MapName}', StartIndex='{StartIndex}', VariableName='{VariableName}', HexValue='{HexValue}', Value='{Value}']";
    }

    #endregion

    #region Properties

    public int MapNo { get; set; }

    public string? MapName { get; set; }

    public int StartIndex { get; set; }

    public string? VariableName { get; set; }

    public string? HexValue { get; set; }

    public float Value { get; set; }

    #endregion
}