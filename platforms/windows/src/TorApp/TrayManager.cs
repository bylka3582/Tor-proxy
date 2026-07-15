using System.ComponentModel;
using System.Drawing;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace TorProxy;

/// <summary>
/// System-tray icon: keeps the proxy running when the window is closed, and lets
/// the user show the window, toggle the proxy, or fully quit.
/// </summary>
public sealed class TrayManager : IDisposable
{
    private readonly MainWindow _window;
    private readonly MainViewModel _viewModel;
    private readonly Action _exit;

    private readonly WinForms.NotifyIcon _notifyIcon;
    private readonly WinForms.ToolStripMenuItem _toggleItem;

    public TrayManager(MainWindow window, MainViewModel viewModel, Action exit)
    {
        _window = window;
        _viewModel = viewModel;
        _exit = exit;

        var showItem = new WinForms.ToolStripMenuItem("Показать окно", null, (_, _) => ShowWindow());
        _toggleItem = new WinForms.ToolStripMenuItem("Запустить", null, (_, _) => _viewModel.ToggleCommand.Execute(null));
        var exitItem = new WinForms.ToolStripMenuItem("Выход", null, (_, _) => _exit());

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add(showItem);
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new WinForms.NotifyIcon
        {
            Icon = LoadIcon(),
            Text = "Tor-proxy",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _notifyIcon.DoubleClick += (_, _) => ShowWindow();

        _viewModel.PropertyChanged += OnViewModelChanged;
        RefreshFromStatus();
    }

    /// <summary>Show a short balloon when the window is hidden to the tray.</summary>
    public void NotifyHidden()
    {
        try
        {
            _notifyIcon.BalloonTipTitle = "Tor-proxy свёрнут";
            _notifyIcon.BalloonTipText = _viewModel.IsRunning
                ? "Прокси продолжает работать. Значок — в области уведомлений."
                : "Программа в области уведомлений. Прокси остановлен.";
            _notifyIcon.ShowBalloonTip(2500);
        }
        catch { /* balloons are best-effort */ }
    }

    private void ShowWindow()
    {
        _window.Show();
        if (_window.WindowState == WindowState.Minimized)
            _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.StatusText)
            or nameof(MainViewModel.IsRunning)
            or nameof(MainViewModel.ToggleLabel))
        {
            var d = Application.Current?.Dispatcher;
            if (d is not null && !d.CheckAccess())
                d.BeginInvoke(RefreshFromStatus);
            else
                RefreshFromStatus();
        }
    }

    private void RefreshFromStatus()
    {
        _toggleItem.Text = _viewModel.ToggleLabel;
        // NotifyIcon.Text is limited to 63 characters.
        string text = $"Tor-proxy — {_viewModel.StatusText}";
        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    private static Icon LoadIcon()
    {
        try
        {
            string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            var icon = Icon.ExtractAssociatedIcon(exe);
            if (icon is not null)
                return icon;
        }
        catch { /* fall through */ }
        return SystemIcons.Application;
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= OnViewModelChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
