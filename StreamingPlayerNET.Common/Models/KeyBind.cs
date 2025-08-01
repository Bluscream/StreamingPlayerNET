using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StreamingPlayerNET.Common.Models;

public class KeyBind
{
    [Category("Hotkey")]
    [DisplayName("Enabled")]
    [Description("Whether this hotkey is enabled")]
    public bool Enabled { get; set; } = true;

    [Category("Hotkey")]
    [DisplayName("Key Combination")]
    [Description("The key combination for this hotkey (e.g., 'Ctrl+Space', 'F1')")]
    public string Combo { get; set; } = string.Empty;

    public KeyBind()
    {
    }

    public KeyBind(string combo, bool enabled = true)
    {
        Combo = combo;
        Enabled = enabled;
    }

    public KeyBind(string combo)
    {
        Combo = combo;
        Enabled = true;
    }

    public override string ToString()
    {
        return Enabled ? Combo : $"{Combo} (Disabled)";
    }

    public static implicit operator string(KeyBind keyBind)
    {
        return keyBind.Enabled ? keyBind.Combo : string.Empty;
    }

    public static implicit operator KeyBind(string combo)
    {
        return new KeyBind(combo);
    }
}