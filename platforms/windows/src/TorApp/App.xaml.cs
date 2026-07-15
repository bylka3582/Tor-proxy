using System.IO;
using System.Threading;
using System.Windows;
using DpiBypass.Tor;

namespace TorProxy;

/// <summary>
/// Composition root: extract the bundled Tor binaries, wire the controller and
/// window, guard against a second instance and stray exceptions.
/// </summary>
public partial class App : Application
{
    private Mutex? _instanceMutex;
    private TorProcessController? _controller;
    private TrayManager? _tray;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _instanceMutex = new Mutex(true, @"Local\TorProxy_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Tor-proxy уже запущен. Проверьте открытое окно программы.",
                "Tor-proxy", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        InstallGlobalExceptionHandlers();

        try
        {
            AppPaths.EnsureSeeded();
            var bin = BinaryExtractor.EnsureBinaries(AppPaths.BinDirectory);

            _controller = new TorProcessController(new TorOptions
            {
                SocksPort = 9150,
                DataDirectory = AppPaths.DataDirectory,
                TorExecutablePath = bin.TorPath,
                Obfs4ProxyPath = bin.LyrebirdPath,
                BridgesPath = AppPaths.BridgesPath,
            });

            var viewModel = new MainViewModel(_controller, AppPaths.BridgesPath);
            var window = new MainWindow(viewModel);
            window.ExitRequested = ExitApp;

            _tray = new TrayManager(window, viewModel, ExitApp);
            window.HiddenToTray = () => _tray?.NotifyHidden();

            window.Show();
        }
        catch (Exception ex)
        {
            LogError(ex, "Startup");
            MessageBox.Show(
                "Не удалось запустить программу:\n\n" + ex.Message,
                "Tor-proxy", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    /// <summary>Full quit: stop the proxy, remove the tray icon, shut down.</summary>
    private void ExitApp()
    {
        if (_exiting)
            return;
        _exiting = true;

        foreach (Window w in Windows)
            if (w is MainWindow mw)
                mw.AllowClose = true;

        try { _controller?.StopAsync().Wait(TimeSpan.FromSeconds(6)); } catch { }
        _tray?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { _controller?.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(6)); } catch { }
        _tray?.Dispose();
        try { _instanceMutex?.ReleaseMutex(); } catch { }
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }

    private void InstallGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += (_, args) =>
        {
            LogError(args.Exception, "Dispatcher");
            args.Handled = true; // keep the window alive
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            LogError(args.ExceptionObject as Exception, "AppDomain");
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogError(args.Exception, "Task");
            args.SetObserved();
        };
    }

    private const long MaxErrorLogBytes = 2 * 1024 * 1024; // 2 MB hard cap
    private bool _inLogError;

    private void LogError(Exception? ex, string source)
    {
        if (ex is null || _inLogError)
            return;
        _inLogError = true;
        try
        {
            Directory.CreateDirectory(AppPaths.RootDirectory);
            string path = AppPaths.ErrorLogPath;
            if (!File.Exists(path) || new FileInfo(path).Length < MaxErrorLogBytes)
                File.AppendAllText(path, $"[{DateTimeOffset.Now:o}] ({source}) {ex}{Environment.NewLine}");
        }
        catch { /* logging must never throw */ }
        finally { _inLogError = false; }
    }
}
