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
        public void ConfigureEndpoint(string user = "admin", string password = "admin", string host = "localhost", int port = 61616)
        {
            artemisEndpoint = Endpoint.Create(host, port, user, password);
        }

        public void ConfigureEndpoint(Endpoint newEndpoint)
        {
            artemisEndpoint = newEndpoint;
        }

        public async Task OpenConnectionAsync()
        {
            var connectionFactory = new ConnectionFactory();
            connection = await connectionFactory.CreateAsync(artemisEndpoint);
        }

        public async Task CloseConnectionAsync()
        {
            await connection.DisposeAsync();
        }
    }

}


