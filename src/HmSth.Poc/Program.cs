using System.Net.Sockets;
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

        Console.WriteLine("OK: game detected and memory transport verified.");
        return 0;
    }
}

internal static class PineCommand
{
    public const byte Read32 = 0x02;
    public const byte Version = 0x08;
    public const byte Title = 0x0B;
    public const byte Id = 0x0C;
}

// ponytail: strict serial request/reply - PCSX2 silently drops replies beyond ~7 in-flight requests
internal sealed class PineClient : IDisposable
{
    private const int ReadTimeoutMs = 5000;

    private readonly string _host;
    private readonly int _port;
    private TcpClient? _tcp;

    public PineClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public void Connect()
    {
        _tcp = new TcpClient();
        _tcp.Connect(_host, _port);
        _tcp.GetStream().ReadTimeout = ReadTimeoutMs;
    }

    public string ReadString(byte opcode)
    {
        byte[] data = Request(opcode);

        if (data.Length < 4)
        {
            throw new IOException($"Truncated string payload for command 0x{opcode:X2}.");
        }

        uint lengthIncludingTerminator = BitConverter.ToUInt32(data, 0);

        if (lengthIncludingTerminator == 0 || data.Length < 4 + lengthIncludingTerminator)
        {
            throw new IOException($"Malformed string payload for command 0x{opcode:X2}.");
        }

        int end = 4 + (int)lengthIncludingTerminator;

        if (data[end - 1] != 0)
        {
            throw new IOException($"Missing null terminator for command 0x{opcode:X2}.");
        }

        return Encoding.UTF8.GetString(data, 4, end - 5);
    }

    public uint ReadU32(uint address)
    {
        byte[] data = Request(PineCommand.Read32, address);

        if (data.Length < 4)
        {
            throw new IOException($"Truncated read payload for address 0x{address:X8}.");
        }

        return BitConverter.ToUInt32(data, 0);
    }

    private byte[] Request(byte opcode, uint address = 0)
    {
        bool carriesAddress = opcode == PineCommand.Read32;
        byte[] packet = carriesAddress ? new byte[7] : new byte[3];

        packet[0] = (byte)packet.Length;
        packet[1] = (byte)(packet.Length >> 8);
        packet[2] = opcode;

        if (carriesAddress)
        {
            BitConverter.GetBytes(address).CopyTo(packet, 3);
        }

        NetworkStream stream = _tcp!.GetStream();
        stream.Write(packet, 0, packet.Length);

        byte[] header = ReadExact(stream, 2);
        int responseSize = header[0] | (header[1] << 8);

        if (responseSize < 6)
        {
            throw new IOException($"Invalid response size {responseSize} for command 0x{opcode:X2}.");
        }

        byte[] rest = ReadExact(stream, responseSize - 2);
        int result = BitConverter.ToInt32(rest, 0);

        if (result != 0)
        {
            throw new IOException($"Command 0x{opcode:X2} failed with PINE result code {result}.");
        }

        return rest[4..];
    }

    private static byte[] ReadExact(Stream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        while (offset < count)
        {
            int read = stream.Read(buffer, offset, count - offset);

            if (read <= 0)
            {
                throw new EndOfStreamException("Connection closed by PCSX2.");
            }

            offset += read;
        }

        return buffer;
    }

    public void Dispose() => _tcp?.Dispose();
}
