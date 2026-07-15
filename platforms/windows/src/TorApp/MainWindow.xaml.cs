using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace TorProxy;

public partial class MainWindow : Window
{
    /// <summary>Set true by the real-exit path so closing actually closes.</summary>
    public bool AllowClose { get; set; }

    /// <summary>Full-quit action, wired by the composition root (stop proxy + shutdown).</summary>
    public Action? ExitRequested { get; set; }

    /// <summary>Called after the window hides to the tray (show a balloon hint).</summary>
    public Action? HiddenToTray { get; set; }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Auto-scroll the log to the newest line. Deferred to Background priority
        // and guarded: a synchronous ScrollIntoView inside CollectionChanged forces
        // a layout pass while the item generator is mid-update and throws
        // "ItemsControl is inconsistent with its items source", which the dispatcher
        // would then retry in a tight CPU-spinning loop.
        viewModel.LogLines.CollectionChanged += (_, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    if (LogList.Items.Count > 0)
                        LogList.ScrollIntoView(LogList.Items[^1]);
                }
                catch (InvalidOperationException)
                {
                    // Generator/collection momentarily out of sync; the next append
                    // scrolls correctly. Never rethrow.
                }
            }));
        };
    }

    private void OnOpenFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            AppPaths.EnsureSeeded();
            Process.Start(new ProcessStartInfo
            {
                FileName = AppPaths.RootDirectory,
                UseShellExecute = true,
            });
        }
        catch
        {
            // Folder may be missing or Explorer unavailable; nothing to do.
        }
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        if (ExitRequested is not null)
            ExitRequested();
        else
        {
            AllowClose = true;
            Close();
        }
    }

    /// <summary>Closing the window hides it to the tray and keeps the proxy running,
    /// unless a real quit was requested via the tray/Exit button.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
            HiddenToTray?.Invoke();
        }
        base.OnClosing(e);
    }
}
