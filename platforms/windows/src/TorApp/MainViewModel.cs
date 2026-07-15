using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DpiBypass.Tor;
using DpiBypass.Tor.Bridges;

namespace TorProxy;

/// <summary>
/// Drives the single window. Wraps <see cref="ITorController"/> and turns its
/// (English) status/log stream into friendly Russian guidance, including the
/// Snowflake "searching for a proxy" phase that otherwise looks frozen.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly ITorController _controller;
    private readonly string _bridgesPath;
    private readonly int _socksPort;
    private readonly DispatcherTimer _uptimeTimer;
    private const int MaxLogLines = 500;

    private TorStatus _status = TorStatus.Stopped;
    private int _percent;
    private bool _searching;
    private int _proxyAttempts;
    private DateTime? _readySince;

    private string _statusText = "Остановлено";
    private string _detailText = "Нажмите «Запустить». Если включён VPN — его лучше выключить.";
    private string _uptimeText = string.Empty;
    private string _checkResult = string.Empty;
    private string _bridgesStatus = string.Empty;
    private string _newBridge = string.Empty;
    private bool _autoStartEnabled;

    public MainViewModel(ITorController controller, string bridgesPath)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _bridgesPath = bridgesPath;
        _socksPort = ParsePort(controller.SocksEndpoint, 9150);
        _autoStartEnabled = AutostartRegistry.IsEnabled();

        _controller.StatusChanged += OnStatusChanged;
        _controller.LogReceived += OnLog;

        ToggleCommand = new AsyncRelayCommand(ToggleAsync);
        CopyEndpointCommand = new RelayCommand(_ => CopyEndpoint());
        CheckCommand = new AsyncRelayCommand(CheckAsync, () => IsReady);
        AddBridgeCommand = new RelayCommand(_ => AddBridge());
        RemoveBridgeCommand = new RelayCommand(p => RemoveBridge(p as string));
        SaveBridgesCommand = new RelayCommand(_ => SaveBridges());

        Bridges = new ObservableCollection<string>(LoadBridges(bridgesPath));

        _uptimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uptimeTimer.Tick += (_, _) => UpdateUptime();
    }

    public ObservableCollection<string> LogLines { get; } = new();
    public ObservableCollection<string> Bridges { get; }

    public AsyncRelayCommand ToggleCommand { get; }
    public RelayCommand CopyEndpointCommand { get; }
    public AsyncRelayCommand CheckCommand { get; }
    public RelayCommand AddBridgeCommand { get; }
    public RelayCommand RemoveBridgeCommand { get; }
    public RelayCommand SaveBridgesCommand { get; }

    public string EndpointText => _controller.SocksEndpoint;

    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public string DetailText { get => _detailText; private set => SetProperty(ref _detailText, value); }
    public string UptimeText { get => _uptimeText; private set => SetProperty(ref _uptimeText, value); }
    public string CheckResult { get => _checkResult; private set => SetProperty(ref _checkResult, value); }
    public string BridgesStatus { get => _bridgesStatus; private set => SetProperty(ref _bridgesStatus, value); }

    public string NewBridge { get => _newBridge; set => SetProperty(ref _newBridge, value); }

    public int Percent { get => _percent; private set => SetProperty(ref _percent, value); }

    /// <summary>Start with Windows (per-user Run key; no admin).</summary>
    public bool AutoStartEnabled
    {
        get => _autoStartEnabled;
        set
        {
            if (!SetProperty(ref _autoStartEnabled, value))
                return;
            try { AutostartRegistry.Set(value); }
            catch (Exception ex) { BridgesStatus = $"Не удалось изменить автозапуск: {ex.Message}"; }
        }
    }

    public bool IsRunning => _status is TorStatus.Starting or TorStatus.Bootstrapping or TorStatus.Ready;
    public bool IsReady => _status == TorStatus.Ready;
    public bool ShowUptime => _readySince is not null;
    public string ToggleLabel => IsRunning ? "Остановить" : "Запустить";

    public int StatusLevel => _status switch
    {
        TorStatus.Ready => 2,
        TorStatus.Faulted => 3,
        TorStatus.Starting or TorStatus.Bootstrapping or TorStatus.Stopping => 1,
        _ => 0,
    };

    private async Task ToggleAsync()
    {
        try
        {
            if (IsRunning)
                await Task.Run(() => _controller.StopAsync()).ConfigureAwait(true);
            else
                await Task.Run(() => _controller.StartAsync()).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            DetailText = $"Ошибка: {ex.Message}";
        }
    }

    private void CopyEndpoint()
    {
        try
        {
            Clipboard.SetText(EndpointText);
            DetailText = $"Адрес {EndpointText} скопирован. Вставьте его в настройки SOCKS5-прокси браузера.";
        }
        catch { /* clipboard can transiently fail */ }
    }

    private async Task CheckAsync()
    {
        CheckResult = "Проверяю соединение через Tor…";
        var result = await Task.Run(() => TorSelfTest.CheckAsync(_socksPort)).ConfigureAwait(true);
        CheckResult = result.Message;
    }

    // ---- Bridge editor ----

    private void AddBridge()
    {
        var value = NewBridge.Trim();
        if (value.Length == 0)
            return;
        if (!BridgeValidation.IsValidBridgeLine(value))
        {
            BridgesStatus = "Это не похоже на строку моста. Пример: snowflake … или obfs4 1.2.3.4:443 …";
            return;
        }
        if (Bridges.Contains(value))
        {
            BridgesStatus = "Такой мост уже есть в списке.";
            return;
        }
        Bridges.Add(value);
        NewBridge = string.Empty;
        BridgesStatus = "Мост добавлен. Нажмите «Сохранить», затем перезапустите прокси.";
    }

    private void RemoveBridge(string? bridge)
    {
        if (bridge is not null && Bridges.Remove(bridge))
            BridgesStatus = "Мост удалён. Нажмите «Сохранить», затем перезапустите прокси.";
    }

    private void SaveBridges()
    {
        try
        {
            var lines = new List<string>
            {
                "# Мосты Tor, по одному в строке. '#' — комментарий.",
                "# Больше мостов: https://bridges.torproject.org или Telegram @GetBridgesBot.",
                "",
            };
            lines.AddRange(Bridges);
            File.WriteAllLines(_bridgesPath, lines);
            BridgesStatus = "Сохранено. Перезапустите прокси (Остановить → Запустить), чтобы применить.";
        }
        catch (Exception ex)
        {
            BridgesStatus = $"Не удалось сохранить: {ex.Message}";
        }
    }

    private static IEnumerable<string> LoadBridges(string path)
    {
        if (!File.Exists(path))
            return Array.Empty<string>();
        return File.ReadLines(path)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .ToList();
    }

    // ---- Status/log plumbing ----

    private void OnStatusChanged(object? sender, TorStatusChangedEventArgs e) => RunOnUi(() =>
    {
        _status = e.Status;
        Percent = e.BootstrapPercent;

        if (e.Status != TorStatus.Bootstrapping || e.BootstrapPercent > 12)
            _searching = false;
        if (e.Status is TorStatus.Starting or TorStatus.Stopped or TorStatus.Faulted)
            _proxyAttempts = 0;

        // Uptime tracking starts at Ready.
        if (e.Status == TorStatus.Ready)
        {
            _readySince ??= DateTime.Now;
            _uptimeTimer.Start();
            UpdateUptime();
        }
        else
        {
            _uptimeTimer.Stop();
            _readySince = null;
            UptimeText = string.Empty;
            if (e.Status is TorStatus.Stopped or TorStatus.Faulted)
                CheckResult = string.Empty;
        }

        Recompute();
    });

    private void OnLog(object? sender, string line) => RunOnUi(() =>
    {
        AppendLog(line);
        if (_status is TorStatus.Bootstrapping or TorStatus.Starting &&
            line.Contains("trying a new proxy", StringComparison.OrdinalIgnoreCase))
        {
            _proxyAttempts++;
            _searching = true;
            Recompute();
        }
    });

    private void UpdateUptime()
    {
        if (_readySince is null)
            return;
        var span = DateTime.Now - _readySince.Value;
        UptimeText = $"Время работы: {(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
    }

    private void Recompute()
    {
        StatusText = _searching && _status == TorStatus.Bootstrapping
            ? "Поиск рабочего моста…"
            : _status switch
            {
                TorStatus.Stopped => "Остановлено",
                TorStatus.Starting => "Запуск…",
                TorStatus.Bootstrapping => $"Подключение… {Percent}%",
                TorStatus.Ready => "Готово — прокси работает",
                TorStatus.Stopping => "Остановка…",
                TorStatus.Faulted => "Ошибка",
                _ => _status.ToString(),
            };

        DetailText = _searching && _status == TorStatus.Bootstrapping
            ? $"Ищу рабочий узел Snowflake (попытка {_proxyAttempts})… Это нормально и обычно занимает 1–3 минуты. Просто подождите."
            : _status switch
            {
                TorStatus.Stopped => "Нажмите «Запустить». Если включён VPN — его лучше выключить.",
                TorStatus.Starting => "Запускаю Tor…",
                TorStatus.Bootstrapping => "Подключаюсь к сети Tor. Подождите, это может занять пару минут.",
                TorStatus.Ready => "Скопируйте адрес прокси ниже и укажите его в браузере (SOCKS5). Не закрывайте окно.",
                TorStatus.Stopping => "Останавливаю Tor…",
                TorStatus.Faulted => "Не удалось подключиться. Проверьте интернет и нажмите «Запустить» ещё раз.",
                _ => string.Empty,
            };

        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(ShowUptime));
        OnPropertyChanged(nameof(ToggleLabel));
        OnPropertyChanged(nameof(StatusLevel));
        CheckCommand.RaiseCanExecuteChanged();
    }

    private void AppendLog(string line)
    {
        LogLines.Add($"{DateTime.Now:HH:mm:ss}  {line}");
        while (LogLines.Count > MaxLogLines)
            LogLines.RemoveAt(0);
    }

    private static int ParsePort(string endpoint, int fallback)
    {
        int i = endpoint.LastIndexOf(':');
        return i >= 0 && int.TryParse(endpoint[(i + 1)..], out int p) ? p : fallback;
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
            dispatcher.BeginInvoke(action);
        else
            action();
    }
}
