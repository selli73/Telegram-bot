using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using telegram_bot;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;
    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;
    static async Task Main(string[] args)
    {

        // Читаем JSON из файла и десериализуем
        string json = File.ReadAllText("config.json");
        Token tokenBot = JsonSerializer.Deserialize<Token>(json);
        var _botclient = new TelegramBotClient(tokenBot.token);

        _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
        {
            AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
           {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                UpdateType.CallbackQuery // Inline кнопки
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            //ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        _botclient.StartReceiving(UpdateHandler, Error, _receiverOptions, cts.Token);   
        var information_bot = await _botclient.GetMeAsync();
        Console.WriteLine($"{information_bot.FirstName} запущен!");
        
        Console.ReadLine();
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        // Эта переменная будет содержать в себе все связанное с сообщениями
                        var message = update.Message;

                        // From - это от кого пришло сообщение (или любой другой Update)
                        var user = message.From;

                        // Выводим на экран то, что пишут нашему боту, а также небольшую информацию об отправителе
                        Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                        // Chat - содержит всю информацию о чате
                        var chat = message.Chat;

                        // Добавляем проверку на тип Message
                        switch (message.Type)
                        {
                            // Тут понятно, текстовый тип
                            case MessageType.Text:
                                {
                                    // тут обрабатываем команду /start, остальные аналогично
                                    if (message.Text == "/start")
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Выбери клавиатуру:\n" +
                                            "/inline\n" +
                                            "/reply\n" +
                                            "/feedback");
                                        return;
                                    }
                                    if (message.Text == "/inline")
                                    {
                                        // Тут создаем нашу клавиатуру
                                        var inlineKeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                                            {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная кнопка в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("Это кнопка с сайтом", "https://habr.com/"),
                                            InlineKeyboardButton.WithUrl("Материалы для тг бота", "https://core.telegram.org/bots/api#available-methods"),
                                        }
                                            });

                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Это ссылки на полезные источники!",
                                            replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup

                                        return;
                                    }
                                    if (message.Text == "/reply")
                                    {
                                        // Тут все аналогично Inline клавиатуре, только меняются классы
                                        // НО! Тут потребуется дополнительно указать один параметр, чтобы
                                        // клавиатура выглядела нормально, а не как абы что

                                        var replyKeyboard = new ReplyKeyboardMarkup(
                                            new List<KeyboardButton[]>()
                                            {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Привет!"),
                                            new KeyboardButton("Пока!"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Позвони мне!")
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Напиши моему соседу!")
                                        }
                                            })
                                        {
                                            // автоматическое изменение размера клавиатуры, если не стоит true,
                                            // тогда клавиатура растягивается чуть ли не до луны,
                                            // проверить можете сами
                                            ResizeKeyboard = true,
                                        };

                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Это reply клавиатура!",
                                            replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup

                                        return;
                                    }
                                    if (message.Text == "/feedback")
                                    {
                                        // Тут создаем нашу клавиатуру
                                        var inlineKeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                                            {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная кнопка в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("ТГ для обратной связи", "https://t.me/selli173"),
                                            InlineKeyboardButton.WithUrl("ВК для обратной связи", "https://vk.com/selli7"),
                                        }
                                            });

                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Обратная связь",
                                            replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup

                                        return;
                                    }

                                    if (message.Text.ToLower().Contains("привет"))
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "И вам доброго");

                                        return;
                                    }

                                    if (message.Text.ToLower().Contains("пока"))
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Всего хорошего)");

                                        return;
                                    }

                                    if (message.Text == "Позвони мне!")
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Хорошо, присылай номер!");
                                        return;
                                    }
                                    if (message.Text == "Напиши моему соседу!")
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "А самому что, трудно что-ли ?");

                                        return;
                                    }
                                    if (Regex.IsMatch(message.Text, @"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\-]?)?[\d\- ]{7,10}$"))
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Отлично, попробую вам позванить");
                                        return;
                                    }
                                    else
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            "Вы ввели некорректный номер телефона");
                                    }

                                    return;
                                }

                            // Добавил default , чтобы показать вам разницу типов Message
                            default:
                                {
                                    await botClient.SendMessage(
                                        chat.Id,
                                        "Используй только текст!");
                                    return;
                                }
                        }
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
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