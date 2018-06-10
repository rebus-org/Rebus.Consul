using System;
using System.Net;

namespace Rebus.Consul
{
    public class FailedToGetSubscriberAddressesFromConsulException : Exception
    {
        public string Topic { get; }
        public HttpStatusCode ResultStatusCode { get; }

        public FailedToGetSubscriberAddressesFromConsulException(string topic, HttpStatusCode resultStatusCode)
        {
            Topic = topic;
            ResultStatusCode = resultStatusCode;
        }
    }
}