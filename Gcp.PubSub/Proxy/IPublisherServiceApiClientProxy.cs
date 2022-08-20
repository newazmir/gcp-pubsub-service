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
    public interface IPublisherServiceApiClientProxy
    {
        bool IsEmulated { get; }
        Task<PublishResponse> PublishAsync(TopicName topic, IEnumerable<PubsubMessage> messages);
        Task<int> PublishBatchAsync(TopicName topic, IEnumerable<PubsubMessage> messages);
        PagedAsyncEnumerable<ListTopicsResponse, Topic> ListTopicsAsync(ProjectName projectName);
        Task<Topic> GetTopicAsync(TopicName topic);
        Task<Topic> CreateTopicAsync(TopicName topic);
    }
}
