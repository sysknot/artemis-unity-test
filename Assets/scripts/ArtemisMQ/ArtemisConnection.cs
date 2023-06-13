using ActiveMQ.Artemis.Client;
using System;
using System.Threading.Tasks;

namespace ArtemisMQ
{
    public class ArtemisConnection
    {
        // We better hardcode this part by now. We should manage users for permissions and stuff.
        private Endpoint artemisEndpoint = Endpoint.Create("localhost", 61616, "admin", "admin");

        // Interface for artemis connection.
         private IConnection connection;

        // Public event for connection.
        public event Action OnConnected;
        public event Action OnDisconnected;

        public bool IsConnected => connection?.IsOpened ?? false;

        public IConnection GetConnection() => connection;

        // If we need to use different connections (users/roles, endpoints)
        public void ConfigureEndpoint(string user = "admin", string password = "admin", string host="localhost", int port=61616)
        {
            artemisEndpoint = Endpoint.Create(host, port, user, password);
        }

        // Main connection method. Can be used asynchronously
        public async Task OpenConnectionAsync()
        {
            var connectionFactory = new ConnectionFactory();
            connection = await connectionFactory.CreateAsync(artemisEndpoint);
            // We call the event
            OnConnected?.Invoke();
        }

        // Dispose the object (producer/consumer) and close connection.
        public async Task CloseConnectionAsync()
        {
            await connection.DisposeAsync();

            // Finally call the event
            OnDisconnected?.Invoke();
        }
    }

}


