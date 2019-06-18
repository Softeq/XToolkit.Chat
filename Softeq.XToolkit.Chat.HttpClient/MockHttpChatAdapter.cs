// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat.HttpClient
{
    public class MockHttpChatAdapter : IHttpChatAdapter
    {
        private const string Avatar1 = "https://cdn.shopify.com/s/files/1/1061/1924/files/Grinning_Emoji_with_Smiling_Eyes.png?9898922749706957214";
        private const string Avatar2 = "https://cdn.shopify.com/s/files/1/1061/1924/products/Smiling_With_Sweat_Emoji_2_03db33ba-4c3b-4e9e-8f29-8bac5b9b9166.png?v=1485577100";
        private const string Avatar3 = "https://www.telegraph.co.uk/content/dam/technology/2017/11/01/silly_trans_NvBQzQNjv4BqqVzuuqpFlyLIwiB6NTmJwfSVWeZ_vEN7c6bHu2jJnT8.png?imwidth=450";
        private const string Img1 = "https://i.pinimg.com/736x/fb/2f/9d/fb2f9dc6f496a25e4e7841fbea6dc130--ocean-wallpapers-beach-iphone-wallpaper.jpg";
        private const string Img2 = "https://www.drivespark.com/img/2017/11/02-1509620655-bajaj-pulsar-ns-200-abs-launch-india-price-images-details-8.jpg";
        private const string Img3 = "https://i.pinimg.com/736x/85/0e/d9/850ed9aa272e199ecbec87774a18dee1--photography-portfolio-hdr-photography.jpg";
        private const string Img4 = "http://www.camotionllc.com/images/BRP15_Horizontal_Bar_15_inch.jpg";
        private const string Img5 = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e6/Noto_Emoji_KitKat_263a.svg/200px-Noto_Emoji_KitKat_263a.svg.png";

        private readonly Subject<ChatMessageModel> _messageReceived = new Subject<ChatMessageModel>();
        private readonly Subject<string> _messageDeleted = new Subject<string>();
        private readonly Subject<string> _messageRead = new Subject<string>();
        private readonly Subject<(string, string)> _messageEdited = new Subject<(string, string)>();
        private readonly Subject<(string, bool)> _isChatMutedChanged = new Subject<(string, bool)>();
        private readonly Subject<(string, int)> _unreadMessageCountChanged = new Subject<(string, int)>();
        private readonly Subject<ChatSummaryModel> _addedToChat = new Subject<ChatSummaryModel>();
        private readonly Subject<string> _kickedFromChat = new Subject<string>();
        private readonly Subject<string> _removedChat = new Subject<string>();

        private readonly Dictionary<string, (string, DateTimeOffset)> _cachedLastMessages
                        = new Dictionary<string, (string, DateTimeOffset)>();

        public MockHttpChatAdapter()
        {
            //RunObservers();
            //SendTestEvents();
        }

        public IObservable<ChatMessageModel> MessageReceived => _messageReceived;

        public IObservable<string> MessageDeleted => _messageDeleted;

        public IObservable<string> MessageRead => _messageRead;

        public IObservable<(string, string)> MessageEdited => _messageEdited;

        public IObservable<(string, bool)> IsChatMutedChanged => _isChatMutedChanged;

        public IObservable<(string, int)> UnreadMessageCountChanged => _unreadMessageCountChanged;

        public IObservable<ChatSummaryModel> AddedToChat => _addedToChat;

        public IObservable<string> KickedFromChat => _kickedFromChat;

        public IObservable<string> RemovedChat => _removedChat;

        public Task<bool> CloseChat(string chatId) => Task.FromResult(true);

        public async Task<ChatSummaryModel> CreateChatAsync(IEnumerable<string> participantsIds)
        {
            var id = Guid.NewGuid().ToString();
            return new ChatSummaryModel
            {
                Id = id,
                PhotoUrl = Avatar1,
                IsMuted = true,
                UnreadMessagesCount = 0,
                Name = "Chat " + id,
                LastMessage = new ChatMessageModel
                {
                    SenderName = "User 3",
                    Body = "test",
                    IsDelivered = true,
                    IsRead = true
                }
            };
        }

        public async Task<IList<ChatSummaryModel>> GetChannelsAsync()
        {
            return new List<ChatSummaryModel>
            {
                new ChatSummaryModel
                {
                    Id = "1",
                    PhotoUrl = Avatar1,
                    IsMuted = false,
                    UnreadMessagesCount = 0,
                    Name = "Chat 1",
                    LastMessage = new ChatMessageModel
                    {
                        SenderName = "User 1",
                        Body = "Help me",
                        IsDelivered = true,
                        IsRead = false
                    }
                },
                new ChatSummaryModel
                {
                    Id = "2",
                    PhotoUrl = Avatar2,
                    IsMuted = false,
                    UnreadMessagesCount = 0,
                    Name = "Chat 2",
                    LastMessage = new ChatMessageModel
                    {
                        SenderName = "User 1",
                        Body = "Don't help me",
                        IsDelivered = false
                    }
                },
                new ChatSummaryModel
                {
                    Id = "3",
                    PhotoUrl = Avatar3,
                    IsMuted = true,
                    UnreadMessagesCount = 1,
                    Name = "Chat 3",
                    LastMessage = new ChatMessageModel
                    {
                        SenderName = "User 2",
                        Body = "Archspire",
                        IsMine = false
                    }
                }
            };
        }

        public async Task<IList<ChatMessageModel>> GetLastReadMessagesAsync(string chatId, string earliestMessageId = null, DateTimeOffset? earliestMessageDateTime = null)
        {
            return null;
        }

        public async Task<IList<ChatMessageModel>> GetUnreadMessagesAsync(string chatId, string latestMessageId = null, DateTimeOffset? latestMessageDateTime = null)
        {
            return null;
        }

        public async Task MarkMessageAsReadAsync(string chatId, string messageId)
        {

        }

        public Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageSize, int pageNumber)
        {
            throw new NotImplementedException();
        }

        public Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(string chatId, string nameFilter, int pageSize, int pageNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId)
        {
            if (chatId == "1")
            {
                return new List<ChatMessageModel>
                {
                    new ChatMessageModel
                    {
                        Id = "1",
                        DateTime = DateTimeOffset.Now.AddYears(-5),
                        SenderName = "User 2",
                        Body = "I’m excited about this trip this should be good. Haven’t been away in a while. I know it’s for work but still :)",
                        SenderPhotoUrl = Avatar1,
                        ImageRemoteUrl = Img4
                    },
                    new ChatMessageModel
                    {
                        Id = "2",
                        DateTime = DateTimeOffset.Now.AddYears(-1),
                        SenderName = "User 1",
                        Body = "You at the airport yet?",
                        SenderPhotoUrl = Avatar2,
                        ImageRemoteUrl = Img2
                    },
                    new ChatMessageModel
                    {
                        Id = "3",
                        DateTime = DateTimeOffset.Now.AddDays(-5),
                        SenderName = "User 1",
                        Body = "I’m in traffic. Wondering if we have to go through is customs in Toronto or San Francisco.",
                        SenderPhotoUrl = Avatar2,
                        ImageRemoteUrl = Img3
                    },
                    new ChatMessageModel
                    {
                        Id = "1",
                        DateTime = DateTimeOffset.Now.AddDays(-2),
                        SenderName = "User 2",
                        Body = "Customs in Toronto",
                        SenderPhotoUrl = Avatar1,
                        ImageRemoteUrl = Img4
                    },
                    new ChatMessageModel
                    {
                        Id = "2",
                        DateTime = DateTimeOffset.Now.AddDays(-1),
                        SenderName = "User 1",
                        Body = "Hey :)",
                        SenderPhotoUrl = Avatar2,
                        ImageRemoteUrl = Img5
                    }
                };
            }
            if (chatId == "2")
            {
                return new List<ChatMessageModel>
                {
                    new ChatMessageModel
                    {
                        Id = "2",
                        SenderName = "User 2",
                        Body = "test 2",
                        SenderPhotoUrl = Avatar2,
                    },
                    new ChatMessageModel
                    {
                        Id = "3",
                        SenderName = "User 1",
                        Body = "Don't help me",
                        SenderPhotoUrl = Avatar3,
                    }
                };
            }
            if (chatId == "3")
            {
                return new List<ChatMessageModel>
                {
                    new ChatMessageModel
                    {
                        Id = "3",
                        SenderName = "User 1",
                        Body = "test 3",
                        SenderPhotoUrl = Avatar3,
                    },
                    new ChatMessageModel
                    {
                        Id = "1",
                        SenderName = "User 2",
                        Body = "Archspire",
                        SenderPhotoUrl = Avatar1,
                    }
                };
            }
            return new List<ChatMessageModel>();
        }

        public Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody)
        {
            return Task.FromResult(new ChatMessageModel { Body = messageBody, Id = chatId, DateTime = DateTimeOffset.Now });
        }

        private async void RunObservers()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async void SendTestEvents()
        {
            await Task.Delay(3000);
            _messageReceived.OnNext(new ChatMessageModel
            {
                Id = "1",
                DateTime = DateTimeOffset.Now,
                ChannelId = "1",
                Body = "Test msg1",
                SenderPhotoUrl = Avatar3,
            });
            var delay = 1000;
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(delay);
                _messageReceived.OnNext(new ChatMessageModel
                {
                    Id = "2",
                    ChannelId = "1",
                    Body = "Test msg2",
                    SenderPhotoUrl = Avatar2,
                    DateTime = DateTimeOffset.Now.AddDays(-i),
                });
                _unreadMessageCountChanged.OnNext(("2", 1));
            }
            await Task.Delay(delay);
            _messageRead.OnNext("2");
            await Task.Delay(delay);
            _messageEdited.OnNext(("1", "Test msg1 edited"));
            await Task.Delay(delay);
            _messageEdited.OnNext(("2", "Test msg2 edited"));
        }

        public Task<ChatUserModel> GetUserSummaryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ChatMessageModel>> GetOlderMessagesAsync(string chatId, string messageFromId = null, DateTimeOffset? messageFromDateTime = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId, string messageFromId, DateTimeOffset messageFromDateTime, int? count = null)
        {
            throw new NotImplementedException();
        }

        public Task MuteChatAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public Task UnMuteChatAsync(string chatId)
        {
            throw new NotImplementedException();
        }

        public Task<ChatSummaryModel> CreateDirectChatAsync(string memberId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubscribeForPushNotificationsAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnsubscribeFromPushNotificationsAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
