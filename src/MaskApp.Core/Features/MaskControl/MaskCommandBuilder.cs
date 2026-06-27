using System.Text;

namespace MaskApp.Core.Features.MaskControl;

public static class MaskCommandBuilder
{
    public static MaskCommand Brightness(int brightness)
    {
        var clampedBrightness = Math.Clamp(brightness, 1, 100);
        return Build(MaskCommandKind.Brightness, $"Brightness {clampedBrightness}%", "LIGHT", (byte)clampedBrightness);
    }

    public static MaskCommand Animation(int presetId, string? displayName = null)
    {
        var preset = checked((byte)presetId);
        return Build(MaskCommandKind.Animation, displayName ?? $"Animation {preset}", "ANIM", preset);
    }

    public static MaskCommand Image(int presetId, string? displayName = null)
    {
        var preset = checked((byte)presetId);
        return Build(MaskCommandKind.Image, displayName ?? $"Image {preset}", "IMAG", preset);
    }

    public static MaskCommand TextMode(int mode)
    {
        var clampedMode = Math.Clamp(mode, 1, 4);
        return Build(MaskCommandKind.TextMode, $"Text mode {clampedMode}", "MODE", (byte)clampedMode);
    }

    public static MaskCommand TextSpeed(int speed)
    {
        var clampedSpeed = Math.Clamp(speed, 1, 100);
        return Build(MaskCommandKind.TextSpeed, $"Text speed {clampedSpeed}", "SPEED", (byte)clampedSpeed);
    }

    private static MaskCommand Build(MaskCommandKind kind, string displayName, string commandName, params byte[] arguments)
    {
        var commandBytes = Encoding.ASCII.GetBytes(commandName);
        var plaintext = new byte[MaskBleProtocol.CommandLength];
        plaintext[0] = checked((byte)(commandBytes.Length + arguments.Length));
        commandBytes.CopyTo(plaintext, 1);
        arguments.CopyTo(plaintext, 1 + commandBytes.Length);

        return new MaskCommand(kind, displayName, plaintext, MaskProtocolCrypto.EncryptBlock(plaintext));
    }
}
