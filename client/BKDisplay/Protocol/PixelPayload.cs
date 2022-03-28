namespace BKDisplay.Protocol;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BKDisplay;

public ref struct PixelPayload
{
    public ReadOnlySpan<Color> ColorTable { get; set; }

    public ReadOnlySpan<byte> Data { get; set; }

    /// <inheritdoc/>
    public bool TryReadBytes(ReadOnlySpan<byte> buffer, out int bytesRead)
    {
        if (buffer.Length < 2)
        {
            bytesRead = default;
            return false;
        }

        // read lengths
        var colorTableLength = buffer[0]; // * 3 (RGB)
        var dataLength = buffer[1];

        if (buffer.Length < 2 + (colorTableLength * 3) + dataLength)
        {
            bytesRead = default;
            return false;
        }

        var colorTableBuffer = buffer.Slice(2, colorTableLength * 3);
        var dataBuffer = buffer.Slice(2 + (colorTableLength * 3), dataLength);

        ColorTable = MemoryMarshal.CreateReadOnlySpan(
            reference: ref Unsafe.As<byte, Color>(ref MemoryMarshal.GetReference(colorTableBuffer)),
            length: colorTableLength);

        Data = dataBuffer;

        bytesRead = 2 + (colorTableLength * 3) + dataLength;
        return true;
    }

    /// <inheritdoc/>
    public bool TryWriteBytes(Span<byte> buffer, out int bytesWritten)
    {
        // write lengths
        buffer[0] = (byte)ColorTable.Length;
        buffer[1] = (byte)Data.Length;

        var colorTable = MemoryMarshal.AsBytes(ColorTable);
        colorTable.CopyTo(buffer[2..]);

        Data.CopyTo(buffer[(2 + colorTable.Length)..]);

        bytesWritten = 2 + (colorTable.Length * 3) + Data.Length;
        return true;
    }
}
