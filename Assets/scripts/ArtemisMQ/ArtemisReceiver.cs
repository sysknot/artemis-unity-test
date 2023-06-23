using ActiveMQ.Artemis.Client;
using System;
using System.Threading.Tasks;

namespace ArtemisMQ
{
    public class ArtemisReceiver
    {
        private IConsumer consumer = null;
        private ArtemisConnection connection = null;

        public async Task ReceiveMessageAsync(bool messageAck, Action<Message> onMessageReceived)
        {
            if (consumer != null)
            {
                Message message = await consumer.ReceiveAsync();
                if (message != null)
                {
                    // Ack bool
                    if (messageAck) await consumer.AcceptAsync(message);

                    onMessageReceived?.Invoke(message);
                }
            }
        }

        public async Task CreateConnectionAsync(Endpoint newEndpoint = null)
        {
            connection = new ArtemisConnection();
            if (newEndpoint != null) connection.ConfigureEndpoint(newEndpoint);
            await connection.OpenConnectionAsync();
        }

        public async Task CreateConsumerAsync(ConsumerConfiguration consumerConf)
        {
            consumer = await connection.GetConnection().CreateConsumerAsync(consumerConf);
        }

        public async Task DisposeConsumerAndConnectionAsync(Action onConsumerDeleted)
        {
            try
            {
                await DisposeConsumerAsync();
                await DisposeConnectionAsync();
                onConsumerDeleted?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        public async Task DisposeConsumerAsync()
        {
            if (consumer != null) await consumer.DisposeAsync();
        }

        public async Task DisposeConnectionAsync()
        {
            await connection.CloseConnectionAsync();
        }

        /// <summary>
        /// Generate an object for the consumer in order to be created.
        /// </summary>
        /// <param name="address">Destination address for the consumer</param>
        /// <param name="queue">Queue for suscribe inside the address</param>
        /// <param name="rType">Routing type - Multicast by default (Anycast/Multicast)</param>
        /// <returns></returns>
        public static ConsumerConfiguration CreateConsumerConfiguration(string address, string? queue = null, RoutingType? rType = null)
        {
            var consumerConfiguration = new ConsumerConfiguration();
            consumerConfiguration.Address = address;
            
            if (queue != null) consumerConfiguration.Queue = queue;
            if (rType != null) consumerConfiguration.RoutingType = rType;
            // Unused attributes
            // consumerConfiguration.Durable = durable;
            // consumerConfiguration.Credit = credit

            return consumerConfiguration;
        }


    }
}