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
        public int QueryTimeoutInMs { get; set; } = 250;
    }
}