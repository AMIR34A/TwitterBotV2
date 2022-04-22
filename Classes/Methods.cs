using DataLayer;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tweetinvi;
using Tweetinvi.Models;
namespace TwitterBotV2.Classes
{
    internal class Methods
    {
        TwitterClient userClient = new TwitterClient("zAra0T5tcxYqoZzGbzNn8zB1h", "2VUh6nBQO2OX7GHDMPLWaxmRziXrZyjaUWOn0COVtIP6MxoV8O", "1297246709253394432-HZd5WrvERipQyVNwcmsYS7kGrikeje", "FhNO7jinFZmHKPyJoRLnAsDXmK8QWK9VF6NwnSz38myXO");

        #region ResponseToText
        public async Task ResponseToStartAsync(TelegramBotClient botClient, Update update)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.Message.Chat.Id);
                if (user == null)
                {
                    twitterDb.Users.Add(new DataLayer.Models.User
                    {
                        ChatId = update.Message.Chat.Id,
                        FirstName = update.Message.Chat.FirstName,
                        LastName = update.Message.Chat.LastName,
                        Username = update.Message.Chat.Username,
                        Step = 0,
                        LastUsing = DateTime.Now
                    });
                    twitterDb.Informations.Add(new DataLayer.Models.Information
                    {
                        ChatIdChannel = 0,
                    });
                }
                else
                {
                    user.FirstName = update.Message.Chat.FirstName;
                    user.LastName = update.Message.Chat.LastName;
                    user.Username = update.Message.Chat.Username;
                    user.Step = 0;
                    user.LastUsing = DateTime.Now;
                }

