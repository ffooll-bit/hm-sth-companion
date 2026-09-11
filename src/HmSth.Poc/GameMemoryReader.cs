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
    public byte Today { get; }
    public byte Forecast { get; }

    public WeatherReading(uint packed)
    {
        Today = (byte)(packed & 0xFF);
        Forecast = (byte)((packed >> 8) & 0xFF);
    }

    public string TodayName => Name(Today);
    public string ForecastName => Name(Forecast);

    public override string ToString() => $"Today: {TodayName} | Forecast: {ForecastName}";

    private static string Name(byte value) => value switch
    {
        0 => "Clear",
        1 => "Light rain",
        2 => "Heavy rain",
        3 => "Storm",
        4 => "Cloudy",
        _ => $"({value})",
    };
}

public readonly struct ToolReading
{
    public byte Id { get; }

    public ToolReading(uint packed) => Id = (byte)(packed & 0xFF);

    public string Name => Id switch
    {
        0xFF => "Empty",
        0x51 => "Sickle",
        0x3A => "Chicken Feed",
        0x53 => "Hoe",
        0x54 => "Watering Can",
        0x55 => "Fishing Rod",
        0x5A => "Flute",
        _ => $"(0x{Id:X2})",
    };

    public override string ToString() => Name;
}

public readonly struct ItemReading
{
    public byte Id { get; }

    public ItemReading(uint packed) => Id = (byte)(packed & 0xFF);

    // ponytail: ID mapping incomplete (ENH-010); show hex until a full look-up table exists.
    public override string ToString() => Id switch
    {
        0xFF => "Empty",
        _ => $"0x{Id:X2}",
    };
}

public sealed class GameMemoryReader
{
    private readonly PineClient _pine;

    // EE addresses resolved via CE->EE translation: PINE_EE = 0x20000000 + (ResolvedHost - EEmemBase).
    // EEmem base (host) = 0x7FF740000000, found via Cheat Engine Lua Engine; matches the user's CE base
    // pointer pcsx2-qt.exe+0317C238 + offsets 864/830/5F32F4. Addresses are reboot-stable (EE space
    // does not shift with Windows ASLR). GOLD - STAMINA = 0x34 matches the CE layout.
    private const uint TimeAddress = 0x2085A2F4;
    private const uint StaminaAddress = 0x20267830;
    private const uint GoldAddress = 0x20267864;
    private const uint WeatherAddress = 0x20267834; // CE offset +834, format 0000XXYY (forecast|today)
    private const uint ActiveToolAddress = 0x20267844; // CE offset +844, format 000000ZZ
    private const uint ActiveItemAddress = 0x20267840; // CE offset +840, format 000000ZZ

    public GameMemoryReader(PineClient pine) => _pine = pine;

    public GoldReading ReadGold() => new(_pine.ReadU32(GoldAddress));

    public StaminaReading ReadStamina() => new(_pine.ReadU32(StaminaAddress));

    public TimeReading ReadTime() => new(_pine.ReadU32(TimeAddress));

    public WeatherReading ReadWeather() => new(_pine.ReadU32(WeatherAddress));

    public ToolReading ReadTool() => new(_pine.ReadU32(ActiveToolAddress));

    public ItemReading ReadItem() => new(_pine.ReadU32(ActiveItemAddress));
}