using Xunit.Abstractions;

namespace RFI._Helpers
{
    public static class TestsLogger
    {
        private static ITestOutputHelper TestOutputHelper;
        public static void Initialize(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;
        public static void WriteLine(string message) => TestOutputHelper.WriteLine(message);
    }
}
