namespace BKDisplay.Emulator;

using System;
using System.Threading;
using System.Threading.Tasks;

public partial class MainForm : Form, IDisplayClient
{
    private readonly MatrixPanel _panel;

    public MainForm()
    {
        InitializeComponent();

        _panel = new MatrixPanel { Dock = DockStyle.Fill, Width = 100, };
        Controls.Add(_panel);
    }

    public ReadOnlyMemory<Color> Colors
    {
        get => _panel.Colors;
        set => _panel.Colors = value;
    }

    public ValueTask<int> UpdateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_panel.InvokeRequired)
        {
            _panel.Invoke(() => _panel.Refresh());
            return default;
        }

        _panel.Refresh();
        return default;
    }
}
