using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace PipeCommunication
{
    public class PipeClient
    {
        string _pipeName;
        Queue<String> _messageQueue;
        Object _access;
        CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public PipeClient(string pipeName)
        {
            _pipeName = pipeName;
            _messageQueue = new Queue<string>();
            _access = new Object();
        }

        public void RunClient()
        {
            var token = _tokenSource.Token;
            Task.Run(() => internalRun(token), token);
        }

        public void SendMessage(string message)
        {
            lock(_access)
            {
                _messageQueue.Enqueue(message);
                Monitor.Pulse(_access);
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        private void internalRun(CancellationToken token)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (!pipeClient.IsConnected)
                        {
                            pipeClient.Connect(2000);
                        }
                    }
                    catch (TimeoutException oEX)
                    {
                        // The Pipe server must be started in order to send data to it.
                        return;
                    }

                    lock (_access)
                    {
                        while (_messageQueue.Count == 0)
                        {
                            Monitor.Wait(_access);
                        }

                        var sentString = _messageQueue.Dequeue();
                        byte[] _buffer = Encoding.UTF8.GetBytes(sentString);
                        pipeClient.BeginWrite(_buffer, 0, _buffer.Length, AsyncSend, pipeClient);

                        Monitor.Pulse(_access);
                    }
                }
            }
        }

        private void AsyncSend(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeClientStream pipeStream = (NamedPipeClientStream)iar.AsyncState;

                // End the write
                pipeStream.EndWrite(iar);
                pipeStream.Flush();
            }
            catch (Exception oEX)
            {
                throw;
            }
        }
    }
}
