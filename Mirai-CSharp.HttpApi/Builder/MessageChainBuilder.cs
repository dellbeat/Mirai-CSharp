using System;
using Mirai.CSharp.HttpApi.Models.ChatMessages;
using ISharedChatMessage = Mirai.CSharp.Models.ChatMessages.IChatMessage;
using ISharedMessageChainBuilder = Mirai.CSharp.Builders.IMessageChainBuilder;
using ISharedPokeType = Mirai.CSharp.Models.PokeType;
using SharedMessageChainBuilder = Mirai.CSharp.Builders.MessageChainBuilder;

namespace Mirai.CSharp.HttpApi.Builders
{
    public interface IMessageChainBuilder : ISharedMessageChainBuilder
    {
        
    }

    public class MessageChainBuilder : SharedMessageChainBuilder, IMessageChainBuilder
    {
        public override ISharedMessageChainBuilder Add(ISharedChatMessage message)
        {
            if (message is not IChatMessage)
            {
                throw new InvalidOperationException($"添加的消息实例 {message.GetType().FullName} 不实现 {typeof(IChatMessage).FullName}");
            }
            return base.Add(message);
        }

        public override ISharedMessageChainBuilder AddPlainMessage(string text)
        {
            Add(new PlainMessage(text));
            return this;
        }

        public override ISharedMessageChainBuilder AddImageMessage(string? imageId = null, string? url = null, string? path = null)
        {
            Add(new ImageMessage(imageId, url, path));
            return this;
        }

        public override ISharedMessageChainBuilder AddFlashImageMessage(string? imageId = null, string? url = null, string? path = null)
        {
            Add(new FlashImageMessage(imageId, url, path));
            return this;
        }

        public override ISharedMessageChainBuilder AddAtMessage(long target)
        {
            Add(new AtMessage(target));
            return this;
        }

        public override ISharedMessageChainBuilder AddAtAllMessage()
        {
            Add(new AtAllMessage());
            return this;
        }

        public override ISharedMessageChainBuilder AddFaceMessage(int id, string? name = null)
        {
            Add(new FaceMessage(id, name));
            return this;
        }

        public override ISharedMessageChainBuilder AddXmlMessage(string xml)
        {
            Add(new XmlMessage(xml));
            return this;
        }

        public override ISharedMessageChainBuilder AddJsonMessage(string json)
        {
            Add(new JsonMessage(json));
            return this;
        }

        public override ISharedMessageChainBuilder AddAppMessage(string content)
        {
            Add(new AppMessage(content));
            return this;
        }

        public override ISharedMessageChainBuilder AddPokeMessage(ISharedPokeType name)
        {
            Add(new PokeMessage(name));
            return this;
        }

        public override ISharedMessageChainBuilder AddVoiceMessage(string? voiceId = null, string? url = null, string? path = null)
        {
            Add(new VoiceMessage(voiceId, url, path));
            return this;
        }
    }

    public class ImmutableMessageChainBuilder : MessageChainBuilder
    {
        protected ISharedChatMessage[]? _builtMessages;

        public override ISharedChatMessage[] Build()
        {
            if (_builtMessages == null)
            {
                _builtMessages = _list.ToArray();
                _list.Clear();
                _list.Capacity = 0;
            }
            return _builtMessages;
        }

        public override ISharedMessageChainBuilder Add(ISharedChatMessage message)
        {
            if (_builtMessages != null)
            {
                throw new InvalidOperationException("由于之前进行过构建操作, 无法添加新的消息实例");
            }
            return base.Add(message);
        }
    }
}
