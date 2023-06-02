using UnityEngine;

using ApacheArtemis.Producer;
using ApacheArtemis.Consumer;
using System.Threading;
using System;

// >artemis-server-client.exe -batchmode - nographics 1 5 user-summon
// >artemis-server-client.exe -batchmode - nographics 2 2 user-summon

public class Tester : MonoBehaviour
{
    private bool isRunning;
    private int sessionTypeArg; // 1 producer , 2 consumer

    private int waitTimeSecondsArg;
    private string addressArg;
    private string queueArg;

    private void Start()
    {
        isRunning = true;
        (sessionTypeArg, waitTimeSecondsArg, addressArg, queueArg) = ParseCommandLineArguments();

        Thread serviceThread;
        switch (sessionTypeArg)
        {
            case 1: // producer
                serviceThread = new Thread(RunProducerService);
                serviceThread.Start();
                break;
            case 2: // consumer
                serviceThread = new Thread(RunConsumerService);
                serviceThread.Start();
                break;
        }
    }

    private void OnApplicationQuit()
    {
        //Consumer.CancelReceiving();
        Consumer.CloseConnection();
        Producer.CloseConnection();
        isRunning = false;
    }

    private (int, int, string, string) ParseCommandLineArguments()
    {
        string[] args = Environment.GetCommandLineArgs();

        int waitTime = 5;
        int sessionType = 2;
        string addressAux = string.Empty;
        string queueAux = string.Empty;

        // Verificar que los argumentos de batchmode y nographics están presentes
        if (args.Length >= 3 && args[1] == "-batchmode" && args[2] == "-nographics")
        {
            // Analizar los argumentos restantes
            if (args.Length >= 7 && int.TryParse(args[3], out int parsedSessionType) && int.TryParse(args[4], out int parsedWaitTime))
            {
                sessionType = parsedSessionType;
                waitTime = parsedWaitTime;
                addressAux = args[5];
                queueAux = args[6];
            }
            else if (args.Length >= 6 && int.TryParse(args[3], out parsedSessionType) && int.TryParse(args[4], out parsedWaitTime))
            {
                sessionType = parsedSessionType;
                waitTime = parsedWaitTime;
                addressAux = args[5];
            }
            else if (args.Length >= 5 && int.TryParse(args[3], out parsedSessionType))
            {
                sessionType = parsedSessionType;
            }
        }
        
        // Debug.Log("ParseCommandLineArguments: " + sessionType + " " + waitTime + " " + addressAux + " " + queueAux);
        return (sessionType, waitTime, addressAux, queueAux);
    }

    private async void RunConsumerService()
    {
        await Consumer.InitializeConnection();
        await Consumer.CreateConsumer(Consumer.GenerateConsumerConfiguration(addressArg, queueArg));

        await Consumer.ReceiveMessage(waitTimeSecondsArg);
        //await Consumer.CancelReceiving();
    }

    private async void RunProducerService()
    {
        await Producer.InitializeConnection();
        await Producer.CreateProducer(addressArg);

        while (isRunning)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            await Producer.SendMessage($"UnityMessage! ##{timestamp}##");

            Thread.Sleep(waitTimeSecondsArg * 1000); // seconds
        }
    }

}