namespace BKDisplay.Controlling;

public interface IDisplayController
{
    void Run(PixelBuffer buffer, CancellationToken cancellationToken = default);
}