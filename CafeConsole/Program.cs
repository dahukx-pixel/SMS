using Cafe.Domain.Models;
using Cafe.Infrastructure.Repositories;
using Cafe.Infrastructure.Repositories.Interfaces;
using CafeClient;
using CafeConsole.Settings;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var settings = config.GetSection("Settings").Get<Settings>();

if (!settings.IsInitialized)
{
    SendSettingsNotInitializedMessage(appSettingsPath);
    return;
}

ClientCafe cafeClient = new ClientCafe($"{settings.ServerIP}:{settings.ServerPort}", "123", "12345");
IMenuItemRepository repository = new MenuItemRepository();

int orderId = 1;

var menu = await cafeClient.GetMenuAsync();

if (menu is not null && menu.Count > 0)
{
    await repository.AddRangeAsync(menu);
    Console.WriteLine("Menu items have been successfully added to the database.");
    Console.WriteLine("Click Any Key to continue...");
    Console.ReadKey();
}
else
{
    Console.WriteLine("No menu items retrieved from the client.");
    Console.WriteLine("Click Any Key to Exit...");
    Console.ReadKey();
    return;
}

while (true)
{
    Console.Clear();

    Console.WriteLine("Menu:");
    foreach (var item in menu)
    {
        Console.WriteLine($"Item: {item.Name}, Price: {item.Price}");
    }

    Console.WriteLine("Make order like: ItemId:count;ItemId:count;");

    var order = Console.ReadLine();

    var items = order.Split(";");

    if (items.Length == 0)
    {
        Console.WriteLine("\nCan't find items in order.");
        Console.WriteLine("Click Any Key to retry...");
        Console.ReadKey();
        continue;
    }

    var orderItems = new List<OrderItem>();

    foreach (var item in items)
    {
        var parts = item.Split(":");
        if (parts.Length != 2)
            continue;
        if (!int.TryParse(parts[0], out int itemId) || !int.TryParse(parts[1], out int count))
            continue;
        var menuItem = menu.FirstOrDefault(x => x.Id == itemId);
        if (menuItem is null)
            continue;

        orderItems.Add(new OrderItem
        {
            Id = itemId,
            Quantity = count
        });
    }

    if (orderItems.Count == 0)
    {
        Console.WriteLine("\nCan't find valid items in order.");
        Console.WriteLine("Click Any Key to retry...");
        Console.ReadKey();
        continue;
    }

    Console.Clear();

    Console.WriteLine("Order...");

    foreach (var orderItem in orderItems)
    {
        var menuItem = menu.First(x => x.Id == orderItem.Id);
        Console.WriteLine($"Item: {menuItem.Name}, Price: {menuItem.Price}, Count: {orderItem.Quantity}, Total: {menuItem.Price * orderItem.Quantity}");
    }

    Console.WriteLine("Click Any Key to make a new order...");
    Console.ReadKey();

    await cafeClient.SendOrderAsync((orderId++).ToString(), orderItems);
}

static void SendSettingsNotInitializedMessage(string path)
{
    Console.WriteLine("Файл конфигурации 'appsettings.json' не настроен. Создан шаблон по умолчанию.");

    Console.WriteLine("\nФайл 'appsettings.json' создан в папке приложения:");
    Console.WriteLine($"   {path}");
    Console.WriteLine("\nВАЖНО: Откройте файл и укажите корректные значения, затем установите IsInitialized в значение true:");
    Console.WriteLine("   • ServerIP / ServerPort — адрес и порт API кафе");
    Console.WriteLine("   • Database* — параметры подключения к БД (если используются)");
    Console.WriteLine("   • CafeClient:ApiKey / ApiSecret — учётные данные API");
    Console.WriteLine("\nРекомендация: Для продакшена используйте переменные окружения вместо хранения паролей в файле.");
    Console.WriteLine("\nНажмите любую клавишу для завершения...");
    Console.ReadKey();
    return;
}
