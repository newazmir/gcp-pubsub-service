using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gcp.PubSub.Interfaces.Attributes;
using Google.Cloud.PubSub.V1;

namespace Gcp.PubSub.Interfaces
{
    public interface IPublisherClient
    {
        bool CanCreateTopic { get; }
        string ProjectId { get; }
        Task Publish(string topicId, IPubSubMessageBase response, PubSubMessageType messageType);
        Task Publish(string topicId, IPubSubMessageBase response);
        Task Publish(string topicId, IEnumerable<IPubSubMessageBase> response);
        Task Publish(string topicId, int interval, IEnumerable<IPubSubMessageBase> response);
        Task Delay(int interval);
        Task<IEnumerable<Topic>> GetTopics();
        Task<Topic> CreateTopic(string topicId);
        Task<Topic> GetTopic(string topicId);
    }
}
