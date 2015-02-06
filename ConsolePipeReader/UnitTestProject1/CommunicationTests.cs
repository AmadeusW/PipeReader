using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipeCommunication;
using System.Collections.Generic;
using System.Threading;

namespace UnitTestProject1
{
    [TestClass]
    public class CommunicationTests
    {
        [TestMethod]
        public void TestSimpleCommunication()
        {
            string pipeName = "test";
            string testMessage = "Sample Text";

            Queue<string> receivedQueue = new Queue<string>();
            PipeHost.CreateBackgroundPipeHost(pipeName, receivedQueue);
            var client = new PipeClient(pipeName);
            client.RunClient();
            client.SendMessage(testMessage);

            Thread.Sleep(100);
            if (receivedQueue.Count > 0)
            {
                var receivedMessage = receivedQueue.Dequeue();
                Assert.AreEqual(testMessage, receivedMessage);
            }
            else
            {
                Assert.Fail("No data available in the receiving queue");
            }
        }

        [TestMethod]
        public void TestHeavyCommunication()
        {
            string pipeName = "test";
            string testMessage = "Sample Text";

            Queue<string> receivedQueue = new Queue<string>();
            PipeHost.CreateBackgroundPipeHost(pipeName, receivedQueue);
            var client = new PipeClient(pipeName);
            client.RunClient();
            for (int testId = 0; testId < 4; testId++)
            {
                client.SendMessage(testMessage);
            }

            for (int testId = 0; testId < 4; testId++)
            {
                Thread.Sleep(100);
                if (receivedQueue.Count > 0)
                {
                    var receivedMessage = receivedQueue.Dequeue();
                    Assert.AreEqual(testMessage, receivedMessage);
                    continue;
                }
                else
                {
                    Assert.Fail("No data available in the receiving queue");
                }
            }
        }
    }
}
