using UnityEngine;

using System.Threading;
using System;
using ActiveMQ.Artemis.Client;

/*
    ESC#1:
    Multicast.

    Sender:
    cls && artemis-server-client.exe -batchmode -nographics sender 5 multicast nondurable noack user.login.test

    Receiver:
    cls && artemis-server-client.exe -batchmode -nographics receiver 2 multicast nondurable noack user.login.test

    ------------------------------------------------------------------------------

    ESC#2:
    Anycast.

    Sender:
    cls && artemis-server-client.exe -batchmode -nographics sender 5 anycast nondurable noack user.login.test2


    Receiver:
    cls && artemis-server-client.exe -batchmode -nographics receiver 0 anycast nondurable noack user.login.test2
    cls && artemis-server-client.exe -batchmode -nographics receiver 0 anycast nondurable ack user.login.test2

    cls && artemis-server-client.exe -batchmode -nographics receiver 2 anycast nondurable noack user.login.test2
    cls && artemis-server-client.exe -batchmode -nographics receiver 2 anycast nondurable noack user.login.test2

    ------------------------------------------------------------------------------

    ESC#3:
    Wildcards.

    Sender:
    cls && artemis-server-client.exe -batchmode -nographics sender 5 multicast nondurable noack user.login.test

    Receiver:
    cls && artemis-server-client.exe -batchmode -nographics receiver 2 multicast nondurable noack *.login.test
    cls && artemis-server-client.exe -batchmode -nographics receiver 2 multicast nondurable noack *.login.*
    cls && artemis-server-client.exe -batchmode -nographics receiver 2 multicast nondurable noack #.test

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
        private bool ackModeArg;

        private void Start()
        {
            (sessionTypeArg, retrySecondsArg, addressArg, queueArg, routinTypegArg, durabilityModeArg, ackModeArg) = ParseArguments();

            ProducerConfiguration producerConf = ArtemisSender.CreateProducerConfiguration(addressArg, durabilityModeArg, routinTypegArg);
            ConsumerConfiguration consumerConf = ArtemisReceiver.CreateConsumerConfiguration(addressArg, queueArg, routinTypegArg);

            Thread serviceThread;
            switch (sessionTypeArg)
            {
                case 1: // producer
                    if (addressArg == string.Empty) break;
                    if (retrySecondsArg == 0)
                    {
                        serviceThread = new Thread(() => SendMessage(producerConf));
                        serviceThread.Start();
                    }
                    else
                    {
                        serviceThread = new Thread(() => RunSenderService(producerConf));
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

        private (int, int, string, string, RoutingType, DurabilityMode, bool) ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            int sessionType = 0;
            string addressDest = "";
            string queueDest = "";
            RoutingType rType = RoutingType.Multicast;
            int retrySeconds = 2;
            DurabilityMode dMode = DurabilityMode.Durable;
            bool ackMode = false;

            if (args.Length >= 9)
            {
                if (args[3] == "sender") sessionType = 1;
                if (args[3] == "receiver") sessionType = 2;

                int.TryParse(args[4], out int retrySecondsInt);
                retrySeconds = retrySecondsInt;

                if (args[5] == "multicast") rType = RoutingType.Multicast;
                if (args[5] == "anycast") rType = RoutingType.Anycast;

                if (args[6] == "durable") dMode = DurabilityMode.Durable;
                if (args[6] == "nondurable") dMode = DurabilityMode.Nondurable;

                if (args[7] == "ack") ackMode = true;
                if (args[7] == "noack") ackMode = false;

                addressDest = args[8];

            }

            if (args.Length >= 10)
            {
                if (args[3] == "sender") sessionType = 1;
                if (args[3] == "receiver") sessionType = 2;

                int.TryParse(args[4], out int retrySecondsInt);
                retrySeconds = retrySecondsInt;

                if (args[5] == "multicast") rType = RoutingType.Multicast;
                if (args[5] == "anycast") rType = RoutingType.Anycast;

                if (args[6] == "durable") dMode = DurabilityMode.Durable;
                if (args[6] == "nondurable") dMode = DurabilityMode.Nondurable;

                if (args[7] == "ack") ackMode = true;
                if (args[7] == "noack") ackMode = false;

                addressDest = args[8];
                queueDest = args[9];
            }

            return (sessionType, retrySeconds, addressDest, queueDest, rType, dMode, ackMode);
        }

        // --------------------------------------------------------------------------------------------------

        private async void RunSenderService(ProducerConfiguration conf)
        {
            ArtemisSender newSender = new ArtemisSender();

            await newSender.CreateConnectionAsync();
            await newSender.CreateProducerAsync(conf);

            while (true)
            {
                Message m = ArtemisSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");

                await newSender.SendMessageAsync(m, () => { Debug.Log("Message sent!"); });

                Thread.Sleep(retrySecondsArg * 1000);
            }
        }

        private async void SendMessage(ProducerConfiguration conf)
        {
            ArtemisSender newSender = new ArtemisSender();

            await newSender.CreateConnectionAsync();
            await newSender.CreateProducerAsync(conf);

            Message m = ArtemisSender.CreateMessage($"Message: {DateTime.Now.ToString("HH:mm:ss")}");
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