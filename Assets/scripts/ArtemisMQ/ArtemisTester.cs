using UnityEngine;

using System.Threading;
using System;
using ActiveMQ.Artemis.Client;

/*
    // more parameters.
    // -routingType <multicast/anycast> -durabilityMode <durable/nondurable>  
    // -message <string>

    artemis-server-client.exe -batchmode -nographics -clientMode sender -address test.login -interval 2
    
    artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.login -queue q1 -interval 2 -ack ack
    artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.login -queue q1 -interval 2 -ack ack
    artemis-server-client.exe -batchmode -nographics -clientMode receiver -address test.login -queue q2 -interval 2 -ack ack
 */

namespace ArtemisMQ
{

    public class ArtemisTester : MonoBehaviour
    {
        private int sessionTypeArg; // 1 producer , 2 consumer
        private string addressArg;
        private string? queueArg;
        private int retrySecondsArg;
        private RoutingType? routinTypegArg;
        private DurabilityMode? durabilityModeArg;
        private bool ackModeArg;
        private string messageArg;

        private void Start()
        {
            (sessionTypeArg, retrySecondsArg, addressArg, queueArg, routinTypegArg, durabilityModeArg, ackModeArg, messageArg) = ParseArguments();

            ProducerConfiguration producerConf = ArtemisSender.CreateProducerConfiguration(addressArg, routinTypegArg, durabilityModeArg);
            ConsumerConfiguration consumerConf = ArtemisReceiver.CreateConsumerConfiguration(addressArg, queueArg, routinTypegArg, durabilityModeArg);

            Thread serviceThread;
            switch (sessionTypeArg)
            {
                case 1: // producer
                    if (addressArg == string.Empty) break;
                    if (retrySecondsArg == 0)
                    {
                        serviceThread = new Thread(() => SendMessage(producerConf, messageArg));
                        serviceThread.Start();
                    }
                    else
                    {
                        serviceThread = new Thread(() => RunSenderService(producerConf, messageArg));
                        serviceThread.Start();
                    }
                    break;
                case 2: // consumer
                    if (addressArg == string.Empty) break;

                    if (retrySecondsArg == 0)
                    {
                        serviceThread = new Thread(() => ReceiveMessage(consumerConf));
                        serviceThread.Start();
                    }
                    else
                    {
                        serviceThread = new Thread(() => RunReceiverService(consumerConf));
                        serviceThread.Start();
                    }
                    break;
            }
        }

        private (int, int, string, string?, RoutingType?, DurabilityMode?, bool, string) ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            int sessionType = 0;
            string addressDest = "empty";
            string? queueDest = null;
            RoutingType? rType = null;
            DurabilityMode? dMode = null;
            int retrySeconds = 2;
            bool ackMode = false;
            string message = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-clientMode" && i + 1 < args.Length)
                {
                    if (args[i + 1] == "sender") sessionType = 1;
                    if (args[i + 1] == "receiver") sessionType = 2;
                }
                else if (args[i] == "-address" && i + 1 < args.Length)
                {
                    addressDest = args[i + 1];
                }
                else if (args[i] == "-queue" && i + 1 < args.Length)
                {
                    queueDest = args[i + 1];
                }
                else if (args[i] == "-routingType" && i + 1 < args.Length)
                {
                    if (args[i + 1] == "multicast") rType = RoutingType.Multicast;
                    if (args[i + 1] == "anycast") rType = RoutingType.Anycast;
                }
                else if (args[i] == "-ack" && i + 1 < args.Length)
                {
                    if (args[i + 1] == "ack") ackMode = true;
                    if (args[i + 1] == "noack") ackMode = false;
                } 
                else if (args[i] == "-durabilityMode" && i + 1 < args.Length)
                {
                    if (args[i + 1] == "durable") dMode = DurabilityMode.Durable;
                    if (args[i + 1] == "nondurable") dMode = DurabilityMode.Nondurable;
                }
                else if (args[i] == "-interval" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out int retrySecondsInt);
                    retrySeconds = retrySecondsInt;
                }
                else if (args[i] == "-message" && i +1 < args.Length)
                {
                    message = args[i + 1];
                }
            }

            return (sessionType, retrySeconds, addressDest, queueDest, rType, dMode, ackMode, message);
        }

        // --------------------------------------------------------------------------------------------------

        private async void RunSenderService(ProducerConfiguration conf, string message)
        {
            ArtemisSender newSender = new ArtemisSender();

            await newSender.CreateConnectionAsync();
            await newSender.CreateProducerAsync(conf);

            while (true)
            {
                // Message m = ArtemisSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");
                Message m = ArtemisSender.CreateMessage(message);

                await newSender.SendMessageAsync(m, () => { Debug.Log("Message sent!"); });

                Thread.Sleep(retrySecondsArg * 1000);
            }
        }

        private async void SendMessage(ProducerConfiguration conf, string message)
        {
            ArtemisSender newSender = new ArtemisSender();

            await newSender.CreateConnectionAsync();
            await newSender.CreateProducerAsync(conf);

            // Message m = ArtemisSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");
            Message m = ArtemisSender.CreateMessage(message);

            await newSender.SendMessageAsync(m, () => { Debug.Log("Message sent!"); });

            await newSender.DisposeProducerAndConnectionAsync(() => { Debug.Log("Closed connection and sender."); }); ;
        }


        // --------------------------------------------------------------------------------------------------

        private async void RunReceiverService(ConsumerConfiguration conf)
        {
            ArtemisReceiver newReceiver = new ArtemisReceiver();

            await newReceiver.CreateConnectionAsync();
            await newReceiver.CreateConsumerAsync(conf);

            while (true)
            {
                if (ackModeArg == false)
                {
                    await newReceiver.ReceiveMessageAsync(false, (Message m) =>
                    {
                        Debug.Log($"Message received: {m.GetBody<string>()}");
                        /* ... */
                    });
                }
                else
                {
                    await newReceiver.ReceiveMessageAsync(true, (Message m) =>
                    {
                        Debug.Log($"Message received (ACK): {m.GetBody<string>()}");
                        /* ... */
                    });
                }

                Thread.Sleep(retrySecondsArg * 1000);
            }
        }

        private async void ReceiveMessage(ConsumerConfiguration conf)
        {
            ArtemisReceiver newReceiver = new ArtemisReceiver();

            await newReceiver.CreateConnectionAsync();
            await newReceiver.CreateConsumerAsync(conf);

            if (ackModeArg == false)
            {
                await newReceiver.ReceiveMessageAsync(false, (Message m) =>
                {
                    Debug.Log($"Message received: {m.GetBody<string>()}");
                    /* ... */
                });
            }
            else
            {
                await newReceiver.ReceiveMessageAsync(true, (Message m) =>
                {
                    Debug.Log($"Message received (ACK): {m.GetBody<string>()}");
                    /* ... */
                });
            }


            await newReceiver.DisposeConsumerAndConnectionAsync(() => { Debug.Log("Closed connection and sender."); }); ;
        }

        // --------------------------------------------------------------------------------------------------
    }
}