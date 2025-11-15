using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Services
{
    public class RateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _interval;

        private readonly Queue<DateTime> _log = new();

        public RateLimiter(int maxRequests, TimeSpan interval)
        {
            _maxRequests = maxRequests;
            _interval = interval;
        }

        public bool TryAcquire()
        {
            var now = DateTime.UtcNow;

            // Удаляем старые записи
            while (_log.Count > 0 && now - _log.Peek() > _interval)
                _log.Dequeue();

            if (_log.Count >= _maxRequests)
                return false;

            _log.Enqueue(now);
            return true;
        }
    }

}
