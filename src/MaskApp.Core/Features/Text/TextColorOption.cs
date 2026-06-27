namespace MaskApp.Core.Features.Text;

public sealed record TextColorOption(string Name, byte Red, byte Green, byte Blue, string Hex)
{
    public TextLedColor ToLedColor() => new(Red, Green, Blue);
}
