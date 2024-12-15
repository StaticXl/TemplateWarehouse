using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;


public class MenuService
{
    public static async Task ShowMenu(ITelegramBotClient botClient, long chatId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new [] { new KeyboardButton("Categories"), new KeyboardButton("Create Category"), new KeyboardButton("Add Template")}
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Menu:", replyMarkup: keyboard);
    }
}