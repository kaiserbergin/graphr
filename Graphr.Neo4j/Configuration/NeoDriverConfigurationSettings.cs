using System;
using Neo4j.Driver;

namespace Graphr.Neo4j.Configuration
{
    public class NeoDriverConfigurationSettings : Config
    {
        public string? Url { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool IsDebugLoggingEnabled { get; set; }
        public bool IsTraceLoggingEnabled { get; set; }
        public new string? EncryptionLevel { get; set; }
        public int? QueryTimeoutInMs { get; set; } = 250;
        public int? MaxTransactionRetryTimeMs { get; set; } = 30_000;
        public int? ConnectionAcquisitionTimeoutMs { get; set; } = 100_000;
        public int? ConnectionTimeoutMs { get; set; } = 30_000;
        public long? ConnectionIdleTimeoutMs { get; set; } = -1;
        public long? MaxConnectionLifetimeMs { get; set; } = 3_600_000;
        public bool IncludeHealthChecks { get; set; } = true;
        public string HealthCheckName { get; set; } = "neo4j-health-check";
        public bool VerifyConnectivity { get; set; } = false;
    }
}