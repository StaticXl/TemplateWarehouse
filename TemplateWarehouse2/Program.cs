using Dapper;
using Npgsql;
using System.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Diagnostics.Eventing.Reader;


namespace TemplateWarehouse2
{
    class Program
    {
        private static IConfigurationRoot configuration;
        public static string ConnectionString;
        public static ITelegramBotClient botClient;

        private static async Task Main(string[] args)
        {
            // Настройка конфигурации
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Получение строки подключения и токена бота
            ConnectionString = configuration.GetConnectionString("TemplateWarehouse");
            var token = configuration["TelegramBot:Token"];
            botClient = new TelegramBotClient(token);

            var cts = new CancellationTokenSource();
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token);

            Console.WriteLine("Bot is running...");
            await Task.Run(() => Console.ReadLine());
            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                var userId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                if (messageText.StartsWith("/start") || messageText.StartsWith("/menu"))
                {
                    using IDbConnection dbConnection = new NpgsqlConnection(ConnectionString);
                    var whiteList = await dbConnection.QuerySingleOrDefaultAsync<string>(
                        "SELECT name FROM users WHERE telegram_id = @UserId",
                        new { UserId = userId });

                    if (whiteList != null)
                    {
                        await MenuService.ShowMenu(botClient, userId);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(userId, "You don't have permissons to use this function, please contact with your administrator user");
                    }
                }
                else if (messageText.StartsWith("/new "))
                {
                    CreateCategory.AddNewCategoryToDatabase(botClient, userId, ConnectionString, messageText);
                }
                else if (messageText.StartsWith("/new_template "))
                {
                    CreateTemplate.AddNewTemplateToDatabase(botClient, userId, ConnectionString, messageText);
                }
                else
                {
                    if (messageText == "Categories")
                    {
                        await GetCategories.SendCategories(botClient, userId, ConnectionString);
                    }
                    else if (messageText == "Create Category")
                    {
                        await CreateCategory.CreateNewCategory(botClient, userId);
                    }
                    else if (messageText == "Add Template")
                    {
                        await CreateTemplate.AddTemplate(botClient, userId);
                    }
                    else if (messageText == "Cancel")
                    { 
                        MenuService.ShowMenu(botClient, userId);
                    }
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery);
            }

        }

        private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            string[] data = callbackQuery.Data.Split('_');
            var action = data[0];
            var userId = callbackQuery.From.Id;

            if (action.StartsWith("open"))
            {
                int categoryId = int.Parse(data[1]);
                await ShowTemplatesList.ShowTemplates(userId, categoryId, ConnectionString, botClient);
            }
            else if (action.StartsWith("update"))
            {
                int templateId = int.Parse(data[1]);
                await UpdateTemplates.UpdateTemplate(botClient, userId, templateId, ConnectionString);
            }
        }

        //Console error handler
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
