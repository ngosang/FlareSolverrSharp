using System;

namespace FlareSolverrSharp.Utilities
{
    public static class ConcurrencyAdvisor
    {
        public const int EstimatedMbPerSession = 250;
        public const int HighConcurrencyThreshold = 8;

        /// <summary>
        /// Estimates the RAM consumption in megabytes for the given number of concurrent browser sessions.
        /// </summary>
        public static int EstimateMemoryMb(int concurrency)
        {
            if (concurrency < 1)
                concurrency = 1;
            return concurrency * EstimatedMbPerSession;
        }

        /// <summary>
        /// Returns an alert if the configured concurrency exceeds the high concurrency threshold.
        /// </summary>
        public static string GetMemoryWarning(int concurrency)
        {
            if (concurrency >= HighConcurrencyThreshold)
            {
                int estMb = EstimateMemoryMb(concurrency);
                double estGb = estMb / 1024.0;
                return $"[ALERTA DE MEMÓRIA] Configuração de alta concorrência detectada ({concurrency} sessões simultâneas). " +
                       $"Estimativa de consumo de RAM: ~{estMb} MB (~{estGb:F1} GB). " +
                       $"Certifique-se de que seu sistema possui memória física livre suficiente para evitar travamentos ou swap de disco.";
            }
            return null;
        }

        /// <summary>
        /// Compares Jackett concurrency (X) with Byparr concurrency (Y) and reports any bottleneck/divergence.
        /// </summary>
        public static string EvaluateDivergence(int jackettConcurrency, int byparrConcurrency)
        {
            if (jackettConcurrency <= 0 || byparrConcurrency <= 0)
                return null;

            if (jackettConcurrency < byparrConcurrency)
            {
                return $"Aviso de Divergência de Concorrência: O sistema está sendo limitado pelo Jackett " +
                       $"({jackettConcurrency} conexões simultâneas no Jackett vs {byparrConcurrency} no Byparr). " +
                       $"Para aproveitar a capacidade total do Byparr, aumente 'FlareSolverrMaxConcurrency' no Jackett.";
            }

            if (byparrConcurrency < jackettConcurrency)
            {
                return $"Aviso de Divergência de Concorrência: O sistema está sendo limitado pelo Byparr " +
                       $"({byparrConcurrency} sessões simultâneas no Byparr vs {jackettConcurrency} no Jackett). " +
                       $"Para processar {jackettConcurrency} requisições em paralelo no Byparr, configure MAX_CONCURRENCY={jackettConcurrency} nas variáveis de ambiente do Byparr.";
            }

            return null;
        }
    }
}
