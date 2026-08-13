using System.Collections.Concurrent;
using ProjectBrain.Api.Entities;
using ProjectBrain.Api.Services;

namespace ProjectBrain.Api.Security;

public sealed class RateLimitAuditRecorder
{
    private const int MaxTrackedWindows = 10_000;
    private readonly ConcurrentDictionary<string, byte> _recordedWindows = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RateLimitAuditRecorder> _logger;
    private int _operationCount;

    public RateLimitAuditRecorder(IServiceScopeFactory scopeFactory, ILogger<RateLimitAuditRecorder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task RecordAsync(HttpContext http, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var remoteIp = http.Connection.RemoteIpAddress;
        if (remoteIp?.IsIPv4MappedToIPv6 == true) remoteIp = remoteIp.MapToIPv4();
        var ipAddress = remoteIp?.ToString() ?? "unknown";
        var minute = now.ToUnixTimeSeconds() / 60;
        var key = $"{ipAddress}|{minute}";

        CleanupPeriodically(minute);
        if (_recordedWindows.Count >= MaxTrackedWindows || !_recordedWindows.TryAdd(key, 0)) return;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            await auditLogService.WriteAsync(new SysAuditLog
            {
                Action = "Login",
                Module = "Auth",
                Description = "登录 IP 请求过于频繁（同一 IP 每分钟仅记录一次）",
                EventCode = SecurityEventCodes.IpRateLimited,
                HttpMethod = http.Request.Method,
                RequestPath = http.Request.Path,
                IpAddress = ipAddress,
                UserAgent = Truncate(http.Request.Headers.UserAgent.ToString(), 500),
                IsSuccess = false,
                StatusCode = StatusCodes.Status429TooManyRequests,
                CreateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist rate-limit audit log for {IpAddress}", ipAddress);
        }
    }

    private void CleanupPeriodically(long currentMinute)
    {
        if (Interlocked.Increment(ref _operationCount) % 256 != 0) return;
        foreach (var key in _recordedWindows.Keys)
        {
            var separator = key.LastIndexOf('|');
            if (separator >= 0 && long.TryParse(key[(separator + 1)..], out var minute) && minute < currentMinute - 1)
                _recordedWindows.TryRemove(key, out _);
        }
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
}
