using System.Text;
using MQTTnet;

Console.OutputEncoding = Encoding.UTF8;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("============================================================");
Console.WriteLine("          IOTECH - MQTT TRAFIK IZLEME (MONITOR)             ");
Console.WriteLine("============================================================");
Console.ResetColor();
Console.WriteLine("Baglanti : localhost:1883");
Console.WriteLine("Dinlenen : iotech/#");
Console.WriteLine("Durdurma : Ctrl + C");
Console.WriteLine(new string('-', 60));

var factory = new MqttClientFactory();
var client = factory.CreateMqttClient();

client.ApplicationMessageReceivedAsync += args =>
{
    var time = DateTime.Now.ToString("HH:mm:ss.fff");
    var topic = args.ApplicationMessage.Topic;
    var payload = args.ApplicationMessage.ConvertPayloadToString();
    var isRetain = args.ApplicationMessage.Retain;

    lock (Console.Out)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{time}] ");

        switch (topic)
        {
            case "iotech/lamp/state":
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[SERVER -> CLIENT] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Topic: {topic} | ");
                Console.ForegroundColor = (payload == "1") ? ConsoleColor.Green : ConsoleColor.Red;
                var lampText = (payload == "1") ? "1 (ACIK)" : "0 (KAPALI)";
                Console.Write($"Lamba Durumu: {lampText}");
                break;

            case "iotech/lamp/set":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("[CLIENT -> SERVER] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Topic: {topic} | ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                var setVal = (payload == "1") ? "1 (ACILSIN)" : "0 (KAPANSIN)";
                Console.Write($"Lamba Degistirme Istegi: {setVal}");
                break;

            case "iotech/text/state":
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[SERVER -> CLIENT] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Topic: {topic} | ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Metin Alani Degeri: \"{payload}\"");
                break;

            case "iotech/text/random":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("[CLIENT -> SERVER] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Topic: {topic} | ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Rastgele Sayi Uretme Istegi");
                break;

            case "iotech/get_state":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("[CLIENT -> SERVER] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Topic: {topic} | ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Client Baglandi - Mevcut Durum Talebi (Senkronizasyon)");
                break;

            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[TRAFIK] ");
                Console.Write($"Topic: {topic} | Payload: {payload}");
                break;
        }

        if (isRetain)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(" [RETAINED]");
        }

        Console.WriteLine();
        Console.ResetColor();
    }

    return Task.CompletedTask;
};

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId($"MonitorConsole_{Guid.NewGuid():N}")
    .Build();

while (true)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sunucuya baglaniliyor (localhost:1883)...");
        Console.ResetColor();

        await client.ConnectAsync(options);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MQTT Broker'a baglanildi! Trafik canli olarak akiyor...\n");
        Console.ResetColor();

        await client.SubscribeAsync("iotech/#");

        while (client.IsConnected)
        {
            await Task.Delay(1000);
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Baglanti kesildi! 3 saniye sonra tekrar denenecek...");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Baglanti kurulamadi: {ex.Message}. 3 saniye sonra tekrar denenecek...");
        Console.ResetColor();
    }

    await Task.Delay(3000);
}
