namespace MaskApp.Core.Features.Text;

public sealed record TextSendLayoutMetadata(
    int ColumnCount,
    bool FixedWidth,
    bool Centered,
    bool VariableWidth,
    bool TwoLine,
    bool Shortened,
    string Description);
