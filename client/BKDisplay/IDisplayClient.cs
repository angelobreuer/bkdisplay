namespace BKDisplay;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IDisplayClient
{
    ReadOnlyMemory<Color> Colors { get; set; }

    ValueTask<int> UpdateAsync(CancellationToken cancellationToken = default);
}