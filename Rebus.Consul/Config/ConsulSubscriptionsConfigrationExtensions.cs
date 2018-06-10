using Rebus.Consul;
using Rebus.Subscriptions;

namespace Rebus.Config
{
    /// <summary>
    /// 
    /// </summary>
    public static class ConsulSubscriptionsConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurer"></param>
        /// <param name="consulAddress"></param>
        /// <param name="baseKey"></param>
        public static void StoreInConsule(this StandardConfigurer<ISubscriptionStorage> configurer,
            string consulAddress, string baseKey)
        {
            StoreInConsul(configurer, consulAddress, baseKey, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurer"></param>
        /// <param name="consulAddress"></param>
        /// <param name="baseKey"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="dataCenter"></param>
        public static void StoreInConsul(this StandardConfigurer<ISubscriptionStorage> configurer,
            string consulAddress, string baseKey, string userName, string password, string dataCenter = "dc1")
        {
            configurer.Register(c => {
                var subscriptionStorage = new ConsulSubscriptionStorage(consulAddress, baseKey, userName, password, dataCenter);
                return subscriptionStorage;
            });
        }
    }
}