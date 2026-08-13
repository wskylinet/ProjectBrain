using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ProjectBrain.Api.Security;

public interface ILoginAttemptTracker
{
    bool IsBlocked(string userName);
    void RecordFailure(string userName);
    void Reset(string userName);
}

public sealed class LoginAttemptTracker : ILoginAttemptTracker
{
    private const int FailureLimit = 10;
    private const int MaxTrackedUsers = 10_000;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, AttemptState> _attempts = new();
    private int _operationCount;

    public bool IsBlocked(string userName)
    {
        CleanupPeriodically();
        if (!_attempts.TryGetValue(Key(userName), out var state)) return false;
        lock (state)
        {
            Prune(state, DateTimeOffset.UtcNow);
            return state.LockedUntil > DateTimeOffset.UtcNow;
        }
    }

    public void RecordFailure(string userName)
    {
        CleanupPeriodically();
        var key = Key(userName);
        if (_attempts.Count >= MaxTrackedUsers && !_attempts.ContainsKey(key)) return;

        var state = _attempts.GetOrAdd(key, _ => new AttemptState());
        lock (state)
        {
            var now = DateTimeOffset.UtcNow;
            Prune(state, now);
            state.Failures.Enqueue(now);
            if (state.Failures.Count >= FailureLimit) state.LockedUntil = now.Add(LockoutDuration);
        }
    }

    public void Reset(string userName) => _attempts.TryRemove(Key(userName), out _);

    private void CleanupPeriodically()
    {
        if (Interlocked.Increment(ref _operationCount) % 256 != 0) return;
        var now = DateTimeOffset.UtcNow;
        foreach (var item in _attempts)
        {
            lock (item.Value)
            {
                Prune(item.Value, now);
                if (item.Value.Failures.Count == 0 && item.Value.LockedUntil <= now)
                    _attempts.TryRemove(item.Key, out _);
            }
        }
    }

    private static void Prune(AttemptState state, DateTimeOffset now)
    {
        while (state.Failures.TryPeek(out var failure) && now - failure >= FailureWindow)
            state.Failures.Dequeue();
        if (state.LockedUntil <= now) state.LockedUntil = DateTimeOffset.MinValue;
    }

    private static string Key(string userName) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userName.Trim().ToUpperInvariant())));

    private sealed class AttemptState
    {
        public Queue<DateTimeOffset> Failures { get; } = new();
        public DateTimeOffset LockedUntil { get; set; }
    }
}
