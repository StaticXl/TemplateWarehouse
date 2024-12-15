using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class CreateCategory
{
    public static async Task CreateNewCategory(ITelegramBotClient botClient, long userId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Cancel") }) { ResizeKeyboard = true };

        await botClient.SendTextMessageAsync(userId, "Enter new category name: /new [Category_Name]", replyMarkup: keyboard);
        
    }

    public static async Task AddNewCategoryToDatabase(ITelegramBotClient botClient, long userId, string ConnectionString,string messageText)
    {
        var categoryName = messageText.Substring(5);
        using IDbConnection dbConnection = new NpgsqlConnection(ConnectionString);
        var categories = await dbConnection.QueryAsync<(int category_id, string category_name)>(
            "SELECT category_id, category_name from categories where category_name = @Category_name",
            new { Category_name = categoryName });
        if (categories != null)
        {
            await dbConnection.ExecuteScalarAsync<int>(
                "INSERT INTO categories (category_name) values (@Category_name) ", new { Category_name = categoryName });
            await botClient.SendTextMessageAsync(userId, "Category created");
        }
        else
        {
            await botClient.SendTextMessageAsync(userId, "Category allready exists");
        }
        await MenuService.ShowMenu(botClient, userId);
    }
}
