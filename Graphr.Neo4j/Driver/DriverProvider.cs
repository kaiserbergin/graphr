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

                    if (settings.EncryptionLevel != null && Enum.TryParse<EncryptionLevel>(settings.EncryptionLevel, out var encryptionLevel))
                        builder.WithEncryptionLevel(encryptionLevel);
                    
                    builder
                        .WithMaxTransactionRetryTime(TimeSpan.FromMilliseconds(settings.MaxTransactionRetryTimeMs!.Value))
                        .WithMaxIdleConnectionPoolSize(settings.MaxConnectionPoolSize)
                        .WithMaxConnectionPoolSize(settings.MaxConnectionPoolSize)
                        .WithConnectionAcquisitionTimeout(TimeSpan.FromMilliseconds(settings.ConnectionAcquisitionTimeoutMs!.Value))
                        .WithConnectionTimeout(TimeSpan.FromMilliseconds(settings.ConnectionTimeoutMs!.Value))
                        .WithSocketKeepAliveEnabled(settings.SocketKeepAlive)
                        .WithConnectionIdleTimeout(TimeSpan.FromMilliseconds(settings.ConnectionIdleTimeoutMs!.Value))
                        .WithMaxConnectionLifetime(TimeSpan.FromMilliseconds(settings.MaxConnectionLifetimeMs!.Value))
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