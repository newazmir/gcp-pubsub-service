using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Proxy
{
    public interface ISubscriberServiceApiClientProxy
    {
        bool IsEmulated { get; }
        PagedAsyncEnumerable<ListSubscriptionsResponse, Subscription> ListSubscriptionsAsync(ProjectName project);
        Task DeleteSubscriptionAsync(SubscriptionName subscription);
        Task<Subscription> CreateSubscriptionAsync(Subscription request);
        Task<Subscription> GetSubscriptionAsync(SubscriptionName request);
    }
}
