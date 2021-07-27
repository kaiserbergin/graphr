namespace Graphr.Neo4j.Configuration
{
    public class NeoDriverConfigurationSettings
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsDebugLoggingEnabled { get; set; }
        public bool IsTraceLoggingEnabled { get; set; }
    }
}