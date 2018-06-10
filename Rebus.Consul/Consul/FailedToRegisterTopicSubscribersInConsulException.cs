using System;
using System.Net;

namespace Rebus.Consul
{
    public class FailedToRegisterTopicSubscribersInConsulException : Exception
    {
        public string Topic { get; }
        public string SubscriberAddress { get; }
        public HttpStatusCode ResultStatusCode { get; }

        public FailedToRegisterTopicSubscribersInConsulException(string topic, string subscriberAddress, HttpStatusCode resultStatusCode)
        {
            Topic = topic;
            SubscriberAddress = subscriberAddress;
            ResultStatusCode = resultStatusCode;
        }
    }
}