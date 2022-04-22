using DataLayer;
using Telegram.Bot;
using Telegram.Bot.Types;
using TwitterBotV2.Classes;
namespace TwitterBotV2
{
    class Program
    {
        static TelegramBotClient bot = new TelegramBotClient("1874331325:AAHMw8_QNcIIwI2mNvnY3bWiZ4U0PmWJJkk");

        public static async Task Main()
        {
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:yyyy/MM//dd-HH:mm:ss} | {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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

