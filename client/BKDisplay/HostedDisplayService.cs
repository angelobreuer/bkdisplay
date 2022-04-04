namespace BKDisplay;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BKDisplay.Controlling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class HostedDisplayService : BackgroundService
{
    public const int FrameRate = 60;
    public const int Height = 11;
    public const int Width = 13;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IDisplayClient _client;
    private readonly IDisplayController _controller;
    private readonly Color[] _data;
    private readonly ILogger<HostedDisplayService> _logger;
    private readonly PixelBuffer _pixelBuffer;
    private readonly Thread _thread;
    public HostedDisplayService(IDisplayClient client, IDisplayController controller, ILogger<HostedDisplayService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _data = new Color[Width * Height];
        _pixelBuffer = new PixelBuffer(_data, Width);
        _thread = new Thread(Run);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client
            .UpdateAsync(stoppingToken)
            .ConfigureAwait(false);

        var stopwatch = new Stopwatch();

        var updates = 0U;
        var frames = 0U;

        stopwatch.Restart();
        _thread.Start();

        var waitDuration = (long)(1 / (double)FrameRate * TimeSpan.TicksPerSecond);
        while (!stoppingToken.IsCancellationRequested)
        {
            var elapsed = stopwatch.ElapsedTicks;
            if (_pixelBuffer.IsDirty)
            {
                await _client
                    .UpdateAsync(stoppingToken)
                    .ConfigureAwait(false);

                Interlocked.Increment(ref updates);
                _pixelBuffer.ClearDirty();
            }

            frames++;
            elapsed = stopwatch.ElapsedTicks - elapsed;

            if (stopwatch.ElapsedMilliseconds >= 1000)
            {
                var framesPerSecond = Math.Ceiling(frames / stopwatch.Elapsed.TotalSeconds);
                var updatesPerSecond = Math.Ceiling(updates / stopwatch.Elapsed.TotalSeconds);

                _logger.LogInformation("FPS: {FPS}, UPS: {UPS}, RTT: {RTT}.", framesPerSecond, updatesPerSecond, elapsed);

                var adjustment = (long)(Math.Abs(framesPerSecond - FrameRate) / FrameRate * TimeSpan.TicksPerMillisecond);
                if (framesPerSecond > FrameRate)
                {
                    waitDuration = Math.Min(waitDuration + adjustment, TimeSpan.TicksPerSecond);
                    Trace.WriteLine($"+ {adjustment / (double)TimeSpan.TicksPerMillisecond}ms, upd: {updates}, frames: {frames}, fps: {framesPerSecond}, elapsed: {stopwatch.Elapsed}");
                }
                else if (framesPerSecond < FrameRate && waitDuration >= adjustment)
                {
                    waitDuration = Math.Max(waitDuration - adjustment, TimeSpan.TicksPerMillisecond);
                    Trace.WriteLine($"- {adjustment / (double)TimeSpan.TicksPerMillisecond}ms, upd: {updates}, frames: {frames}, fps: {framesPerSecond}, elapsed: {stopwatch.Elapsed}");
                }

                stopwatch.Restart();
                frames = 0;
                updates = 0;
            }

            var ticksToWait = waitDuration - elapsed;

            if (ticksToWait > 0)
            {
                await Task
                    .Delay(TimeSpan.FromTicks(ticksToWait), stoppingToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private void Run()
    {
        var cancellationToken = _cancellationTokenSource.Token;

        try
        {
            _client.Colors = _data;
            _controller.Run(_pixelBuffer, cancellationToken);
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested && exception.CancellationToken == cancellationToken)
        {
        }
    }
}
