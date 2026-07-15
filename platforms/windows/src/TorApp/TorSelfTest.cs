using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace TorProxy;

/// <summary>
/// Confirms traffic really flows through Tor by asking check.torproject.org
/// (via the local SOCKS5 proxy) whether it sees a Tor exit.
/// </summary>
public static class TorSelfTest
{
    public sealed record CheckResult(bool Ok, string Message);

    public static async Task<CheckResult> CheckAsync(int socksPort, CancellationToken ct = default)
    {
        try
        {
            using var handler = new SocketsHttpHandler
            {
                // socks5 with a hostname destination = remote DNS (socks5h), so the
                // name is resolved by Tor, not locally.
                Proxy = new WebProxy($"socks5://127.0.0.1:{socksPort}"),
                UseProxy = true,
                ConnectTimeout = TimeSpan.FromSeconds(20),
            };
            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(35) };

            string json = await http.GetStringAsync("https://check.torproject.org/api/ip", ct)
                                    .ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            bool isTor = root.TryGetProperty("IsTor", out var t) && t.ValueKind == JsonValueKind.True;
            string ip = root.TryGetProperty("IP", out var ipEl) ? ipEl.GetString() ?? "?" : "?";

            return isTor
                ? new CheckResult(true, $"Проверка пройдена: трафик идёт через Tor. Внешний IP: {ip}")
                : new CheckResult(false, $"Соединение есть, но выход не опознан как Tor (IP: {ip}). Попробуйте ещё раз.");
        }
        catch (Exception ex)
        {
            return new CheckResult(false,
                "Проверка не удалась: прокси пока не отвечает. Дождитесь статуса «Готово» и попробуйте снова. " +
                $"({ex.GetType().Name})");
        }
    }
}
