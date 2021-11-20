using System;
using Graphr.Neo4j.Configuration;
using Neo4j.Driver;
using Neo4jLogger = Neo4j.Driver.ILogger;

namespace Graphr.Neo4j.Driver
{
    public class DriverProvider : IDisposable, IDriverProvider
    {
        private bool _disposed = false;
        public IDriver Driver { get; }

        ~DriverProvider() => Dispose(false);

        public DriverProvider(NeoDriverConfigurationSettings settings, Neo4jLogger? neoLogger = null)
        {
            if (settings?.Url == null || settings.Username == null || settings.Password == null) 
                throw new ArgumentNullException(nameof(settings));
            
            Driver = GraphDatabase.Driver(
                settings.Url, 
                AuthTokens.Basic(settings.Username, settings.Password),
                builder =>
                {
                    if (neoLogger != null) builder.WithLogger(neoLogger);
                    builder
                        .WithEncryptionLevel(settings.EncryptionLevel)
                        .WithMaxTransactionRetryTime(settings.MaxTransactionRetryTime)
                        .WithMaxIdleConnectionPoolSize(settings.MaxConnectionPoolSize)
                        .WithMaxConnectionPoolSize(settings.MaxConnectionPoolSize)
                        .WithConnectionAcquisitionTimeout(settings.ConnectionAcquisitionTimeout)
                        .WithConnectionTimeout(settings.ConnectionTimeout)
                        .WithSocketKeepAliveEnabled(settings.SocketKeepAlive)
                        .WithConnectionIdleTimeout(settings.ConnectionIdleTimeout)
                        .WithMaxConnectionLifetime(settings.MaxConnectionLifetime)
                        .WithIpv6Enabled(settings.Ipv6Enabled)
                        .WithDefaultReadBufferSize(settings.DefaultReadBufferSize)
                        .WithMaxReadBufferSize(settings.MaxReadBufferSize)
                        .WithDefaultWriteBufferSize(settings.DefaultWriteBufferSize)
                        .WithMaxWriteBufferSize(settings.MaxWriteBufferSize)
                        .WithFetchSize(settings.FetchSize);
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