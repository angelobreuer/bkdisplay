namespace BKDisplay;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BKDisplay.Protocol;
using Microsoft.Extensions.Options;

public sealed class BKClient : IDisplayClient
{
    private readonly Memory<byte> _buffer;
    private readonly Color[] _colorTable;
    private readonly IPEndPoint _endPoint;
    private readonly Socket _socket;

    public BKClient(IOptions<BKClientOptions> options)
    {
        _endPoint = new IPEndPoint(
            address: IPAddress.Parse(options.Value.Host),
            port: options.Value.Port);

        _buffer = GC.AllocateUninitializedArray<byte>(4096); // Should be far enough
        _colorTable = GC.AllocateUninitializedArray<Color>(1024);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(_endPoint);
    }

    public ReadOnlyMemory<Color> Colors { get; set; }

    public ValueTask<int> UpdateAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(Colors.Length <= 1024);

        var table = _colorTable.AsSpan();
        var tableIndex = 0;

        var colors = Colors.Span;
        Span<byte> colorIndexBuffer = stackalloc byte[colors.Length];

        for (var index = 0; index < colors.Length; index++)
        {
            var color = colors[index];
            var colorIndex = table[..tableIndex].IndexOf(color);

            if (colorIndex < 0)
            {
                colorIndex = tableIndex++;
                table[colorIndex] = color;
            }

            colorIndexBuffer[index] = (byte)colorIndex;
        }

        var payload = new PixelPayload
        {
            ColorTable = table[..tableIndex],
            Data = colorIndexBuffer,
        };

        _buffer.Span[0] = 1; // Operation Code
        var result = payload.TryWriteBytes(_buffer.Span[1..], out var bytesWritten);
        Debug.Assert(result);

        return SendAsync(bytesWritten + 1, cancellationToken);
    }

    private ValueTask<int> SendAsync(int payloadLength, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _socket.SendAsync(_buffer[..payloadLength], SocketFlags.None, cancellationToken);
    }

    private ValueTask<int> SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var payload = new StatusPayload { IsEnabled = enabled, };

        _buffer.Span[0] = 0; // Operation Code
        var result = payload.TryWriteBytes(_buffer.Span[1..], out var bytesWritten);
        Debug.Assert(result);

        return SendAsync(bytesWritten + 1, cancellationToken);
    }
}
