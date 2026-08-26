using System.Drawing;
using System.Windows.Forms;

namespace HmSth.App;

// Theme tokens copied verbatim from docs/UI_DESIGN_SPEC.md (social-preview.html).
internal static class Theme
{
    public static readonly Color Bg = ColorTranslator.FromHtml("#0b0e14");
    public static readonly Color Mantle = ColorTranslator.FromHtml("#12151e");
    public static readonly Color Surface = ColorTranslator.FromHtml("#191d2b");
    public static readonly Color Surface1 = ColorTranslator.FromHtml("#242a3d");
    public static readonly Color Text = ColorTranslator.FromHtml("#e6e8ee");
    public static readonly Color TextMuted = ColorTranslator.FromHtml("#9aa1b5");
    public static readonly Color Accent = ColorTranslator.FromHtml("#58a55c");
    public static readonly Font Regular = new("Segoe UI", 10F);
    public static readonly Font Mono = new("Consolas", 10F);
    public static readonly Font Label = new("Consolas", 9F, FontStyle.Bold);
}
