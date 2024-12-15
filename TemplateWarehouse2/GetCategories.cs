using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Dapper;
using Npgsql;

internal class GetCategories
    {
    public static async Task SendCategories(ITelegramBotClient botClient, long chatId, string ConnectionString) {
        using IDbConnection dbConnection = new NpgsqlConnection(ConnectionString);
        var categories = await dbConnection.QueryAsync<(int category_id, string category_name)>(
            "SELECT category_id, category_name FROM categories");

        foreach (var category in categories)
        {
            var message = $"{category.category_name}";
            var replyMarkup = new InlineKeyboardMarkup(new[]
            {
                    InlineKeyboardButton.WithCallbackData("Open", $"open_{category.category_id}"),
                });

            await botClient.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup);
        }
    }
    }
