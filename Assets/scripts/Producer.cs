using UnityEngine;

using ActiveMQ.Artemis.Client;
using System;
using System.Threading.Tasks;

namespace ApacheArtemis.Producer
{
    class Producer
    {
        private static string addressDest;
        private static string queueDest;

        static private Endpoint artemisEndpoint = Endpoint.Create("localhost", 61616, "artemis", "artemis");
        static private IConnection connection;
        static private IProducer producer = null;

        static public async Task InitializeConnection()
        {
            var connectionFactory = new ConnectionFactory();
            connection = await connectionFactory.CreateAsync(artemisEndpoint);
        }

        static public async Task CloseConnection()
        {
            await producer.DisposeAsync();
            await connection.DisposeAsync();
        }

        static public async Task CreateProducer(string addressDest)
        {
            producer = await connection.CreateProducerAsync(addressDest);
        }

        static public async Task SendMessage(string messageText)
        {
            if (!connection.IsOpened)
                await InitializeConnection(); // Reconnect

            if(producer != null) 
            {
                var messageAux = new ActiveMQ.Artemis.Client.Message(messageText);

                // Message options:
                messageAux.TimeToLive = TimeSpan.FromMinutes(2);
                messageAux.Priority = 5;
                messageAux.DurabilityMode = DurabilityMode.Nondurable;
                //messageAux.Subject = "Test Subject from code";
                //messageAux.CorrelationId = "1234";

                await producer.SendAsync(messageAux);
            }
        }

    }
}
