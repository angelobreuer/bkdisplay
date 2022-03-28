namespace BKDisplay.Protocol;

using System;

public ref struct StatusPayload
{
    public bool IsEnabled { get; set; }

    /// <inheritdoc/>
    public bool TryReadBytes(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        if (buffer.Length < 1)
        {
            bytesRead = default;
            return false;
        }

        bytesRead = 1;
        IsEnabled = buffer[0] is 1;
        return true;
    }

    /// <inheritdoc/>
    public bool TryWriteBytes(Span<byte> buffer, out int bytesWritten)
    {
        if (buffer.Length < 1)
        {
            bytesWritten = default;
            return false;
        }

        bytesWritten = 1;
        buffer[0] = (byte)(IsEnabled ? 1 : 0);
        return true;
    }
}
