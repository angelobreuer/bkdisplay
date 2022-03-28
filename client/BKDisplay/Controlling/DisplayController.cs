namespace BKDisplay.Controlling;

using System.Threading;

public sealed class DisplayController : DisplayControllerBase
{
    /// <inheritdoc/>
    protected override void Run(CancellationToken cancellationToken = default)
    {
        const double Scale = 50.0D;
        Buffer.AutoCommit = false;

        static double F(double x) => Math.Cos(x);

        // Plotschritt berechnen
        var step = 1.0D / Buffer.Width * Scale;
        var x = 0.0D;

        // Durch die einzelnen Werte iterieren, und das Minimum und Maximum bestimmen
        var min = double.MaxValue;
        var max = double.MinValue;

        var index = 0;
        for (; index < Buffer.Width; x += step, index++)
        {
            var value = F(x);
            min = Math.Min(value, min);
            max = Math.Max(value, max);
        }

        // Durch die einzelnen Werte iterieren, und die Funktion plotten
        index = 0;
        for (x = 0.0D; index < Buffer.Width; x += step, index++)
        {
            var newValue = (F(x) - min) * (Buffer.Height - 1) / (max - min);
            Buffer[index, (int)newValue] = Color.Red;
        }

        Buffer.Commit();

        // Animation starten
        for (var offset = x; ; offset += step)
        {
            Buffer.Clear();

            for (x = offset, index = 0; index < Buffer.Width; x += step, index++)
            {
                var value = F(x);
                min = Math.Min(value, min);
                max = Math.Max(value, max);

                var newValue = (value - min) * (Buffer.Height - 1) / (max - min);
                Buffer[index, (int)newValue] = Color.Red;
            }

            Buffer.Commit();
            Wait(0.01);
        }
    }
}
