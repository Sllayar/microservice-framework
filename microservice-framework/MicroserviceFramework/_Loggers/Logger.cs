using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using RFI.MicroserviceFramework._Environment;
using RFI.MicroserviceFramework._Helpers;
using RFI.MicroserviceFramework._Metrics;

namespace RFI.MicroserviceFramework._Loggers
{
    public enum LogLevel
    {
        Info,
        Warning,
        Exception
    }

    public static class Logger
    {
        public static void Log(bool info, string message, object extra = null, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = null)
        {
            WriteLine(info ? LogLevel.Info : LogLevel.Warning, message, extra, methodName, filePath);
        }

        public static void Log(this Exception ex, string message = null, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = null)
        {
            var exception = ex.InnerException ?? ex;

            var exMessageAndStack = new List<string>();
            if(exception.Message.NotEmpty()) exMessageAndStack.AddRange(exception.Message.Split(Environment.NewLine));
            if(exception.StackTrace.NotEmpty()) exMessageAndStack.AddRange(exception.StackTrace.Split(Environment.NewLine));

            WriteLine(LogLevel.Exception, message, exMessageAndStack, methodName, filePath);

            SMetrics.CounterExceptions.Inc(exception.GetType().Name);
        }

        private static void WriteLine(LogLevel level, string logMessage, object logExtra, string methodName, string filePath)
        {
            Console.Out.WriteLine(JsonConvert.SerializeObject(new
            {
                level = level.ToString("G"),
                module = filePath.NotEmpty() ? filePath?.Regex(@"[^\\/]+(?=\.cs)") : null,
                method = methodName,
                message = logMessage,
                extra = logExtra
            }, SEnv.IsDebug ? Formatting.Indented : Formatting.None));
        }
    }
}