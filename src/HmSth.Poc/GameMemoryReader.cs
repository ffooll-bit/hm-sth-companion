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

    // EE addresses resolved via CE->EE translation: PINE_EE = 0x20000000 + (ResolvedHost - EEmemBase).
    // EEmem base (host) = 0x7FF740000000, found via Cheat Engine Lua Engine; matches the user's CE base
    // pointer pcsx2-qt.exe+0317C238 + offsets 864/830/5F32F4. Addresses are reboot-stable (EE space
    // does not shift with Windows ASLR). GOLD - STAMINA = 0x34 matches the CE layout.
    private const uint TimeAddress = 0x2085A2F4;
    private const uint StaminaAddress = 0x20267830;
    private const uint GoldAddress = 0x20267864;
    // Weather address not yet located (CE hunt pending, see ENH-009). ReadWeather returns Unknown.

    public GameMemoryReader(PineClient pine) => _pine = pine;

    public GoldReading ReadGold() => new(_pine.ReadU32(GoldAddress));

    public StaminaReading ReadStamina() => new(_pine.ReadU32(StaminaAddress));

    public TimeReading ReadTime() => new(_pine.ReadU32(TimeAddress));

    public WeatherReading ReadWeather() => new("Unknown (address not yet located)");
}