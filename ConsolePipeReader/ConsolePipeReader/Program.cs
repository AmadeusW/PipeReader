using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

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

            createPipeServer();

            while (true)
            {
                askForPipeNameAndStart();
            }
        }

        private static void createPipeServer()
        {
            Console.WriteLine("");
            Console.WriteLine("Enter name of the host pipe:");
            string pipeName = Console.ReadLine();

            if (String.IsNullOrEmpty(pipeName))
            {
                return;
            }

            Console.WriteLine("Creating " + pipeName + "...");

            Parallel.Invoke(() => runServer(pipeName));
        }

        private static object runServer(string pipeName)
        {
            int BufferSize = 256;
            Decoder decoder = Encoding.Default.GetDecoder();
            Byte[] bytes = new Byte[BufferSize];
            char[] chars = new char[BufferSize];
            int numBytes = 0;
            StringBuilder msg = new StringBuilder();

            try
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    while (true)
                    {
                        pipeServer.WaitForConnection();

                        do
                        {
                            msg.Length = 0;
                            do
                            {
                                numBytes = pipeServer.Read(bytes, 0, BufferSize);
                                if (numBytes > 0)
                                {
                                    int numChars = decoder.GetCharCount(bytes, 0, numBytes);
                                    decoder.GetChars(bytes, 0, numBytes, chars, 0, false);
                                    msg.Append(chars, 0, numChars);
                                }
                            } while (numBytes > 0 && !pipeServer.IsMessageComplete);

                            decoder.Reset();

                            if (numBytes > 0)
                            {
                                Console.Write(msg);
                            }
                        } while (numBytes != 0);

                        pipeServer.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server Error:");
                Console.WriteLine(ex);
            }
            return null;
        }


        private static void askForPipeNameAndStart()
        {
            Console.WriteLine("");
            Console.WriteLine("Enter name of the client pipe:");
            string pipeName = Console.ReadLine();

            if (String.IsNullOrEmpty(pipeName))
            {
                return;
            }

            Console.WriteLine("Connecting to " + pipeName + "...");

            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
            {
                try
                {
                    pipeClient.Connect(2000);
                }
                catch
                {
                    Console.WriteLine("The Pipe server must be started in order to send data to it.");
                    return;
                }
                Console.WriteLine("Connected to pipe. Send messages.");
                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.WriteLine(Console.ReadLine());
                }
            }   
        }
    }
}

/* Unsuccessful attempt at a server:

        StreamWriter debugWriter;
        NamedPipeServerStream pipeServer;
        private void beginDebugConnection()
        {
            pipeServer = new NamedPipeServerStream("DebugPipe", PipeDirection.Out, 1);

            try
            {
                // Read user input and send that to the client process. 
                debugWriter = new StreamWriter(pipeServer);
                debugWriter.AutoFlush = true;
                // Send a 'sync message' and wait for client to receive it.
                debugWriter.WriteLine("SYNC");
                pipeServer.WaitForPipeDrain();
            }
            // Catch the IOException that is raised if the pipe is broken 
            // or disconnected. 
            catch (IOException e)
            {
                MessageObject errorMessage = new MessageObject(e.Message, CommunicationArguments.MessageKind.Error);
                CommunicationHub.ShowUserMessage(errorMessage);
            }
        }

        private void writeDebugMessage(string message)
        {
            try
            {
                if (debugWriter != null)
                {
                    debugWriter.WriteLine(message);
                }
            }
            catch (IOException e)
            {
                MessageObject errorMessage = new MessageObject(e.Message, CommunicationArguments.MessageKind.Error);
                CommunicationHub.ShowUserMessage(errorMessage);
            }
        }

        private void closeDebugConnection()
        {
            try
            {
                if (debugWriter != null)
                {
                    debugWriter.WriteLine("STOP");
                    debugWriter.Flush();
                    debugWriter.Dispose();
                    debugWriter.Close();
                }
                debugWriter = null;
                if (pipeServer != null)
                {
                    pipeServer.Dispose();
                    pipeServer.Close();
                }
                pipeServer = null;
            }
            catch (IOException e)
            {
                MessageObject errorMessage = new MessageObject(e.Message, CommunicationArguments.MessageKind.Error);
                CommunicationHub.ShowUserMessage(errorMessage);
            }
        }


    */