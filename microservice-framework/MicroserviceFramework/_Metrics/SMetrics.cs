namespace RFI.MicroserviceFramework._Metrics
{
    public static class SMetrics
    {
        public static readonly Metrica CounterExceptions = new Metrica("CounterExceptions", "exception");

        public static readonly Metrica CounterHealthCheck = new Metrica("CounterHealthCheck", "result");

        public static readonly Metrica CounterApiRequests = new Metrica("CounterRequests", "User", "Path", "StatusCode", "StatusMessage");
    }
}
