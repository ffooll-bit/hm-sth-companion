using System.Text;

namespace HmSth.Poc;

// ponytail: constants instead of arg parsing - POC scope; promote to config when the real app takes shape
internal static class Program
{
    private const string Host = "127.0.0.1";
    private const int Port = 28011;
    private const string ExpectedSerial = "SLUS-20251";
    private const uint DemoAddress = 0x00200000;

    private static int Main()
    {
        using var pine = new PineClient(Host, Port);

        try
        {
            pine.Connect();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Cannot connect to PCSX2 PINE IPC at {Host}:{Port}: {ex.Message}");
            Console.Error.WriteLine("Enable PINE IPC in the PCSX2 settings, start a game, then retry.");
            return 1;
        }

        string version = pine.ReadString(PineCommand.Version);
        string serial = pine.ReadString(PineCommand.Id);
        string title = pine.ReadString(PineCommand.Title);
        uint value = pine.ReadU32(DemoAddress);

        Console.WriteLine($"Emulator : {version}");
        Console.WriteLine($"Title    : {title}");
        Console.WriteLine($"Serial   : {serial}");
        Console.WriteLine($"Read32 @ 0x{DemoAddress:X8} -> 0x{value:X8}");

        if (!string.Equals(serial, ExpectedSerial, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Game mismatch: expected {ExpectedSerial}, emulator reports '{serial}'.");
            return 2;
        }

        var reader = new GameMemoryReader(pine);
        Console.WriteLine($"Gold     : {reader.ReadGold()}");
        Console.WriteLine($"Stamina  : {reader.ReadStamina()}");
        Console.WriteLine($"Time     : {reader.ReadTime()}");
        Console.WriteLine($"Weather  : {reader.ReadWeather()}");

        Console.WriteLine("OK: game detected and memory transport verified.");
        return 0;
    }
}
