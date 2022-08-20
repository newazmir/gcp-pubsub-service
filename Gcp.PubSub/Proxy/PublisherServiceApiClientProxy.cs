using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gcp.PubSub.Common;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Gcp.PubSub.Proxy
{
    public class PublisherServiceApiClientProxy : IPublisherServiceApiClientProxy
    {
        public bool IsEmulated { get; private set; }

        private readonly ILogger<PublisherServiceApiClientProxy> _logger;
        private readonly PublisherServiceApiClient _serviceApiClient;
        private readonly LogScopeFactory _logScopeFactory;

        private PublisherServiceApiClientProxy(ILogger<PublisherServiceApiClientProxy> logger,
            PublisherServiceApiClient serviceApiClient, LogScopeFactory logScopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceApiClient = serviceApiClient ?? throw new ArgumentNullException(nameof(serviceApiClient));
            _logScopeFactory = logScopeFactory ?? throw new ArgumentNullException(nameof(logScopeFactory));
        }

        public static async Task<PublisherServiceApiClientProxy> Create(ILoggerFactory loggerFactory,
            LogScopeFactory logScopeFactory, CancellationToken token)
        {
            var publisher = await PublisherServiceApiClient.CreateAsync(token);
            var logger = loggerFactory.CreateLogger<PublisherServiceApiClientProxy>();
            var proxy = new PublisherServiceApiClientProxy(logger, publisher, logScopeFactory) { IsEmulated = false };

            return proxy;
        }

        public static async Task<PublisherServiceApiClientProxy> CreateEmulated(string host,
            ILoggerFactory loggerFactory, LogScopeFactory factory, CancellationToken token)
        {
            var publisher = await new PublisherServiceApiClientBuilder
            {
                Endpoint = host,
                ChannelCredentials = ChannelCredentials.Insecure
            }.BuildAsync(token);

            var logger = loggerFactory.CreateLogger<PublisherServiceApiClientProxy>();
            return new PublisherServiceApiClientProxy(logger, publisher, factory) { IsEmulated = true };
        }

        public Task<PublishResponse> PublishAsync(TopicName topic, IEnumerable<PubsubMessage> messages) => _serviceApiClient.PublishAsync(topic, messages);

        public async Task<int> PublishBatchAsync(TopicName topic, IEnumerable<PubsubMessage> messages)
        {
            var pubsubMessages = messages as PubsubMessage[] ?? messages.ToArray();
            using (_logScopeFactory.CreateScope(_logger, null, $"sending in bulk {pubsubMessages.Length}"))
            {
                await _serviceApiClient.PublishAsync(topic, pubsubMessages);
            }
            return pubsubMessages.Length;
        }
        public PagedAsyncEnumerable<ListTopicsResponse, Topic> ListTopicsAsync(ProjectName projectName) => _serviceApiClient.ListTopicsAsync(projectName);

        public Task<Topic> GetTopicAsync(TopicName topic) => _serviceApiClient.GetTopicAsync(topic);

        public Task<Topic> CreateTopicAsync(TopicName topic) => _serviceApiClient.CreateTopicAsync(topic);
    }
}
