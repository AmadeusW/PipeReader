using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;

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
                try
                {
                    pipeClient.Connect(2000);
                }
                catch
                {
                    // The Pipe server must be started in order to send data to it.
                    return;
                }
                // Connected to pipe. Send messages.
                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    while (!token.IsCancellationRequested)
                    {
                        lock (_access)
                        {
                            while (_messageQueue.Count == 0)
                            {
                                Monitor.Wait(_access);
                            }
                            sw.WriteLine(_messageQueue.Dequeue());
                            Monitor.Pulse(_access);
                        }
                    }
                }
            }
        }
    }
}
