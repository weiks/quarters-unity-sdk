using Microsoft.Extensions.Logging;

namespace Packages.gg.poq.unity.sdk.Runtime.QuartersSDK.Scripts.FileLogger
{
    public static class FileLoggerExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string filePath)
        {
            factory.AddProvider(new FileLoggerProvider(filePath));
            return factory;
        }
    }
}
