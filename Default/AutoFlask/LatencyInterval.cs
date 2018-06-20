using System.Diagnostics;
using Loki.Bot;

namespace Default.AutoFlask
{
    public class LatencyInterval
    {
        private readonly Stopwatch _stopwatch;
        private readonly int _minDelay;

        private int _nextDelay;

        public LatencyInterval(int ms)
        {
            _stopwatch = Stopwatch.StartNew();
            _minDelay = ms;
            _nextDelay = ms;
        }

        public bool Elapsed
        {
            get
            {
                if (_stopwatch.ElapsedMilliseconds >= _nextDelay)
                {
                    _stopwatch.Restart();

                    var latency = LatencyTracker.Current;
                    if (latency > _minDelay)
                    {
                        _nextDelay = (int) (latency * 1.15f);
                    }
                    else
                    {
                        _nextDelay = _minDelay;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}