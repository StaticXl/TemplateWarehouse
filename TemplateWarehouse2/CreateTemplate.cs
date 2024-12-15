using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Npgsql;
using Dapper;
using System.Data;
using System.Data.Common;
using Npgsql.Replication.PgOutput.Messages;
using System.Text.RegularExpressions;
using System.Xml.Linq;

internal class CreateTemplate
    {
    public static async Task AddTemplate(ITelegramBotClient botClient, long userId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Cancel") }) { ResizeKeyboard = true };

        await botClient.SendTextMessageAsync(userId, "Enter  template name: /new_template [Template_name] ['Category'] [Template_Text]", replyMarkup: keyboard);

    }
    public static async Task AddNewTemplateToDatabase(ITelegramBotClient botClient, long userId, string ConnectionString, string messageText)
    {
        List<string> parameters = new List<string>();

        string[] parts = messageText.Split('\'');

        for (int i = 1; i < parts.Length; i += 2)
        {
            if (!parts[i].Trim().StartsWith("/new_template"))
            {
                parameters.Add(parts[i].Trim());
            }
        }

        string[] data = messageText.Split(" ");
        var templateName = parameters[0];
        var category_name = parameters[1];
        var templateText = parameters[2];
        var templateFileId = "";

        Console.WriteLine(templateName);
        Console.WriteLine(category_name);
        Console.WriteLine(templateText);

        using IDbConnection dbConnection = new NpgsqlConnection(ConnectionString);
        var categories = await dbConnection.QueryAsync<(int category_id, string Category_Name)>(
            "SELECT category_id, category_name from categories where category_name = @Category_Name",
            new { Category_Name = category_name });

        foreach (var category in categories) {
            Console.WriteLine(category);
        }
        List<int> categoryIds = categories.Select(x => x.category_id).ToList();
        if (categories != null)
        {
            await dbConnection.ExecuteAsync(
                "INSERT INTO template_text (template_text, template_file_id, category_id, template_name) values (@TemplateText, null, @Category_id, @template_Name)",
                new { TemplateText = templateText, Category_id = categoryIds[0], template_Name = templateName });
            await botClient.SendTextMessageAsync(userId, $"Template {templateName} added to {category_name}");
        }
        else
        {
            await botClient.SendTextMessageAsync(userId, $"Category with name {category_name} doesn't exists");
        }
            await MenuService.ShowMenu(botClient, userId);
    }
}
