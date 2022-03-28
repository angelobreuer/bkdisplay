namespace BKDisplay.Emulator;

public sealed class MatrixPanel : Panel
{
    private new const int Width = HostedDisplayService.Width;

    private bool _resized;

    public ReadOnlyMemory<Color> Colors { get; set; }

    /// <inheritdoc/>
    protected override void OnPaintBackground(PaintEventArgs eventArgs)
    {
        if (Colors.IsEmpty)
        {
            eventArgs.Graphics.Clear(System.Drawing.Color.White);
            return;
        }

        if (_resized)
        {
            eventArgs.Graphics.Clear(System.Drawing.Color.White);
            _resized = false;
        }

        using var bufferedBitmap = new Bitmap(eventArgs.ClipRectangle.Width, eventArgs.ClipRectangle.Height);
        using var bufferedGraphics = Graphics.FromImage(bufferedBitmap);

        var largestBoundSize = (float)Math.Min(
            val1: eventArgs.ClipRectangle.Width,
            val2: eventArgs.ClipRectangle.Height);

        largestBoundSize *= eventArgs.ClipRectangle.Width / eventArgs.ClipRectangle.Height;

        var boxLength = largestBoundSize / Width;

        var offset = new PointF(
            x: (eventArgs.ClipRectangle.Width - (Width * boxLength)) / 2,
            y: (eventArgs.ClipRectangle.Height - (Colors.Length / Width * boxLength)) / 2);

        for (var y = 0; y < Colors.Length / Width; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var color = Colors.Span[(y * Width) + x];
                var drawingColor = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

                using var solidBrush = new SolidBrush(drawingColor);

                bufferedGraphics.FillRectangle(
                    brush: solidBrush,
                    x: offset.X + (x * boxLength),
                    y: offset.Y + (y * boxLength),
                    width: boxLength,
                    height: boxLength);
            }
        }

        eventArgs.Graphics.DrawImage(bufferedBitmap, Point.Empty);
    }

    /// <inheritdoc/>
    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);

        _resized = true;
        Refresh();
    }
}
