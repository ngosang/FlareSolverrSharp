using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FlareSolverrSharp.Utilities
{
    public class SemaphoreLocker
    {
        private static int _maxConcurrency = ResolveInitialConcurrency();
        private static bool _manuallySet = false;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);

        public static int MaxConcurrency
        {
            get
            {
                if (!_manuallySet)
                {
                    int currentFromConfig = ResolveInitialConcurrency();
                    if (currentFromConfig != _maxConcurrency && currentFromConfig > 0)
                    {
                        SetMaxConcurrencyInternal(currentFromConfig);
                    }
                }
                return _maxConcurrency;
            }
            set
            {
                _manuallySet = true;
                SetMaxConcurrencyInternal(value);
            }
        }

        public static int EstimatedMemoryMb => ConcurrencyAdvisor.EstimateMemoryMb(_maxConcurrency);
        public static string MemoryWarning => ConcurrencyAdvisor.GetMemoryWarning(_maxConcurrency);

        public static void SetMaxConcurrency(int concurrency)
        {
            _manuallySet = true;
            SetMaxConcurrencyInternal(concurrency);
        }

        private static void SetMaxConcurrencyInternal(int concurrency)
        {
            if (concurrency < 1)
                concurrency = 1;

            if (_maxConcurrency == concurrency)
                return;

            _maxConcurrency = concurrency;
            _semaphore = new SemaphoreSlim(concurrency, concurrency);

            var warning = ConcurrencyAdvisor.GetMemoryWarning(concurrency);
            if (!string.IsNullOrEmpty(warning))
            {
                Console.WriteLine(warning);
            }
        }

        public static void ResetToConfig()
        {
            _manuallySet = false;
            int configVal = ResolveInitialConcurrency();
            SetMaxConcurrencyInternal(configVal);
        }

        private static int ResolveInitialConcurrency()
        {
            try
            {
                // 1. Check environment variable
                var env = Environment.GetEnvironmentVariable("FLARESOLVERR_MAX_CONCURRENCY");
                if (!string.IsNullOrEmpty(env) && int.TryParse(env, out int val) && val > 0)
                    return val;

                // 2. Check Jackett ServerConfig.json
                var programData = Environment.GetEnvironmentVariable("ProgramData") ?? @"C:\ProgramData";
                var configPath = Path.Combine(programData, "Jackett", "ServerConfig.json");
                if (File.Exists(configPath))
                {
                    var content = File.ReadAllText(configPath);
                    var match = Regex.Match(content, @"""FlareSolverrMaxConcurrency""\s*:\s*(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int configVal) && configVal > 0)
                    {
                        return configVal;
                    }
                }
            }
            catch
            {
                // Fallback gracefully to default
            }

            return 5; // Default modernized parallel concurrency
        }

        public async Task LockAsync<T>(Func<T> worker)
            where T : Task
        {
            await _semaphore.WaitAsync();
            try
            {
                await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
