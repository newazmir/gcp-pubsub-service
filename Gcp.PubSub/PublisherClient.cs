using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gcp.PubSub.Common;
using Gcp.PubSub.Interfaces;
using Gcp.PubSub.Interfaces.Attributes;
using Gcp.PubSub.Proxy;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace Gcp.PubSub
{
    public class PublisherClient : IPublisherClient
    {
        public bool CanCreateTopic { get; }
        public string? ProjectId { get; }

        private readonly ILogger? _logger;
        private readonly IJsonSerializer? _jsonSerializer;
        private readonly ProjectName? _projectName;
        private readonly IPublisherServiceApiClientProxy? _publisherProxy;
        private readonly LogScopeFactory? _logScopeFactory;
        private readonly AsyncRetryPolicy? _retryPolicy;

        public Task Publish(string topicId, IPubSubMessageBase response, PubSubMessageType messageType) => _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await PublishInternal(topicId, response, messageType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Can't publish to {topicId}");
                throw;
            }
        });

        public Task Publish(string topicId, IPubSubMessageBase response)
        {
            throw new NotImplementedException();
        }

        public Task Publish(string topicId, IEnumerable<IPubSubMessageBase> response)
        {
            throw new NotImplementedException();
        }

        public async Task Publish(string topicId, int interval, IEnumerable<IPubSubMessageBase> response)
        {
            if (topicId == null)
            {
                throw new ArgumentNullException(nameof(topicId));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var topicName = TopicName.FromProjectTopic(ProjectId, topicId);
            var publishedMessageCount = 0;
            var subMessageBases = response as IPubSubMessageBase[] ?? response.ToArray();
            if (subMessageBases.Length == 0)
            {
                return;
            }

            try
            {
                foreach (var message in subMessageBases)
                {
                    await _publisherProxy.PublishAsync(topicName, new[] { message }.Select(CreatePubSubMessage));
                    publishedMessageCount++;
                    await Delay(interval);
                }
            }
            catch (RpcException rpcException) when (rpcException.Status.StatusCode == StatusCode.NotFound)
            {
                if (CanCreateTopic)
                {
                    try
                    {
                        _logger.LogInformation($"Started creating topic {topicName}");
                        await _publisherProxy.CreateTopicAsync(topicName);
                        _logger.LogInformation($"Created topic {topicName}");
                    }
                    catch (RpcException rpc) when (rpc.Status.StatusCode ==
                                                   StatusCode.AlreadyExists)
                    {
                        _logger.LogWarning($"Racing topic creation {topicName}. Should never happen at production.", rpc);
                    }


                    foreach (var message in subMessageBases)
                    {
                        await _publisherProxy.PublishAsync(topicName, new[] { message }.Select(CreatePubSubMessage));
                    }
                    await Task.Delay(interval);
                }
                else
                {
                    throw;
                }
            }

            _logger.LogInformation($"Published {publishedMessageCount} messages");
        }

        public Task Delay(int interval) => Task.Delay(interval);

        public async Task<IEnumerable<Topic>> GetTopics()
        {
            IEnumerable<Topic>? topics = null;

            try
            {
                topics = await _publisherProxy.ListTopicsAsync(_projectName).ReadPageAsync(1000);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.NotFound)
            {
                _logger.LogError(e, "Topics do not exist.");
            }
            catch (RpcException unavailableException) when (unavailableException.Status.StatusCode ==
                                                            StatusCode.Unavailable)
            {
                _logger.LogError(unavailableException, $"Cannot reach server");
                throw;
            }

            return topics;
        }

        public async Task<Topic> CreateTopic(string topicId)
        {
            if (!CanCreateTopic)
            {
                throw new InvalidOperationException($"CanCreateTopic is false, error creating new topic {topicId}");
            }

            using (_logScopeFactory.CreateScope(_logger, null, $"create new topic {ProjectId}/{topicId}"))
            {
                var topicName = TopicName.FromProjectTopic(ProjectId, topicId);
                Topic? topic = null;

                try
                {
                    topic = await _publisherProxy.GetTopicAsync(topicName);
                    _logger.LogInformation($"Topic {topic} already exists");
                }
                catch (RpcException e) when (e.Status.StatusCode == StatusCode.NotFound)
                {
                    try
                    {
                        topic = await _publisherProxy.CreateTopicAsync(topicName);
                        _logger.LogInformation($"Topic {topic.Name} created.");
                    }
                    catch (RpcException rpc) when (rpc.Status.StatusCode ==
                                                   StatusCode.AlreadyExists)
                    {
                        _logger.LogWarning($"Racing topic creation {topicName}. Should never happen at production.", rpc);
                        throw;
                    }
                    catch (Exception)
                    {
                        _logger.LogError($"Cannot create topic {topic}.");
                        throw;
                    }
                }
                catch (RpcException unavailableException) when (unavailableException.Status.StatusCode ==
                                                                StatusCode.Unavailable)
                {
                    _logger.LogError($"Cannot reach server");
                    throw;
                }

                return topic;
            }
        }

        public async Task<Topic> GetTopic(string topicId)
        {
            var topicName = TopicName.FromProjectTopic(_projectName.ProjectId, topicId);
            Topic? topic = null;

            try
            {
                topic = await _publisherProxy.GetTopicAsync(topicName);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.NotFound)
            {
                _logger.LogError($"Topic {topicId} does not exist.");
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.InvalidArgument)
            {
                _logger.LogError($"Topic {topicId} is not a valid topic name");
            }
            catch (RpcException unavailableException) when (unavailableException.Status.StatusCode ==
                                                            StatusCode.Unavailable)
            {
                _logger.LogError("Cannot reach server");
                throw;
            }

            return topic;
        }
        private PubsubMessage CreatePubSubMessage(IPubSubMessageBase obj) => CreatePubSubMessage(obj, null);
        private PubsubMessage CreatePubSubMessage(IPubSubMessageBase obj, PubSubMessageType pubSubMessageType)
        {
            if (pubSubMessageType == null &&
                Attribute.GetCustomAttribute(obj.GetType(), typeof(PubSubMessageTypeAttribute)) is
                    PubSubMessageTypeAttribute pubSubMessageTypeAttribute)
            {
                pubSubMessageType = pubSubMessageTypeAttribute.MessageType;
            }

            if (pubSubMessageType != null)
            {
                var messageType = pubSubMessageType.Type;
                var version = pubSubMessageType.Version;

                var message = new PubsubMessage
                {
                    // The data is any arbitrary ByteString. Here, we're using text.
                    Data = ByteString.CopyFromUtf8(_jsonSerializer.Serialize(obj)),
                    // The attributes provide metadata in a string-to-string dictionary.
                    Attributes =
                    {
                        {"messageType", messageType},
                    }
                };

                AddAttributes(obj, message);

                if (version.IsNotEmpty())
                {
                    message.Attributes["messageVersion"] = version;
                }

                if (!string.IsNullOrEmpty(obj.CorrelationId))
                {
                    message.Attributes["correlationId"] = obj.CorrelationId;
                }

                if (obj is IPubSubResponse pubSubResponse)
                {
                    message.Attributes["statusCode"] = ((int)pubSubResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
                }

                return message;
            }

            throw new NotImplementedException(
                $"{obj.GetType()} should be decorated by {nameof(PubSubMessageTypeAttribute)} attribute");
        }

        private static void AddAttributes(IPubSubMessageBase request, PubsubMessage message)
        {
            var props = from p in request.GetType().GetProperties()
                        let attr = p.GetCustomAttributes(typeof(PubSubAttributeNameAttribute), true)
                        where attr.Length == 1
                        select new { Property = p, Attribute = attr.First() as PubSubAttributeNameAttribute };

            foreach (var prop in props)
            {
                if (prop.Property.PropertyType.IsValueType ||
                    prop.Property.PropertyType == typeof(string))
                {
                    var val = prop.Property.GetValue(request);
                    if (val != null)
                    {
                        message.Attributes[prop.Attribute.Name] = val.ToString();
                    }
                }
                else
                {
                    throw new NotSupportedException($"{prop.Property.Name} is not a value type.");
                }
            }
        }
        private async Task PublishInternal(string topicId, IPubSubMessageBase obj, PubSubMessageType pubSubMessageType)
        {
            if (topicId == null)
            {
                throw new ArgumentNullException(nameof(topicId));
            }

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var topicName = TopicName.FromProjectTopic(ProjectId, topicId);

            var message = CreatePubSubMessage(obj, pubSubMessageType);
            using (_logScopeFactory.CreateScope(_logger, obj, $"publishing message {pubSubMessageType}"))
            {
                try
                {
                    var messageResponse = await _publisherProxy.PublishAsync(topicName, new[] { message });
                    _logger.LogDebug(
                        $"Message {pubSubMessageType} queued to {topicName} with MessageId: {messageResponse.MessageIds[0]}");
                }
                catch (RpcException rpcException) when (rpcException.Status.StatusCode ==
                                                        StatusCode.NotFound)
                {
                    _logger.LogError($"Can't publish message to {topicName}, canCreatTopic={CanCreateTopic}", rpcException);
                    if (CanCreateTopic)
                    {
                        try
                        {
                            _logger.LogInformation($"Started creating topic {topicName}");
                            await _publisherProxy.CreateTopicAsync(topicName);
                            _logger.LogInformation($"Created topic {topicName}");
                        }
                        catch (RpcException rpc) when (rpc.Status.StatusCode ==
                                                       StatusCode.AlreadyExists)
                        {
                            _logger.LogWarning($"Racing topic creation {topicName}. Should never happen at production.", rpc);
                        }

                        var messageResponse = await _publisherProxy.PublishAsync(topicName, new[] { message });
                        _logger.LogDebug(
                            $"Message {pubSubMessageType} published to {topicName} with MessageId: {messageResponse.MessageIds[0]}");
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (RpcException rpcException)
                {
                    _logger.LogError($"Can't publish message to {ProjectId}/{topicName}", rpcException);
                    throw;
                }
            }
        }
    }
}
