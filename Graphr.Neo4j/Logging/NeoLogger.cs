using System;
using Graphr.Neo4j.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jLogger = Neo4j.Driver.ILogger;


namespace Graphr.Neo4j.Logging
{
    public class NeoLogger : Neo4jLogger
    {
        private readonly ILogger<NeoLogger> _logger;
        private readonly bool _isDebugEnabled;
        private readonly bool _isTraceEnabled;

        public NeoLogger(ILogger<NeoLogger> logger, NeoDriverConfigurationSettings? settings = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isDebugEnabled = settings?.IsDebugLoggingEnabled ?? false;
            _isTraceEnabled = settings?.IsTraceLoggingEnabled ?? false;
        }

        public void Error(Exception cause, string message, params object[] args) => 
            _logger.LogError(cause, message, args);

        public void Warn(Exception cause, string message, params object[] args) =>
            _logger.LogWarning(cause, message, args);

        public void Info(string message, params object[] args) =>
            _logger.LogInformation(message, args);

        public void Debug(string message, params object[] args) =>
            _logger.LogDebug(message, args);

        public void Trace(string message, params object[] args) =>
            _logger.LogTrace(message, args);

        public bool IsTraceEnabled() => _isTraceEnabled;

        public bool IsDebugEnabled() => _isDebugEnabled;
    }
}