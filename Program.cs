using DataLayer;
using Telegram.Bot;
using Telegram.Bot.Types;
using TwitterBotV2.Classes;
namespace TwitterBotV2
{
    class Program
    {
        static TelegramBotClient bot = new TelegramBotClient("Token-Bot");

        public static async Task Main()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                using (TwitterDbContext twitterDb = new TwitterDbContext())
                {
                    twitterDb.Database.CreateIfNotExists();
                }
                bot.StartReceiving(HandleUpdateAsyns, HandleErrorAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:yyyy/MM//dd-HH:mm:ss} | {ex.Message}");
                Console.WriteLine("------------------------------------------------------");
            }
            Console.ReadKey();
        }

        private static async Task HandleUpdateAsyns(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Response response = new Response();

            int offset = 0;
            while (true)
            {
                var updates = await botClient.GetUpdatesAsync(offset);

                foreach (var up in updates)
                {
                    offset = up.Id + 1;

                    if (up.CallbackQuery != null)
                        response.ResponseToCallbackQuery(bot, up);
                    else if (up.Message != null)
                        response.ResponceToText(bot, up);
                }
            }
        }
        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
    }
}

