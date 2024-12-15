using Npgsql;
using Dapper;
using Telegram.Bot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class ShowTemplatesList
{
    public static async Task ShowTemplates(long userId, int categoryId, string ConnectionString, ITelegramBotClient botClient)
    {
        using IDbConnection dbConnection = new NpgsqlConnection(ConnectionString);
        var templates = await dbConnection.QueryAsync<(int template_id, string template_name, string template_text)>(
            "SELECT id,template_name, template_text from template_text where category_id = @CategoryId",
            new { CategoryId = categoryId });

        if (templates == null)
        {
            await botClient.SendTextMessageAsync(userId, "No templates for this category");
        }
        else
        {
            foreach (var template in templates)
            {
                var message = $"{template.template_name}\n{template.template_text}";
                var replyMarkup = new InlineKeyboardMarkup(new[]
            {
                    InlineKeyboardButton.WithCallbackData("Update", $"update_{template.template_id}"),
                });
                await botClient.SendTextMessageAsync(userId, message, parseMode: ParseMode.Html, replyMarkup: replyMarkup);
            }
        }
    }
}
