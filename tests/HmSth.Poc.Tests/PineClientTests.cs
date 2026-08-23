using System.Buffers.Binary;
using System.Text;
using HmSth.Poc;

namespace HmSth.Poc.Tests;

public class PineClientTests : IDisposable
{
    private readonly FakePineServer _server = new();
    private readonly PineClient _client;

    public PineClientTests()
    {
        _client = new PineClient("127.0.0.1", _server.Port);
        _client.Connect();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public void ReadU32_SendsSevenByteFramedRequestAndParsesValue()
    {
        byte[]? captured = null;
        _server.ServeOne(request =>
        {
            captured = request;
            return FakePineServer.Ok([0x39, 0x05, 0x00, 0x00]);
        });

        uint value = _client.ReadU32(0x00200000);

        Assert.Equal(1337u, value);
        Assert.NotNull(captured);
        Assert.Equal(7, captured.Length);
        Assert.Equal(7, captured[0] | (captured[1] << 8));
        Assert.Equal(PineCommand.Read32, captured[2]);
        Assert.Equal(0x00200000u, BinaryPrimitives.ReadUInt32LittleEndian(captured.AsSpan(3)));
    }

    [Fact]
    public void ReadString_MetadataOpcode_SendsThreeByteRequest()
    {
        byte[]? captured = null;
        _server.ServeOne(request =>
        {
            captured = request;
            return FakePineServer.OkString("PCSX2 2.3.123");
        });

        string version = _client.ReadString(PineCommand.Version);

        Assert.Equal("PCSX2 2.3.123", version);
        Assert.NotNull(captured);
        Assert.Equal(3, captured.Length);
        Assert.Equal(PineCommand.Version, captured[2]);
    }

    [Fact]
    public void ReadString_DecodesUtf8Payload()
    {
        _server.ServeOne(_ => FakePineServer.OkString("SCUS-94164"));

        string value = _client.ReadString(PineCommand.Id);

        Assert.Equal("SCUS-94164", value);
    }

    [Theory]
    [InlineData(new byte[] { 0x05, 0x00 })]
    [InlineData(new byte[] { 0x00, 0x00 })]
    [InlineData(new byte[] { 0x01, 0x00 })]
    public void ResponseSize_BelowSix_Throws(byte[] header)
    {
        _server.ServeOne(_ => header);

        IOException failure = Assert.Throws<IOException>(() => _client.ReadString(PineCommand.Title));

        Assert.Contains("Invalid response size", failure.Message);
    }

    [Fact]
    public void NonZeroResultCode_ThrowsWithCodeInMessage()
    {
        _server.ServeOne(_ => FakePineServer.Error(255));

        IOException failure = Assert.Throws<IOException>(() => _client.ReadU32(0x00100000));

        Assert.Contains("result code 255", failure.Message);
    }

    [Fact]
    public void ConnectionClosed_BeforeHeader_ThrowsEndOfStream()
    {
        _server.ServeOne(null);

        Assert.Throws<EndOfStreamException>(() => _client.ReadString(PineCommand.Id));
    }

    [Fact]
    public void ConnectionClosed_AfterPartialBody_ThrowsEndOfStream()
    {
        _server.ServeOne(_ =>
        {
            byte[] partial = new byte[8];
            BinaryPrimitives.WriteUInt16LittleEndian(partial, 10);
            return partial;
        });

        Assert.Throws<EndOfStreamException>(() => _client.ReadU32(0x00100000));
    }

    [Fact]
    public void StringPayload_ZeroLength_Throws()
    {
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, 0);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        IOException failure = Assert.Throws<IOException>(() => _client.ReadString(PineCommand.Id));

        Assert.Contains("Malformed string payload", failure.Message);
    }

    [Fact]
    public void StringPayload_LengthBeyondData_Throws()
    {
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, 20);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        IOException failure = Assert.Throws<IOException>(() => _client.ReadString(PineCommand.Id));

        Assert.Contains("Malformed string payload", failure.Message);
    }

    [Fact]
    public void StringPayload_MissingNullTerminator_Throws()
    {
        byte[] text = Encoding.UTF8.GetBytes("SLUS");
        byte[] payload = new byte[4 + text.Length + 1];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, (uint)(text.Length + 1));
        text.CopyTo(payload, 4);
        payload[^1] = 0x2E;

        _server.ServeOne(_ => FakePineServer.Ok(payload));

        IOException failure = Assert.Throws<IOException>(() => _client.ReadString(PineCommand.Id));

        Assert.Contains("Missing null terminator", failure.Message);
    }

    [Fact]
    public void U32Payload_Truncated_Throws()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0x01, 0x02]));

        IOException failure = Assert.Throws<IOException>(() => _client.ReadU32(0x00100000));

        Assert.Contains("Truncated read payload", failure.Message);
    }
}