                await twitterDb.SaveChangesAsync();
            }

            stringBuilder.AppendLine("Hello dear friend👋");
            stringBuilder.AppendLine("🔗Please Send your tweet id to me");
            stringBuilder.AppendLine("<b>📌For example : https://twitter.com/TwitterLive/status/925770404068601856 </b>");

            var keyboard = new KeyboardButton[][]
            {
                      new[] { new KeyboardButton("⚙️Settings") },
            };
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, stringBuilder.ToString(),
                  ParseMode.Html, null, true, false, null, null, new ReplyKeyboardMarkup(keyboard) { ResizeKeyboard = true });
        }

        public async Task ResponseToSettingsAsync(TelegramBotClient botClient, Update update)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.Message.Chat.Id);
                var information = await twitterDb.Informations.FindAsync(user.Id);
                stringBuilder.AppendLine($"<b>⛓The ChatID of my channel </b>: <code>{information.ChatIdChannel}</code>");
                stringBuilder.AppendLine("<b>📄The subtitle for send tweet in my channel </b> : ");
                stringBuilder.AppendLine(information.Description);
            }

            var inlineKeyboard = new InlineKeyboardButton[][]
            {
                  new InlineKeyboardButton[]
                  {
                      InlineKeyboardButton.WithCallbackData("🖋Subtitle","SetSubtitle") , InlineKeyboardButton.WithCallbackData("🛠ChatId Channel","SetChatIdChannel")
                  }
            };
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, stringBuilder.ToString(),
                  ParseMode.Html, null, true, false, null, null, new InlineKeyboardMarkup(inlineKeyboard));
        }

        public async Task ResponseToChatIdChannelAsync(TelegramBotClient botClient, Update update)
        {
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.Message.Chat.Id);

                var chatIdChannel = update.Message.Text;
                if (chatIdChannel.StartsWith('-'))
                {
                    var isDigit = long.TryParse(chatIdChannel, out long channelId);
                    if (isDigit)
                    {
                        var information = await twitterDb.Informations.FindAsync(user.Id);
                        information.ChatIdChannel = channelId;
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "✅<b>The ChatID of your channel was setted</b>", ParseMode.Html);
                    }
                }
                user.Step = 0;
                await twitterDb.SaveChangesAsync();
            }
        }
        public async Task ResponceToSubtitleTweetAsync(TelegramBotClient botClient, Update update)
        {
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.Message.Chat.Id);
                var information = await twitterDb.Informations.FindAsync(user.Id);
                information.Description = update.Message.Text;
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "✅<b>The subtitle was setted</b>", ParseMode.Html);
                user.Step = UserStep.MainMenu;
                await twitterDb.SaveChangesAsync();
            }
        }
        public async Task ResponseToGetTweetAsync(string url, TelegramBotClient botClient, Update update)
        {
            try
            {
                var tweetID = await ConvertURLToTweetID(url);
                var tweet = await userClient.Tweets.GetTweetAsync(tweetID);

                var keyboard = new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithUrl($"🔄{tweet.RetweetCount}",$"https://twitter.com/intent/retweet?tweet_id={tweetID}") , InlineKeyboardButton.WithUrl($"❤️{tweet.FavoriteCount}",$"https://twitter.com/intent/like?tweet_id={tweetID}")
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithUrl($"{tweet.CreatedBy.Name}",$"https://twitter.com/intent/follow?screen_name={tweet.CreatedBy.ScreenName}")
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("📤Send to Your Channel","SendToChannel")
                    }
                };

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, tweet.Text, ParseMode.Html,
                                null, true, false, update.Message.MessageId, null, new InlineKeyboardMarkup(keyboard));
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "<b>Sorry❗️😢\nI couldn't find the tweet</b>", ParseMode.Html,
                                null, true, false, update.Message.MessageId);
            }
        }

        public async Task ResponseToGetTrendsAsync(TelegramBotClient botClient, Update update, double latitude = 37.0902, double longitude = -95.7129, int count = 5)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var medals = new[] { "🥇", "🥈", "🥉" };
            var coordinate = update.Message.Text.Split(' ');

            var coordinates = new Coordinates(latitude, longitude);
            var trendingLocations = await userClient.Trends.GetTrendsLocationCloseToAsync(coordinates);
            var trends = await userClient.Trends.GetPlaceTrendsAtAsync(trendingLocations[0].WoeId);
            var results = trends.Trends.Where((tweet, counter) => counter <= count - 1).Select(tweet => tweet).ToList();

            stringBuilder.AppendLine($"<b>🗺Location :</b> <code>{trendingLocations[0].Country}</code>");
            stringBuilder.AppendLine();
            for (int i = 0; i < results.Count; i++)
            {
                stringBuilder.AppendFormat("{0}{1}\n", (i < 3) ? medals[i] : "•", results[i].Name);
                if (results[i].TweetVolume.HasValue)
                    stringBuilder.AppendLine($"<b>🎉{results[i].TweetVolume.Value}</b> Tweets");

                stringBuilder.AppendLine();
            }
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, stringBuilder.ToString(),
                  ParseMode.Html, null, false, false, update.Message.MessageId);
        }
        #endregion

        #region ResponseToCallbackQueryData

        public async Task ResponseToSetChatIDChannelAsync(TelegramBotClient botClient, Update update)
        {
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.CallbackQuery.Message.Chat.Id);
                user.Step = UserStep.ChatIdMenu;
                await twitterDb.SaveChangesAsync();
            }

            await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "<b>‼️Please send the ChatID of your channel</b>",
                            ParseMode.Html);
        }

        public async Task ResponseToSendToChannelAsync(TelegramBotClient botClient, Update update)
        {
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.CallbackQuery.Message.Chat.Id);
                var information = await twitterDb.Informations.FindAsync(user.Id);
                var status = await botClient.GetChatMemberAsync(information.ChatIdChannel, update.CallbackQuery.Message.Chat.Id);
                if (status.Status == ChatMemberStatus.Creator)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(update.CallbackQuery.Message.Text);
                    stringBuilder.AppendLine(information.Description);
                    await botClient.SendTextMessageAsync(information.ChatIdChannel, stringBuilder.ToString(), ParseMode.Html);
                    await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "<b>✅Sent</b>", ParseMode.Html);
                }
            }
        }

        public async Task ResponseToSetSubtitleAync(TelegramBotClient botClient, Update update)
        {
            using (var twitterDb = new TwitterDbContext())
            {
                var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.CallbackQuery.Message.Chat.Id);
                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "<b>‼️Please send the subtitle of your tweet that send to your channel</b>",
                ParseMode.Html);
                user.Step = UserStep.SubtitleMenu;
                await twitterDb.SaveChangesAsync();
            }
        }
        #endregion

        private async Task<long> ConvertURLToTweetID(string url)
        {
            string id = "";
            for (int index = url.Length - 1; index >= 0; index--)
            {
                if (char.IsDigit(url[index]))
                    id = url[index] + id;
                else
                    break;
            }
            return long.Parse(id);
        }
    }
}
