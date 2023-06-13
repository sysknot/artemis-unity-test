using ActiveMQ.Artemis.Client;
using System;
using System.Threading.Tasks;

namespace ArtemisMQ
{
    public class ArtemisReceiver
    {
        private IConsumer consumer = null;
        private ArtemisConnection connection = null;

        public async Task CreateConsumerAsync(ConsumerConfiguration consumerConf, Action OnConsumerCreated)
        {
            try
            {
                if (connection!= null && connection.IsConnected)
                {
                    await CreateProducerAsync(consumerConf);
                    OnConsumerCreated?.Invoke();
                }
                else
                {
                    await CreateConnectionAsync();
                    await CreateProducerAsync(consumerConf);
                    OnConsumerCreated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        public async Task CreateConnectionAsync()
        {
            connection = new ArtemisConnection();
            // We can configure the endpoint for different roles and uses
            connection.ConfigureEndpoint();
            await connection.OpenConnectionAsync();
        }
        public async Task CreateProducerAsync(ConsumerConfiguration consumerConf)
        {
            consumer = await connection.GetConnection().CreateConsumerAsync(consumerConf);
        }

        public async Task DeleteConsumerAndConnectionAsync(Action onConsumerDeleted)
        {
            try
            {
                await DeleteConsumerAsync();
                await CloseConnectionAsync();
                onConsumerDeleted?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        public async Task DeleteConsumerAsync()
        {
            if (consumer != null)
                await consumer.DisposeAsync();
        }

        public async Task CloseConnectionAsync()
        {
            await connection.CloseConnectionAsync();
        }

        public async Task ReceiveMessageAsync(Action<Message> onMessageReceived)
        {
            if (consumer != null)
            { 
                Message message = await consumer.ReceiveAsync();

                if (message != null)
                {
                    // We do receive a new message to process!
                    onMessageReceived?.Invoke(message);
                }
            }
        }

        /// <summary>
        /// Generate an object for the consumer in order to be created.
        /// </summary>
        /// <param name="address">Destination address for the consumer</param>
        /// <param name="queue">Queue for suscribe inside the address</param>
        /// <param name="rType">Routing type - Multicast by default (Anycast/Multicast)</param>
        /// <returns></returns>
        public ConsumerConfiguration CreateConsumerConfiguration(string address, string queue, RoutingType rType = RoutingType.Multicast)
        {
            var consumerConfiguration = new ConsumerConfiguration();
            consumerConfiguration.Address = address;
            consumerConfiguration.Queue = queue;
            consumerConfiguration.RoutingType = rType;

            // Unused attributes
            // consumerConfiguration.Durable = durable;
            // consumerConfiguration.Credit = credit

            return consumerConfiguration;
        }


    }
}