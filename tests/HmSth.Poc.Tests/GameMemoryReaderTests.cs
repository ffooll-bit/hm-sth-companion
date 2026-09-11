using System.Buffers.Binary;
using HmSth.Poc;

namespace HmSth.Poc.Tests;

public class GameMemoryReaderTests : IDisposable
{
    private readonly FakePineServer _server = new();
    private readonly PineClient _client;
    private readonly GameMemoryReader _reader;

    public GameMemoryReaderTests()
    {
        _client = new PineClient("127.0.0.1", _server.Port);
        _client.Connect();
        _reader = new GameMemoryReader(_client);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public void ReadGold_ReturnsParsedValue()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0x10, 0x27, 0x00, 0x00])); // 10000

        GoldReading gold = _reader.ReadGold();

        Assert.Equal(10000u, gold.Value);
    }

    [Fact]
    public void ReadStamina_ParsesFourBytesCorrectly()
    {
        // Packed: maxFatigue=140, fatigue=80, maxStamina=140, stamina=100
        uint packed = (140u << 24) | (80u << 16) | (140u << 8) | 100u;
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, packed);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        StaminaReading stamina = _reader.ReadStamina();

        Assert.Equal(140, stamina.MaxFatigue);
        Assert.Equal(80, stamina.Fatigue);
        Assert.Equal(140, stamina.MaxStamina);
        Assert.Equal(100, stamina.Stamina);
        Assert.False(stamina.IsMaxed);
    }

    [Fact]
    public void ReadStamina_IsMaxedWhenFatigueEqualsMaxFatigue()
    {
        uint packed = (140u << 24) | (140u << 16) | (140u << 8) | 100u;
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, packed);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        StaminaReading stamina = _reader.ReadStamina();

        Assert.True(stamina.IsMaxed);
    }

    [Fact]
    public void ReadTime_ParsesSeasonDayHourMinute()
    {
        // Summer(1), Day 7, 21:00
        uint packed = (1u << 24) | (7u << 16) | (21u << 8) | 0u;
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, packed);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        TimeReading time = _reader.ReadTime();

        Assert.Equal(1, time.Season);
        Assert.Equal(7, time.Day);
        Assert.Equal(21, time.Hour);
        Assert.Equal(0, time.Minute);
        Assert.Equal("Summer, Day 07, 21:00", time.ToString());
    }

    [Fact]
    public void ReadWeather_ParsesTodayAndForecast()
    {
        // Packed 0000XXYY: forecast=3, today=2
        uint packed = (3u << 8) | 2u;
        byte[] payload = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, packed);
        _server.ServeOne(_ => FakePineServer.Ok(payload));

        WeatherReading weather = _reader.ReadWeather();

        Assert.Equal(2, weather.Today);
        Assert.Equal(3, weather.Forecast);
        Assert.Equal("Heavy rain", weather.TodayName);
        Assert.Equal("Storm", weather.ForecastName);
        Assert.Equal("Today: Heavy rain | Forecast: Storm", weather.ToString());
    }

    [Fact]
    public void ReadTool_MapsKnownIdToName()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0x51, 0x00, 0x00, 0x00])); // Sickle

        ToolReading tool = _reader.ReadTool();

        Assert.Equal(0x51, tool.Id);
        Assert.Equal("Sickle", tool.ToString());
    }

    [Fact]
    public void ReadTool_EmptyWhenSlotClear()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0xFF, 0x00, 0x00, 0x00])); // Empty

        Assert.Equal("Empty", _reader.ReadTool().ToString());
    }

    [Fact]
    public void ReadTool_UnknownIdShowsHex()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0x7B, 0x00, 0x00, 0x00])); // unmapped

        Assert.Equal("(0x7B)", _reader.ReadTool().ToString());
    }

    [Fact]
    public void ReadItem_EmptyWhenSlotClear()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0xFF, 0x00, 0x00, 0x00])); // Empty

        Assert.Equal("Empty", _reader.ReadItem().ToString());
    }

    [Fact]
    public void ReadItem_UnknownIdShowsHex()
    {
        _server.ServeOne(_ => FakePineServer.Ok([0x2A, 0x00, 0x00, 0x00])); // unmapped

        Assert.Equal("0x2A", _reader.ReadItem().ToString());
    }

}