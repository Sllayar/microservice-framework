namespace RFI.MicroserviceFramework._Metrics
{
    internal static class SMetrics
    {
        public static readonly Metrica CounterExceptions = new Metrica("CounterExceptions", "exception");
        public static readonly Metrica CounterHealthCheck = new Metrica("CounterHealthCheck", "result");
    }
}
