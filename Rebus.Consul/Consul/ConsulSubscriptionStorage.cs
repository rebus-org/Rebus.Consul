using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rebus.Subscriptions;
using Consul;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rebus.Consul
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsulSubscriptionStorage : ISubscriptionStorage, IDisposable
    {
        private readonly string _baseKey;
        private readonly ConsulClient _client;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consulAddress"></param>
        /// <param name="baseKey"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="dataCenter"></param>
        /// <param name="serializerSettings"></param>
        public ConsulSubscriptionStorage(string consulAddress, string baseKey, string userName, string password, string dataCenter, JsonSerializerSettings serializerSettings = null)
        {
            if (consulAddress == null) throw new ArgumentNullException(nameof(consulAddress));
            if (dataCenter == null) throw new ArgumentNullException(nameof(dataCenter));
            _baseKey = baseKey;

            _serializerSettings = serializerSettings ?? new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _client = new ConsulClient(
                config =>
                {
                    config.Address = new Uri(consulAddress);
                }, 
                client => { },
                handler =>
                {
                    if (userName != null && password != null)
                    {
                        handler.Credentials = new NetworkCredential
                        {
                            UserName = userName,
                            Password = password
                        };
                    }
                });
        }


        /// <inheritdoc />
        public async Task<string[]> GetSubscriberAddresses(string topic)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            var key = CreateKey(topic);
            var result = await _client.KV.Get(key).ConfigureAwait(false);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return new string[] { };
            }
            if (result.Response == null) throw new FailedToGetSubscriberAddressesFromConsulException(topic, result.StatusCode);

            return DeserializeFromKVPair(result.Response);
        }

        /// <inheritdoc />
        public Task RegisterSubscriber(string topic, string subscriberAddress)
        {
            return MutateAndPut(topic, subscriberAddress, Enumerable.Contains,  Enumerable.Concat);
        }

        /// <inheritdoc />
        public Task UnregisterSubscriber(string topic, string subscriberAddress)
        {
            return MutateAndPut(topic, subscriberAddress, (xs, x) => !xs.Contains(x), Enumerable.Except);
        }

        public Task RemoveTopicKey(string topic)
        {
            return _client.KV.Delete(CreateKey(topic));
        }

        private async Task MutateAndPut(string topic, 
                                        string subscriberAddress,
                                        Func<IEnumerable<string>, string, bool> presenceChecker, Func<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> mutator)
        {
            var addresses = await GetSubscriberAddresses(topic);
            if (presenceChecker(addresses, subscriberAddress)) return;
            var pair = SerializeIntoKVPair(topic, mutator(addresses, new [] { subscriberAddress }));
            var result = await _client.KV.Put(pair).ConfigureAwait(false);
            if (!result.Response)
            {
                throw new FailedToRegisterTopicSubscribersInConsulException(topic, subscriberAddress, result.StatusCode);
            }
        }

        // ReSharper disable once InconsistentNaming

        private KVPair SerializeIntoKVPair(string topic, IEnumerable<string> addresses)
        {
            if (addresses == null) throw new ArgumentNullException(nameof(addresses));
            var topicSubscribers = new TopicSubscribers
            {
                Addresses = addresses.ToArray()
            };
            var jsonAsString = JsonConvert.SerializeObject(topicSubscribers, _serializerSettings);
            var bytes = Encoding.UTF8.GetBytes(jsonAsString);
            return new KVPair(CreateKey(topic))
            {
                Value = bytes
            };
        }

        // ReSharper disable once InconsistentNaming

        private string[] DeserializeFromKVPair(KVPair pair)
        {
            if (pair == null) throw new ArgumentNullException(nameof(pair));
            var jsonAsString = Encoding.UTF8.GetString(pair.Value);
            var topicSubscribers = JsonConvert.DeserializeObject<TopicSubscribers>(jsonAsString, _serializerSettings);
            return topicSubscribers.Addresses;
        }

        private string CreateKey(string topic)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            return $"{_baseKey}/{topic}";
        }

        public bool IsCentralized { get; } = true;

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class TopicSubscribers
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "addresses")]
        public string[] Addresses { get; set; }
    }
}