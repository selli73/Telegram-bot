using System.Reflection.Metadata;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

class Program
{
    static void Main(string[] args)
    {
        var client = new TelegramBotClient("7323782581:AAHDVZlRjDDib8jFL9DjaUYLiCoS-M0_RR4");
        client.StartReceiving(Update, Error);

        Console.ReadLine();
    }

    async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        var message = update.Message;  // это контент
        if (message.Text != null)
        {
            Console.WriteLine($"{message.Chat.FirstName}     |      {message.Text}");
            if (message.Text.ToLower().Contains("здорово"))
            {
                await client.SendMessage(message.Chat.Id, "Приветик :)");
                return;
            }
        }
        
        if (message.Photo != null)
        {
            await client.SendMessage(message.Chat.Id, "Классная фотка!");
            return;
        }
    }


    private static async Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}