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

            int tries = 10;
            while (tries-- > 0)
            {
                if (receivedQueue.Count > 0)
                {
                    var receivedMessage = receivedQueue.Dequeue();
                    Assert.AreEqual(testMessage, receivedMessage);
                    break;
                }
                Thread.Sleep(10);
            }
            Assert.Fail("No data available in the receiving queue");
        }

        public void TestHeavyCommunication()
        {

        }
    }
}
