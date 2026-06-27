namespace MaskApp.Core.Features.MaskControl;

public sealed class MaskCommand
{
    private readonly byte[] plaintext;
    private readonly byte[] encryptedPayload;

    public MaskCommand(MaskCommandKind kind, string displayName, byte[] plaintext, byte[] encryptedPayload)
    {
        if (plaintext.Length != MaskBleProtocol.CommandLength)
        {
            throw new ArgumentException("Mask commands must be exactly 16 bytes.", nameof(plaintext));
        }

        if (encryptedPayload.Length != MaskBleProtocol.CommandLength)
        {
            throw new ArgumentException("Encrypted mask commands must be exactly 16 bytes.", nameof(encryptedPayload));
        }

        Kind = kind;
        DisplayName = displayName;
        this.plaintext = [.. plaintext];
        this.encryptedPayload = [.. encryptedPayload];
    }

    public MaskCommandKind Kind { get; }

    public string DisplayName { get; }

    public ReadOnlyMemory<byte> Plaintext => plaintext;

    public ReadOnlyMemory<byte> EncryptedPayload => encryptedPayload;
}
