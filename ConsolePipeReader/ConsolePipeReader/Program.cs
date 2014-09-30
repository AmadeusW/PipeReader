using System;
using System.IO;
using System.IO.Pipes;

namespace ConsolePipeReader
{
    /// <summary>
    /// Requests a name of a handle to the pipe and displays messages received through the pipe.
    /// Based on http://msdn.microsoft.com/en-us/library/bb546102(v=vs.110).aspx
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Pipe Reader.");

            while (true)
            {
                askForPipeNameAndStart();
            }
        }

        private static void askForPipeNameAndStart()
        {
            // Tracks whether the connection has started, used in exception handling.
            bool activeConnection = false;

            Console.WriteLine("");
            Console.WriteLine("Enter name of the pipe to listen to:");
            string pipeName = Console.ReadLine();

            if (String.IsNullOrEmpty(pipeName))
            {
                return;
            }

            Console.WriteLine("Connecting to " + pipeName + "...");

            try
            {
                using (PipeStream pipeStream = new AnonymousPipeClientStream(PipeDirection.In, pipeName))
                {
                    var checkMe = pipeStream.TransmissionMode;

                    using (StreamReader reader = new StreamReader(pipeStream))
                    {
                        string receivedMessage;

                        // Wait until we receive "SYNC"
                        do
                        {
                            Console.WriteLine("Waiting for the other party...");
                            receivedMessage = reader.ReadLine();
                        }
                        while (receivedMessage != "SYNC");

                        activeConnection = true;
                        Console.WriteLine("Established connection.");
                        Console.WriteLine("--------------------------------------------------------------");

                        // Display messages until we receive "STOP"
                        while ((receivedMessage = reader.ReadLine()) != "STOP")
                        {
                            Console.WriteLine(receivedMessage);
                        }
                        activeConnection = false;
                        Console.WriteLine("--------------------------------------------------------------");
                        Console.WriteLine("The other party stopped communication.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // If we are receiving messages, draw a separator before printing local message.
                if (activeConnection)
                {
                    Console.WriteLine("--------------------------------------------------------------");
                }
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
