using System;
using System.Text;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeCommunication
{
    public static class PipeHost
    {
        public static void CreateBackgroundPipeHost(string pipeName, Queue<String> receivedQueue, int bufferSize = 256)
        {
            Task.Run(() => PipeHost.CreatePipeHost(pipeName, receivedQueue, bufferSize));
        }

        public static void CreatePipeHost(string pipeName, Queue<String> receivedQueue, int bufferSize)
        {
            Decoder decoder = Encoding.Default.GetDecoder();
            Byte[] bytes = new Byte[bufferSize];
            char[] chars = new char[bufferSize];
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
                                numBytes = pipeServer.Read(bytes, 0, bufferSize);
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
                                receivedQueue.Enqueue(msg.ToString());
                            }
                        } while (numBytes != 0);
                        // Apparently, we need to reconnect each time
                        pipeServer.Disconnect();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
