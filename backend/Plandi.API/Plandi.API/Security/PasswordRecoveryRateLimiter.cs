using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Plandi.API.Security;

public sealed class PasswordRecoveryRateLimiter
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private const int PermitLimit = 5;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _attempts = new();

    public bool TryAcquire(string remoteIp, string discriminator)
    {
        var rawKey = $"{remoteIp}|{discriminator.Trim().ToLowerInvariant()}";
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
        var attempts = _attempts.GetOrAdd(key, _ => new Queue<DateTime>());
        var now = DateTime.UtcNow;
        lock (attempts)
        {
            while (attempts.TryPeek(out var attempt) && now - attempt >= Window) attempts.Dequeue();
            if (attempts.Count >= PermitLimit) return false;
            attempts.Enqueue(now);
            return true;
        }
    }
}
