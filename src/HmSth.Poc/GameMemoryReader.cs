using System;

namespace HmSth.Poc;

public readonly struct GoldReading
{
    public uint Value { get; }
    public GoldReading(uint value) => Value = value;
    public override string ToString() => $"{Value} G";
}

public readonly struct StaminaReading
{
    public byte MaxFatigue { get; }
    public byte Fatigue { get; }
    public byte MaxStamina { get; }
    public byte Stamina { get; }

    public StaminaReading(uint packed)
    {
        MaxFatigue = (byte)(packed >> 24);
        Fatigue = (byte)(packed >> 16);
        MaxStamina = (byte)(packed >> 8);
        Stamina = (byte)packed;
    }

    public bool IsMaxed => Fatigue == MaxFatigue;
    public override string ToString() => $"{Stamina}/{MaxStamina} (max {MaxStamina})";
}

public readonly struct TimeReading
{
    public byte Season { get; }
    public byte Day { get; }
    public byte Hour { get; }
    public byte Minute { get; }

    public TimeReading(uint packed)
    {
        Season = (byte)(packed >> 24);
        Day = (byte)(packed >> 16);
        Hour = (byte)(packed >> 8);
        Minute = (byte)packed;
    }

    public static string SeasonName(byte s) => s switch
    {
        0 => "Spring",
        1 => "Summer",
        2 => "Autumn",
        3 => "Winter",
        _ => $"Season {s}"
    };

    public override string ToString() => $"{SeasonName(Season)}, Day {Day:D2}, {Hour:D2}:{Minute:D2}";
}

public readonly struct WeatherReading
{
    public string Description { get; }
    public WeatherReading(string description) => Description = description;
    public override string ToString() => Description;
}

public sealed class GameMemoryReader
{
    private readonly PineClient _pine;

    // EE addresses from MEMORY_MAP.md
    // TIME anchor verified: 0x002085A2F4
    // STAMINA candidate region: 0x2085A2E2-E8 (using midpoint 0x2085A2E5 for POC)
    // GOLD = STAMINA + 0x34 (GS2/CE delta correlation)
    // Weather hunt start: 0x2085A2E2
    private const uint TimeAddress = 0x002085A2F4;
    private const uint StaminaAddress = 0x002085A2E5;
    private const uint GoldAddress = StaminaAddress + 0x34;
    private const uint WeatherAddress = 0x002085A2E2;

    public GameMemoryReader(PineClient pine) => _pine = pine;

    public GoldReading ReadGold() => new(_pine.ReadU32(GoldAddress));

    public StaminaReading ReadStamina() => new(_pine.ReadU32(StaminaAddress));

    public TimeReading ReadTime() => new(_pine.ReadU32(TimeAddress));

    public WeatherReading ReadWeather()
    {
        try
        {
            uint raw = _pine.ReadU32(WeatherAddress);
            return new WeatherReading(DecodeWeather(raw));
        }
        catch
        {
            return new WeatherReading("Unknown (address not yet located)");
        }
    }

    private static string DecodeWeather(uint raw)
    {
        byte b0 = (byte)(raw >> 24);
        byte b1 = (byte)(raw >> 16);
        byte b2 = (byte)(raw >> 8);
        byte b3 = (byte)raw;
        return $"Raw 0x{raw:X8} (bytes {b0:X2} {b1:X2} {b2:X2} {b3:X2})";
    }
}