using ActiveMQ.Artemis.Client;
using System;
using System.Threading.Tasks;

namespace ArtemisMQ
{
    class ArtemisSender
    {
        private IProducer producer = null;
        private ArtemisConnection connection = null;

        public async Task CreateProducerAndConnectionAsync(ProducerConfiguration producerConfiguration, Action<string> OnProducerCreated)
        {
            try
            {
                await CreateConnectionAsync();
                await CreateProducerAsync(producerConfiguration);

                OnProducerCreated?.Invoke(producerConfiguration.Address);
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

        public async Task CreateProducerAsync(ProducerConfiguration producerConfiguration)
        {
            producer = await connection.GetConnection().CreateProducerAsync(producerConfiguration);
        }

        public async Task DeleteProducerAndConnectionAsync(Action OnProducerDeleted)
        {
            try
            {
                await DeleteProducerAsync();
                await CloseConnectionAsync();

                OnProducerDeleted?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        public async Task DeleteProducerAsync()
        {
            if (producer != null)
                await producer.DisposeAsync();
        }
        public async Task CloseConnectionAsync()
        {
            await connection.CloseConnectionAsync();
        }

        public async Task SendMessageAsync(Message message, Action onMessageSent)
        {
            // We need to call CreateProducer() first.

            if (producer != null)
            {
                await producer.SendAsync(message);

                // Ack?
                onMessageSent?.Invoke();
            }
        }

        /// <summary>
        /// Helper method for creation the artemis messages.
        /// </summary>
        /// <param name="textMessage">The body of the message</param>
        /// <param name="subject">The subject of the message</param>
        /// <param name="correlationId">Id relation</param>
        /// <param name="timeToLive">In minutes, can be set to 0 for infinite</param>
        /// <param name="priority">Can be used with a casted byte from int (0 to 10)</param>
        /// <param name="durabilityMode">Durable by default, can be non- durable with DurabilityMode.Nodurable)</param>
        /// <returns></returns>
        public Message CreateMessage(string textMessage, string subject = "", string correlationId = "", int timeToLive = 0, byte priority = 5, DurabilityMode durabilityMode = DurabilityMode.Durable)
        {
            var message = new Message(textMessage);
            message.Subject = subject;
            message.CorrelationId = correlationId;
            message.Priority = (byte)priority;
            message.DurabilityMode = durabilityMode;

            if (timeToLive == 0)
                message.TimeToLive = null;
            else
                message.TimeToLive = TimeSpan.FromMinutes(timeToLive);

            return message;
        }

        /// <summary>
        /// Create an object for the producer to be created with.
        /// </summary>
        /// <param name="address">Destination address</param>
        /// <param name="dMode">Durability mode configuration</param>
        /// <param name="rType">Sender address routing type</param>
        /// <returns></returns>
        public ProducerConfiguration CreateProducerConfiguration(string address, DurabilityMode dMode = DurabilityMode.Durable, RoutingType rType = RoutingType.Multicast)
        {
            var producerConfiguration = new ProducerConfiguration();
            producerConfiguration.Address = address;
            producerConfiguration.RoutingType = rType;

            // May override each message durabilityMode
            producerConfiguration.MessageDurabilityMode = dMode;

            return producerConfiguration;
        }

    }
}