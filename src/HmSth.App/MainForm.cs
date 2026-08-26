using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HmSth.Poc;

namespace HmSth.App;

internal enum AppState { Disconnected, WrongGame, Playing }

internal sealed class MainForm : Form
{
    private const string Host = "127.0.0.1";
    private const int Port = 28011;
    private const string ExpectedSerial = "SLUS-20251";
    private const string GoldAddr = "0x20267864";
    private const string StaminaAddr = "0x20267830";
    private const string TimeAddr = "0x2085A2F4";

    private const int DwmUseImmersiveDarkMode = 20;

    private readonly System.Windows.Forms.Timer _timer = new();
    private PineClient? _pine;

    private Panel _hud = null!;
    private Panel _monitor = null!;
    private Panel _guide = null!;
    private Panel _staminaTrack = null!;
    private Panel _staminaFill = null!;
    private Label _staminaLabel = null!;
    private Label _moneyValue = null!;
    private Label _weatherValue = null!;
    private Label _monGold = null!;
    private Label _monStamina = null!;
    private Label _monTime = null!;
    private Label _monFps = null!;
    private ProgressBar _guideBar = null!;
    private Label _guideText = null!;
    private FlowLayoutPanel _strip = null!;
    private Label _stripVersion = null!;
    private Label _stripSerial = null!;
    private Panel _stateDot = null!;
    private Label _stripState = null!;

