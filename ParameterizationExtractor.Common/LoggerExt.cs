using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quipu.ParameterizationExtractor.Common
{
    public static class LoggerExt
    {
        public static void Debug(this ILogger logger, string msg, params object[] p)
        {
            logger.LogDebug(msg, p);
        }

        public static void DebugFormat(this ILogger logger, string msg, params object[] p)
        {
            logger.LogDebug(msg, p);
        }

        public static void InfoFormat(this ILogger logger, string msg, params object[] p)
        {
            logger.LogInformation(msg, p);
        }

        public static void Error(this ILogger logger, string msg, params object[] p)
        {
            logger.LogError(msg, p);
        }

        public static void Error(this ILogger logger, Exception ex)
        {
            logger.LogError(ex, ex?.Message);
        }
    }
}
