using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Rebus.Consul.Tests
{
    [TestFixture]
    public class ConsulSubscriptionStorageExploratoryTests
    {
        private ConsulSubscriptionStorage _subscriptionStorage;
        private string _topicName;
        private string _subscriber1Address = "queue1@machine";
        private string _subscriber2Address = "queue1@other-machine";

        [Test]
        public async Task FirstTest()
        {
            // Assert that the topic has no subscribers (Topic name is a randomly generated guid).
            var addresses = await _subscriptionStorage.GetSubscriberAddresses(_topicName);
            Assert.IsEmpty(addresses);

            // Register subscriber #1
            await _subscriptionStorage.RegisterSubscriber(_topicName, _subscriber1Address);
            addresses = await _subscriptionStorage.GetSubscriberAddresses(_topicName);
            CollectionAssert.AreEquivalent(new [] {_subscriber1Address}, addresses);

            // Register subscriber #2
            await _subscriptionStorage.RegisterSubscriber(_topicName, _subscriber2Address);
            addresses = await _subscriptionStorage.GetSubscriberAddresses(_topicName);
            CollectionAssert.AreEquivalent(new[] { _subscriber1Address, _subscriber2Address }, addresses);

            // Unregister subscriber #1 & #2
            await _subscriptionStorage.UnregisterSubscriber(_topicName, _subscriber1Address);
            await _subscriptionStorage.UnregisterSubscriber(_topicName, _subscriber2Address);
            addresses = await _subscriptionStorage.GetSubscriberAddresses(_topicName);
            Assert.IsEmpty(addresses);
        }

        [SetUp]
        public void Setup()
        {
            _topicName = Guid.NewGuid().ToString("D");
            _subscriptionStorage = new ConsulSubscriptionStorage("http://localhost:8500", "bus-topics", null, null, "dc1");
        }

        [TearDown]
        public void Teardown()
        {
            _subscriptionStorage.RemoveTopicKey(_topicName);
            _subscriptionStorage.Dispose();
        }
    }
}
