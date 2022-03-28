namespace BKDisplay;

using System;
using System.Text;

public sealed class PixelBuffer
{
    private readonly Color[] _buffer;

    public PixelBuffer(Color[] buffer, int width)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        Width = width;
    }

    public Span<Color> AsSpan() => _buffer;

    public bool AutoCommit { get; set; } = true;

    public int Height => _buffer.Length / Width;

    public bool IsDirty { get; private set; }

    public int Width { get; }

    public Color this[int x, int y]
    {
        get => _buffer[(y * Width) + x];

        set
        {
            _buffer[(y * Width) + x] = value;

            if (AutoCommit)
            {
                Commit();
            }
        }
    }

    public void Clear()
    {
        _buffer.AsSpan().Clear();

        if (AutoCommit)
        {
            Commit();
        }
    }

    public void ClearDirty() => IsDirty = false;

    public void Commit() => IsDirty = true;

    public void SetColumn(int x, Color color)
    {
        for (var y = 0; y < Height; y++)
        {
            this[x, y] = color;
        }

        if (AutoCommit)
        {
            Commit();
        }
    }

    public void SetRow(int y, Color color)
    {
        var span = _buffer.AsSpan(y * Width, Width);
        span.Fill(color);

        if (AutoCommit)
        {
            Commit();
        }
    }
    public override string ToString()
    {
        static char GetChar(Color color)
        {
            if (color.R + color.G + color.B > 255 * 2 / 4D * 3D)
            {
                return '@';
            }

            if (color.R > color.G && color.R > color.B)
            {
                return 'R';
            }

            if (color.G > color.R && color.G > color.B)
            {
                return 'G';
            }

            if (color.B > color.B && color.B > color.G)
            {
                return 'B';
            }

            return '#';
        }

        var stringBuilder = new StringBuilder((Width + 1) * Height);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                stringBuilder.Append(GetChar(this[x, y]));
            }

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}
