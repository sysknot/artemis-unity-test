using UnityEngine;

using ActiveMQ.Artemis.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApacheArtemis.Consumer
{
    class Consumer
    {
        private static string addressDest;
        private static string queueDest;

        static private Endpoint artemisEndpoint = Endpoint.Create("localhost", 61616, "artemis", "artemis");
        static private IConnection connection;
        static private IConsumer consumer = null;
        private ConsumerConfiguration consumerConf = null;

        static private CancellationTokenSource cancellationTokenSource = null;
        static private CancellationToken cancellationToken;


        static public async Task InitializeConnection()
        {
            var connectionFactory = new ConnectionFactory();
            connection = await connectionFactory.CreateAsync(artemisEndpoint);
        }

        static public async Task CloseConnection()
        {
            await consumer.DisposeAsync();
            await connection.DisposeAsync();
        }

        static public async Task CreateConsumer(ConsumerConfiguration conf)
        {
            consumer = await connection.CreateConsumerAsync(conf);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        static public ConsumerConfiguration GenerateConsumerConfiguration(string address, string queue = null, bool durable = false)
        {
            var consumerConfiguration = new ConsumerConfiguration();
            consumerConfiguration.Address = addressDest = address;
            consumerConfiguration.Queue = queueDest= queue;
            consumerConfiguration.Durable = durable;
            return consumerConfiguration;
        }

        static public async Task ReceiveMessage(int secondsDelayReceiver)
        {
            if (!connection.IsOpened)
                await InitializeConnection(); // Reconnect

            if (consumer != null)
            {
                if (queueDest == null) queueDest = string.Empty;
                Debug.Log("Receiving messages from: " + addressDest + ":" + queueDest);

                Task backTask = Task.Run(() => ReceiveMessagesFromQueue(consumer, cancellationToken, secondsDelayReceiver));
            }
        }

       static public Task CancelReceiving()
        {
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        static async Task ReceiveMessagesFromQueue(IConsumer? receiver, CancellationToken cancellationToken, int secondsDelayReceiver)
        {
            while (!cancellationToken.IsCancellationRequested && receiver != null)
            {
                Message message = await receiver.ReceiveAsync();

                if (message != null)
                {
                    Debug.Log($"New message: {message.GetBody<string>()}");
                }

                await Task.Delay(TimeSpan.FromSeconds(secondsDelayReceiver), cancellationToken);
            }
        }

    }
}