using DataLayer;
using Telegram.Bot;
using Telegram.Bot.Types;
namespace TwitterBotV2.Classes
{
    public class Response
    {
        public async Task ResponseToCallbackQuery(TelegramBotClient botClient, Update update)
        {
            Methods methods = new Methods();

            switch (update.CallbackQuery.Data)
            {
                case "SetChatIdChannel":
                    await methods.ResponseToSetChatIDChannelAsync(botClient, update);
                    break;

                case "SendToChannel":
                    await methods.ResponseToSendToChannelAsync(botClient, update);
                    break;

                case "SetSubtitle":
                    await methods.ResponseToSetSubtitleAync(botClient, update);
                    break;
            }
        }

        public async Task ResponceToText(TelegramBotClient botClient, Update update)
        {
            Methods methods = new Methods();

            switch (update.Message.Text)
            {
                case "/start":
                    await methods.ResponseToStartAsync(botClient, update);
                    break;

                case "⚙️Settings":
                    await methods.ResponseToSettingsAsync(botClient, update);
                    break;

                default:
                    if (update.Message.Text.Contains("https://twitter.com/") || update.Message.Text.Contains("http://twitter.com/"))
                        await methods.ResponseToGetTweetAsync(update.Message.Text, botClient, update);

                    else if (update.Message.Text.Contains("/trends"))
                    {
                        var items = update.Message.Text.Split(' ');
                        if (items.Length == 4)
                        {
                            int count = int.Parse(items[3]);

                            if (1 > count || count > 20)
                                count = 4;
                            await methods.ResponseToGetTrendsAsync(botClient, update, double.Parse(items[1]), double.Parse(items[2]), count);
                        }
                        else
                            await methods.ResponseToGetTrendsAsync(botClient, update);
                    }
                    else
                    {
                        using (TwitterDbContext twitterDb = new TwitterDbContext())
                        {
                            var user = twitterDb.Users.FirstOrDefault(user => user.ChatId == update.Message.Chat.Id);

                            if (user.Step == UserStep.ChatIdMenu)
                                await methods.ResponseToChatIdChannelAsync(botClient, update);
                            else
                                await methods.ResponceToSubtitleTweetAsync(botClient, update);
                        }
                    }
                    break;
            }
        }
    }
}
