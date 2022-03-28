namespace BKDisplay.Controlling;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Threading;

public abstract class DisplayControllerBase : IDisplayController
{
    private CancellationToken _cancellationToken;
    private PixelBuffer? _buffer;

    protected PixelBuffer Buffer => _buffer!;

    /// <inheritdoc/>
    public void Run(PixelBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Interlocked.CompareExchange(ref _buffer, buffer, null) is not null)
        {
            throw new InvalidOperationException("The controller is busy.");
        }

        _cancellationToken = cancellationToken;

        try
        {
            Run(cancellationToken);
        }
        finally
        {
            _buffer = null;
        }
    }

    protected void Wait(double millisecondsDelay)
    {
        if (millisecondsDelay < 1)
        {
            // Period way too short to wait for
            var stopwatch = new Stopwatch();
            var ticks = (long)(TimeSpan.TicksPerMillisecond * millisecondsDelay);
            stopwatch.Restart();
            SpinWait.SpinUntil(() => stopwatch.ElapsedTicks >= ticks);
            return;
        }

        Task.Delay(
            delay: TimeSpan.FromMilliseconds(millisecondsDelay),
            cancellationToken: _cancellationToken).GetAwaiter().GetResult();
    }

    protected unsafe void Screenshot(int screenWidth, int screenHeight)
    {
#pragma warning disable CA1416 
        using var bitmap = new Bitmap(screenWidth, screenHeight);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(
            sourceX: 0,
            sourceY: 0,
            destinationX: 0,
            destinationY: 0,
            blockRegionSize: new Size(screenWidth, screenHeight));

        var resized = ResizePicture(bitmap, Buffer.Width, Buffer.Height);

        var bitmapData = resized.LockBits(
            rect: new Rectangle(0, 0, Buffer.Width, Buffer.Height),
            flags: ImageLockMode.ReadOnly,
            format: PixelFormat.Format24bppRgb);

        try
        {
            var source = new Span<BKDisplay.Color>((void*)bitmapData.Scan0, Buffer.Width * Buffer.Height);
            var destination = Buffer.AsSpan();

            Debug.Assert(bitmapData.Stride / Buffer.Width is 3);
            Debug.Assert(source.Length == destination.Length);

            // need to swap B-R
            for (var index = 0; index < source.Length; index++)
            {
                var sourceColor = source[index];
                destination[index] = new BKDisplay.Color(sourceColor.B, sourceColor.G, sourceColor.R);
            }
        }
        finally
        {
            resized.UnlockBits(bitmapData);
        }

#pragma warning restore CA1416
    }

    protected abstract void Run(CancellationToken cancellationToken = default);

    [SupportedOSPlatform("windows")]
    public static Bitmap ResizePicture(Bitmap sourceBitmap, int width, int height)
    {
        var destX = 0;
        var destY = 0;

        var horizontalRatio = width / (float)sourceBitmap.Width;
        var verticalRatio = height / (float)sourceBitmap.Height;

        float nPercent;
        if (verticalRatio < horizontalRatio)
        {
            nPercent = verticalRatio;
            destX = (int)((width - (sourceBitmap.Width * nPercent)) / 2);
        }
        else
        {
            nPercent = horizontalRatio;
            destY = (int)((height - (sourceBitmap.Height * nPercent)) / 2);
        }

        var destWidth = (int)(sourceBitmap.Width * nPercent);
        var destHeight = (int)(sourceBitmap.Height * nPercent);

        var destinationBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        destinationBitmap.SetResolution(sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

        using var graphics = Graphics.FromImage(destinationBitmap);
        graphics.Clear(Color.Black);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        graphics.DrawImage(sourceBitmap,
            destRect: new Rectangle(destX, destY, destWidth, destHeight),
            srcRect: new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
            srcUnit: GraphicsUnit.Pixel);

        return destinationBitmap;
    }
}
