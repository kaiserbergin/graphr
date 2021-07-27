using System;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.Logging;
using Neo4j.Driver;

namespace Graphr.Neo4j.Driver
{
    public class DriverProvider : IDisposable, IDriverProvider
    {
        private bool _disposed = false;
        public IDriver Driver { get; }

        ~DriverProvider() => Dispose(false);

        public DriverProvider(NeoDriverConfigurationSettings settings, NeoLogger neoLogger)
        {
            Driver = GraphDatabase.Driver(
                settings.Url, 
                AuthTokens.Basic(settings.Username, settings.Password),
                builder =>
                {
                    if (neoLogger != null)
                        builder.WithLogger(neoLogger);
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Driver?.Dispose();
            }

            _disposed = true;
        }
    }
}