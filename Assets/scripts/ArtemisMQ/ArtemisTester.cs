using UnityEngine;

using System.Threading;
using System;
using ActiveMQ.Artemis.Client;

/*
 
ESC#1:
Multicast.

Sender:
cls && artemis-server-client.exe -batchmode -nographics 1 5 multicast nondurable user.login.test

Receiver:
cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable user.login.test

------------------------------------------------------------------------------

ESC#2:
Anycast.

Sender:
cls && artemis-server-client.exe -batchmode -nographics 1 5 anycast nondurable user.login.test2


Receiver:
cls && artemis-server-client.exe -batchmode -nographics 2 2 anycast nondurable user.login.test2

------------------------------------------------------------------------------

ESC#3:
Wildcards.

Sender:
cls && artemis-server-client.exe -batchmode -nographics 1 5 multicast nondurable user.login.test

Receiver:
cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable *.login.test
cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable *.login.*
cls && artemis-server-client.exe -batchmode -nographics 2 2 multicast nondurable #.test

 */

namespace ArtemisMQ
{

    public class ArtemisTester : MonoBehaviour
    {
        private int sessionTypeArg; // 1 producer , 2 consumer
        private string addressArg;
        private string queueArg;
        private int retrySecondsArg;
        private RoutingType routinTypegArg;
        private DurabilityMode durabilityModeArg;

        private void Start()
        {
            (sessionTypeArg, retrySecondsArg, addressArg, queueArg, routinTypegArg, durabilityModeArg) = ParseArguments();

            Thread serviceThread;
            switch (sessionTypeArg)
            {
                case 1: // producer
                    if (addressArg == string.Empty) break;
                    if (retrySecondsArg == 0)
                    {
                        serviceThread = new Thread(SendMessage);
                        serviceThread.Start();
                    }
                    else
                    {
                        serviceThread = new Thread(RunSenderService);
                        serviceThread.Start();
                    }
                    break;
                case 2: // consumer
                    if (addressArg == string.Empty) break;

                    if (retrySecondsArg == 0)
                    {
                        serviceThread = new Thread(ReceiveMessage);
                        serviceThread.Start();
                    }
                    else
                    {
                        serviceThread = new Thread(RunReceiverService);
                        serviceThread.Start();
                    }
                    break;
            }
        }

        private (int, int, string, string, RoutingType, DurabilityMode) ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            int sessionType = 0;
            string addressDest = "";
            string queueDest = "";
            RoutingType rType = RoutingType.Multicast;
            int retrySeconds = 2;
            DurabilityMode dMode = DurabilityMode.Durable;

            if (args.Length >= 8)
            {
                if (int.TryParse(args[3], out int sType))
                {
                    sessionType = sType;

                    int.TryParse(args[4], out int retrySecondsInt);
                    retrySeconds = retrySecondsInt;

                    if (args[5] == "multicast") rType = RoutingType.Multicast;
                    if (args[5] == "anycast") rType = RoutingType.Anycast;

                    if (args[6] == "durable") dMode = DurabilityMode.Durable;
                    if (args[6] == "nondurable") dMode = DurabilityMode.Nondurable;

                    addressDest = args[7];
                }
            }

            if (args.Length >= 9)
            {
                if (int.TryParse(args[3], out int sType))
                {
                    sessionType = sType;

                    int.TryParse(args[4], out int retrySecondsInt);
                    retrySeconds = retrySecondsInt;

                    if (args[5] == "multicast") rType = RoutingType.Multicast;
                    if (args[5] == "anycast") rType = RoutingType.Anycast;

                    if (args[6] == "durable") dMode = DurabilityMode.Durable;
                    if (args[6] == "nondurable") dMode = DurabilityMode.Nondurable;

                    addressDest = args[7];
                    queueDest = args[8];
                }
            }

            return(sessionType, retrySeconds, addressDest, queueDest, rType, dMode);
        }

        // --------------------------------------------------------------------------------------------------

        private async void RunSenderService()
        {
            ArtemisSender newSender = new ArtemisSender();

            ProducerConfiguration conf = newSender.CreateProducerConfiguration(addressArg, durabilityModeArg, routinTypegArg);

            await newSender.CreateProducerAndConnectionAsync(conf, (string m) => { Debug.Log("Producer created. Address: " + m); });

            while(true)
            {
                // Random timestamp message for debuging.
                Message m = newSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");

                await newSender.SendMessageAsync(m, () => { Debug.Log("Message sent!"); });

                Thread.Sleep(retrySecondsArg * 1000);
            }
        }

        private async void SendMessage()
        {
            ArtemisSender newSender = new ArtemisSender();
            ProducerConfiguration conf = newSender.CreateProducerConfiguration(addressArg, durabilityModeArg, routinTypegArg);
            await newSender.CreateProducerAndConnectionAsync(conf, (string m) => { Debug.Log("Producer created. Address: " + m); });
            Message m = newSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");
            await newSender.SendMessageAsync(m, () => { Debug.Log("Message sent!"); });
        }

        // --------------------------------------------------------------------------------------------------

        private async void RunReceiverService()
        {
            ArtemisReceiver newReceiver = new ArtemisReceiver();
            ConsumerConfiguration conf = newReceiver.CreateConsumerConfiguration(addressArg, queueArg, routinTypegArg);

            await newReceiver.CreateConsumerAsync(conf, () => { Debug.Log($"Consumer created: {addressArg} {queueArg}"); });

            while(true)
            {
                await newReceiver.ReceiveMessageAsync((Message m) =>
                {
                    Debug.Log($"Message received: {m.GetBody<string>()}");
                    
                    /* ... */
                });

                Thread.Sleep(retrySecondsArg * 1000);
            }
        }

        private async void ReceiveMessage()
        {
            ArtemisReceiver newReceiver = new ArtemisReceiver();
            ConsumerConfiguration conf = newReceiver.CreateConsumerConfiguration(addressArg, queueArg, routinTypegArg);
            await newReceiver.CreateConsumerAsync(conf, () => { Debug.Log($"Consumer created: {addressArg} {queueArg}"); });
            await newReceiver.ReceiveMessageAsync((Message m) => { Debug.Log($"Message received: {m.GetBody<string>()}"); });
        }
    }
}