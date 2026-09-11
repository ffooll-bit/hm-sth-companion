using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
    private const int RefreshIntervalMs = 400;

    private const int DwmUseImmersiveDarkMode = 20;

    private CancellationTokenSource? _cts;
    private Task? _refreshLoop;
    private PineClient? _pine;
    private string? _cachedVersion;
    private string? _cachedSerial;
    private string? _cachedTitle;

    private Panel _hud = null!;
    private Panel _monitor = null!;
    private Panel _guide = null!;
    private Panel _staminaTrack = null!;
    private Panel _staminaFill = null!;
    private Label _staminaLabel = null!;
    private Label _moneyValue = null!;
    private Label _weatherValue = null!;
    private Label _toolValue = null!;
    private Label _itemValue = null!;
    private Label _shopStatus = null!;
    private Label _shopMap = null!;
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
        MinimumSize = new Size(420, 300);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Bg;
        ForeColor = Theme.Text;
        Font = Theme.Regular;
        AutoScaleMode = AutoScaleMode.Font;
        AccessibleName = "HM · STH Companion";
        BuildLayout();
        _hud.AccessibleName = "Game HUD";
        _hud.AccessibleRole = AccessibleRole.Pane;
        _monitor.AccessibleName = "Memory Monitor";
        _monitor.AccessibleRole = AccessibleRole.Pane;
        _guide.AccessibleName = "Guide";
        _guide.AccessibleRole = AccessibleRole.Pane;
        _strip.AccessibleName = "Connection status";
        SetState(AppState.Disconnected, "Enable PINE IPC in PCSX2, then start a game");
        _cts = new CancellationTokenSource();
        _refreshLoop = Task.Run(() => RefreshLoopAsync(_cts.Token));
        FormClosing += MainForm_FormClosing;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _cts?.Cancel();
        _pine?.Dispose();
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
        _toolValue = ValueLabel("Tool     —");
        _itemValue = ValueLabel("Item     —");
        _hud.Controls.Add(_itemValue);
        _hud.Controls.Add(_toolValue);
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
        _shopStatus = NewShopLine();
        _shopMap = MutedLabel(ShopMapText());
        _guide.Controls.Add(_shopMap);
        _guide.Controls.Add(_shopStatus);
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

    private async Task RefreshLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                RefreshOnce();
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await Task.Delay(RefreshIntervalMs, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void RefreshOnce()
    {
        if (_pine is null)
        {
            try
            {
                _pine = new PineClient(Host, Port);
                _pine.Connect();
                _cachedVersion = _pine.ReadString(PineCommand.Version);
                _cachedSerial = _pine.ReadString(PineCommand.Id);
                _cachedTitle = _pine.ReadString(PineCommand.Title);
            }
            catch (Exception)
            {
                _pine?.Dispose();
                _pine = null;
                _cachedVersion = _cachedSerial = _cachedTitle = null;
                UpdateUiDisconnected("Enable PINE IPC in PCSX2, then start a game");
                return;
            }
        }

        if (!string.Equals(_cachedSerial, ExpectedSerial, StringComparison.OrdinalIgnoreCase))
        {
            UpdateUiWrongGame();
            _pine?.Dispose();
            _pine = null;
            _cachedVersion = _cachedSerial = _cachedTitle = null;
            return;
        }

        try
        {
            var reader = new GameMemoryReader(_pine!);
            GoldReading gold = reader.ReadGold();
            StaminaReading stamina = reader.ReadStamina();
            TimeReading time = reader.ReadTime();
            WeatherReading weather = reader.ReadWeather();
            ToolReading tool = reader.ReadTool();
            ItemReading item = reader.ReadItem();
            UpdateUiPlaying(gold, stamina, time, weather, tool, item, _cachedTitle!, _cachedVersion!, _cachedSerial!);
        }
        catch (PineConnectionException)
        {
            _pine?.Dispose();
            _pine = null;
            _cachedVersion = _cachedSerial = _cachedTitle = null;
            UpdateUiDisconnected("PINE no response — is the game in-game?");
        }
        catch (IOException)
        {
            _pine?.Dispose();
            _pine = null;
            _cachedVersion = _cachedSerial = _cachedTitle = null;
            UpdateUiDisconnected("Connection lost");
        }
    }

    private void UpdateUiDisconnected(string hint)
    {
        RunOnUi(() =>
        {
            SetState(AppState.Disconnected, hint);
            _moneyValue.Text = "Money    — G";
            _staminaLabel.Text = "Stamina  — / —";
            _weatherValue.Text = "Weather  —";
            _toolValue.Text = "Tool     —";
            _itemValue.Text = "Item     —";
            _monGold.Text = $"{GoldAddr}  —";
            _monStamina.Text = $"{StaminaAddr}  —";
            _monTime.Text = $"{TimeAddr}  —";
            _monFps.Text = "FPS       —";
            _guideText.Text = "Year ? — Ending ?  (save profile pending, ENH-011)";
            _shopStatus.Text = "Shops —";
        });
    }

    private void UpdateUiWrongGame()
    {
        RunOnUi(() =>
        {
            SetState(AppState.WrongGame, "Wrong game");
            _moneyValue.Text = "Money    — G";
            _staminaLabel.Text = "Stamina  — / —";
            _weatherValue.Text = "Weather  —";
            _toolValue.Text = "Tool     —";
            _itemValue.Text = "Item     —";
            _monGold.Text = $"{GoldAddr}  —";
            _monStamina.Text = $"{StaminaAddr}  —";
            _monTime.Text = $"{TimeAddr}  —";
            _guideText.Text = $"{_cachedTitle} — Year ? / Ending ?  (save profile pending)";
            _shopStatus.Text = "Shops —";
            _stripVersion.Text = _cachedVersion!;
            _stripSerial.Text = _cachedSerial!;
        });
    }

    private void UpdateUiPlaying(GoldReading gold, StaminaReading stamina, TimeReading time, WeatherReading weather, ToolReading tool, ItemReading item, string title, string version, string serial)
    {
        RunOnUi(() =>
        {
            _moneyValue.Text = $"Money    {gold}";
            _staminaLabel.Text = $"Stamina  {stamina.Stamina}/{stamina.MaxStamina}";
            _staminaFill.Width = stamina.MaxStamina == 0
                ? 0
                : (int)(_staminaTrack.Width * (stamina.Stamina / (float)stamina.MaxStamina));
            _weatherValue.Text = $"Weather  {weather}";
            _toolValue.Text = $"Tool     {tool}";
            _itemValue.Text = $"Item     {item}";

            _monGold.Text = $"{GoldAddr}  {gold}";
            _monStamina.Text = $"{StaminaAddr}  {stamina.Stamina}/{stamina.MaxStamina}";
            _monTime.Text = $"{TimeAddr}  {time}";
            _monFps.Text = "FPS       —";

            _guideText.Text = $"{title} — Year ? / Ending ?  (save profile pending)";
            _shopStatus.Text = ShopStatusText(time);
            _stripVersion.Text = version;
            _stripSerial.Text = serial;
            SetState(AppState.Playing, "Playing");
        });
    }

    private static readonly string[] WeekdayNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    // Spring 1 is always Monday; every season has 30 days, and 30 % 7 = 2, so each season
    // starts two weekdays later than the previous one: Summer 1 = Wed, Fall 1 = Fri, Winter 1 = Sun.
    private static readonly int[] SeasonStartWeekday = { 0, 2, 4, 6 };

    // Curated from Ushi No Tane townshops.php (8 shops, ENH-009). ClosedDays: 0=Mon..6=Sun.
    private static readonly (string Name, int[] ClosedDays)[] Shops =
    {
        ("Sunny Bar", new[] { 0 }),
        ("Sunny Cafe", new[] { 0 }),
        ("Louis's", new[] { 2, 5 }),
        ("Carpenter", new[] { 1, 3 }),
        ("Clove's Villa", new[] { 1, 3 }),
        ("Farmer's", new[] { 3 }),
        ("Lyla's Flowers", new[] { 6 }),
        ("Supermarket", new[] { 6 }),
    };

    private static int WeekdayOf(TimeReading time) => (time.Day - 1 + SeasonStartWeekday[time.Season]) % 7;

    private static string ShopMapText() =>
        "Closures: Mon–Bar/Cafe · Tue/Thu–Carpenter/Clove's · Wed/Sat–Louis's · Thu–Farmer's · Sun–Lyla's/Supermarket";

    private static string ShopStatusText(TimeReading time)
    {
        int today = WeekdayOf(time);
        string closed = string.Join(", ", Shops.Where(s => s.ClosedDays.Contains(today)).Select(s => s.Name));
        return closed.Length == 0
            ? $"All shops open ({WeekdayNames[today]})"
            : $"Closed today ({WeekdayNames[today]}): {closed}";
    }

    private static Label NewShopLine() => new()
    {
        Dock = DockStyle.Top,
        Text = "Shops —",
        ForeColor = Theme.Accent,
        Font = Theme.Mono,
        Height = 20,
        Padding = new Padding(0, 4, 0, 2),
    };

    private void RunOnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }
}
