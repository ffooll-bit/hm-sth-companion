using System.Net.Sockets;
using System.Text;

namespace HmSth.Poc;

public static class PineCommand
{
    public const byte Read32 = 0x02;
    public const byte Version = 0x08;
    public const byte Title = 0x0B;
    public const byte Id = 0x0C;
}

public sealed class PineConnectionException : Exception
{
    public PineConnectionException(string message) : base(message) { }
    public PineConnectionException(string message, Exception inner) : base(message, inner) { }
}

// ponytail: strict serial request/reply - PCSX2 silently drops replies beyond ~7 in-flight requests
public sealed class PineClient : IDisposable
{
    private const int ReadMetadataTimeoutMs = 15000;
    private const int ReadGameplayTimeoutMs = 5000;

    private const bool DebugLog = true;

    private static void LogDebug(string message)
    {
        if (DebugLog)
            Console.Error.WriteLine($"[PINE DEBUG] {DateTime.Now:HH:mm:ss.fff} {message}");
    }

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
        _tcp.GetStream().ReadTimeout = ReadGameplayTimeoutMs;
    }

    public string ReadString(byte opcode)
    {
        var stream = _tcp!.GetStream();
        var originalTimeout = stream.ReadTimeout;
        try
        {
            stream.ReadTimeout = ReadMetadataTimeoutMs;
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
        finally
        {
            stream.ReadTimeout = originalTimeout;
        }
    }

    public uint ReadU32(uint address)
    {
        var stream = _tcp!.GetStream();
        var originalTimeout = stream.ReadTimeout;
        try
        {
            stream.ReadTimeout = ReadGameplayTimeoutMs;
            byte[] data = Request(PineCommand.Read32, address);

            if (data.Length < 4)
            {
                throw new IOException($"Truncated read payload for address 0x{address:X8}.");
            }

            return BitConverter.ToUInt32(data, 0);
        }
        finally
        {
            stream.ReadTimeout = originalTimeout;
        }
    }

    private byte[] Request(byte opcode, uint address = 0)
    {
        bool carriesAddress = opcode == PineCommand.Read32;
        int bodyLength = carriesAddress ? 4 : 0;
        int packetSize = 5 + bodyLength; // 4 bytes size + 1 opcode + body
        byte[] packet = new byte[packetSize];

        // 4-byte size header (u32 LE)
        byte[] sizeBytes = BitConverter.GetBytes((uint)packetSize);
        sizeBytes.CopyTo(packet, 0);

        // 1-byte opcode
        packet[4] = opcode;

        if (carriesAddress)
        {
            BitConverter.GetBytes(address).CopyTo(packet, 5);
        }

        NetworkStream stream = _tcp!.GetStream();
        stream.Write(packet, 0, packet.Length);

        LogDebug($"TX: {BitConverter.ToString(packet).Replace("-", " ")}");

        // Response header: 5 bytes (4 size + 1 result code)
        byte[] header = ReadExact(stream, 5);
        int responseSize = header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24);
        byte resultCode = header[4];

        LogDebug($"RX header: {BitConverter.ToString(header).Replace("-", " ")} (size={responseSize}, resultCode={resultCode})");

        if (responseSize < 5)
        {
            throw new IOException($"Invalid response size {responseSize} for command 0x{opcode:X2}.");
        }

        if (resultCode == 0xFF)
        {
            throw new IOException($"Command 0x{opcode:X2} failed with PINE result code 255.");
        }

        if (resultCode != 0)
        {
            throw new IOException($"Command 0x{opcode:X2} failed with PINE result code {resultCode}.");
        }

        byte[] rest = ReadExact(stream, responseSize - 5);

        LogDebug($"RX body: {BitConverter.ToString(rest).Replace("-", " ")}");

        // Response body starts after 5-byte header (4 size + 1 result code)
        return rest;
    }

    private static byte[] ReadExact(Stream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        while (offset < count)
        {
            try
            {
                int read = stream.Read(buffer, offset, count - offset);

                if (read <= 0)
                {
                    throw new EndOfStreamException("Connection closed by PCSX2.");
                }

                offset += read;
            }
            catch (IOException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.TimedOut })
            {
                throw new PineConnectionException(
                    "Connected to PCSX2 but no PINE response received — ensure the game is fully in-game (not paused/menu) and PINE IPC is fully initialized in PCSX2 settings.",
                    ex);
            }
        }

        return buffer;
    }

    public void Dispose() => _tcp?.Dispose();
}