    public MainForm()
    {
        Text = "HM · STH Companion";
        ClientSize = new Size(660, 440);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = Theme.Regular;
        BuildLayout();
        SetState(AppState.Disconnected, "Enable PINE IPC in PCSX2, then start a game");
        _timer.Interval = 1500;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int v = 1;
        DwmSetWindowAttribute(Handle, DwmUseImmersiveDarkMode, ref v, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private void BuildLayout()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Theme.Bg,
            Padding = new Padding(8),
        };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

        _hud = MakePanel("GAME HUD");
        _staminaTrack = new Panel { Dock = DockStyle.Top, Height = 14, BackColor = Theme.Surface1 };
        _staminaFill = new Panel { Dock = DockStyle.Left, Width = 0, BackColor = Theme.Accent };
        _staminaTrack.Controls.Add(_staminaFill);
        _staminaLabel = ValueLabel("Stamina  — / —");
        _moneyValue = ValueLabel("Money    — G");
        _weatherValue = ValueLabel("Weather  —");
        _hud.Controls.Add(_weatherValue);
        _hud.Controls.Add(_moneyValue);
        _hud.Controls.Add(_staminaLabel);
        _hud.Controls.Add(_staminaTrack);

        _monitor = MakePanel("MEMORY MONITOR");
        _monGold = ValueLabel($"{GoldAddr}  —");
        _monStamina = ValueLabel($"{StaminaAddr}  —");
        _monTime = ValueLabel($"{TimeAddr}  —");
        _monFps = MutedLabel("FPS       —");
        _monitor.Controls.Add(_monFps);
        _monitor.Controls.Add(_monTime);
        _monitor.Controls.Add(_monStamina);
        _monitor.Controls.Add(_monGold);

        _guide = MakePanel("GUIDE");
        _guideBar = new ProgressBar
        {
            Dock = DockStyle.Top,
            Height = 16,
            Maximum = 100,
            Value = 0,
            BackColor = Theme.Surface1,
            ForeColor = Theme.Accent,
        };
        _guideText = MutedLabel("Year ? — Ending ?  (save profile pending, ENH-011)");
        _guide.Controls.Add(_guideText);
        _guide.Controls.Add(_guideBar);

        _strip = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6, 4, 6, 4),
        };
        _stateDot = new Panel { Size = new Size(12, 12), Margin = new Padding(0, 4, 8, 0), BackColor = Color.Gray };
        _stripState = new Label { AutoSize = true, ForeColor = Theme.TextMuted, Font = Theme.Mono };
        _stripVersion = new Label { AutoSize = true, ForeColor = Theme.TextMuted, Font = Theme.Mono, Margin = new Padding(0, 4, 12, 0) };
        _stripSerial = new Label { AutoSize = true, ForeColor = Theme.TextMuted, Font = Theme.Mono, Margin = new Padding(0, 4, 12, 0) };
        _strip.Controls.Add(_stateDot);
        _strip.Controls.Add(_stripState);
        _strip.Controls.Add(_stripVersion);
        _strip.Controls.Add(_stripSerial);

        main.Controls.Add(_hud, 0, 0);
        main.Controls.Add(_monitor, 1, 0);
        main.Controls.Add(_guide, 0, 1);
        main.SetColumnSpan(_guide, 2);
        main.Controls.Add(_strip, 0, 2);
        main.SetColumnSpan(_strip, 2);

        Controls.Add(main);
    }

    private static Panel MakePanel(string title)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Mantle, Padding = new Padding(8) };
        p.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Text = title,
            ForeColor = Theme.Accent,
            Font = Theme.Label,
            Height = 16,
        });
        return p;
    }

    private static Label ValueLabel(string text) => new()
    {
        Dock = DockStyle.Top,
        Text = text,
        ForeColor = Theme.Text,
        Font = Theme.Mono,
        Height = 20,
        Padding = new Padding(0, 2, 0, 2),
    };

    private static Label MutedLabel(string text) => new()
    {
        Dock = DockStyle.Top,
        Text = text,
        ForeColor = Theme.TextMuted,
        Font = Theme.Mono,
        Height = 20,
        Padding = new Padding(0, 2, 0, 2),
    };

    private void SetState(AppState state, string hint)
    {
        Color dot = state switch
        {
            AppState.Playing => Theme.Accent,
            AppState.WrongGame => Color.Gold,
            _ => Color.Gray,
        };
        _stateDot.BackColor = dot;
        _stripState.Text = state switch
        {
            AppState.Playing => "Playing",
            AppState.WrongGame => "Wrong game",
            _ => "Disconnected",
        };
        _stripState.ForeColor = dot;
        _hud.Enabled = state != AppState.Disconnected;
        _monitor.Enabled = state == AppState.Playing;
        _guide.Enabled = state == AppState.Playing;
        if (state == AppState.Disconnected)
        {
            _stripVersion.Text = hint;
            _stripSerial.Text = string.Empty;
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_pine is null)
        {
            try
            {
                _pine = new PineClient(Host, Port);
                _pine.Connect();
            }
            catch
            {
                _pine?.Dispose();
                _pine = null;
                SetState(AppState.Disconnected, "Enable PINE IPC in PCSX2, then start a game");
                return;
            }
        }

        try
        {
            string version = _pine.ReadString(PineCommand.Version);
            string serial = _pine.ReadString(PineCommand.Id);
            string title = _pine.ReadString(PineCommand.Title);

            _stripVersion.Text = version;
            _stripSerial.Text = serial;

            if (!string.Equals(serial, ExpectedSerial, StringComparison.OrdinalIgnoreCase))
            {
                SetState(AppState.WrongGame, "Wrong game");
                _moneyValue.Text = "Money    — G";
                _staminaLabel.Text = "Stamina  — / —";
                _weatherValue.Text = "Weather  —";
                _monGold.Text = $"{GoldAddr}  —";
                _monStamina.Text = $"{StaminaAddr}  —";
                _monTime.Text = $"{TimeAddr}  —";
                return;
            }

            var reader = new GameMemoryReader(_pine);
            GoldReading gold = reader.ReadGold();
            StaminaReading stamina = reader.ReadStamina();
            TimeReading time = reader.ReadTime();
            WeatherReading weather = reader.ReadWeather();

            _moneyValue.Text = $"Money    {gold}";
            _staminaLabel.Text = $"Stamina  {stamina.Stamina}/{stamina.MaxStamina}";
            _staminaFill.Width = stamina.MaxStamina == 0
                ? 0
                : (int)(_staminaTrack.Width * (stamina.Stamina / (float)stamina.MaxStamina));
            _weatherValue.Text = $"Weather  {weather.Description}";

            _monGold.Text = $"{GoldAddr}  {gold}";
            _monStamina.Text = $"{StaminaAddr}  {stamina.Stamina}/{stamina.MaxStamina}";
            _monTime.Text = $"{TimeAddr}  {time}";
            _monFps.Text = "FPS       —";

            _guideText.Text = $"{title} — Year ? / Ending ?  (save profile pending)";
            SetState(AppState.Playing, "Playing");
        }
        catch (PineConnectionException)
        {
            _pine.Dispose();
            _pine = null;
            SetState(AppState.Disconnected, "PINE no response — is the game in-game?");
        }
        catch (IOException)
        {
            _pine.Dispose();
            _pine = null;
            SetState(AppState.Disconnected, "Connection lost");
        }
    }
}
