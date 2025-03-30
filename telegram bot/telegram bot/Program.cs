using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using telegram_bot;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static async Task Main(string[] args)
    {

        // Читаем JSON из файла и десериализуем
        string json = File.ReadAllText("config.json");
        Token tokenBot = JsonSerializer.Deserialize<Token>(json);

        
        var client = new TelegramBotClient(tokenBot.token);

        
        client.StartReceiving(Update, Error);   

        var information_bot = await client.GetMeAsync();
        Console.WriteLine($"{information_bot.FirstName} запущен!");

        Console.ReadLine();
    }

    static async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        /*
        var message = update.Message;  // это контент
        if (message.Text != null)
        {
            Console.WriteLine($"{message.Chat.FirstName}     |      {message.Text}");
            if (message.Text.ToLower().Contains("привет") || message.Text.ToLower().Contains("здравств") || message.Text.ToLower().Contains("здорово"))
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
        */
        try
        {
            // update.Type - это свойство в классе Update. Оно получает тип обновления. 
            switch (update.Type)  
                {
                case UpdateType.Message:
                    {
                        // эта переменная будет содержать в себе все связанное с сообщениями
                        var message = update.Message;
                        // From - это от кого пришло сообщение
                        var user = message.From;
                        Console.WriteLine($"{user.FirstName} (id {user.Id}) написал сообщение: {message.Text}");
               
                        // Chat - содержит всю информацию о чате
                        var chat = message.Chat;

                        await client.SendTextMessageAsync(chat.Id, 
                            message.Text // отправляем то, что написал пользователь
                            );

                        return;
                    }
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString);
        }
    }


    private static Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }   
}