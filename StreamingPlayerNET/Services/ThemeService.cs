using System.Drawing;
using System.Windows.Forms;
using NLog;
using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Services;

public static class ThemeService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // Dark mode color scheme
    public static class DarkColors
    {
        public static Color Background = Color.FromArgb(32, 32, 32);
        public static Color Foreground = Color.FromArgb(255, 255, 255);
        public static Color Control = Color.FromArgb(45, 45, 45);
        public static Color ControlLight = Color.FromArgb(60, 60, 60);
        public static Color ControlDark = Color.FromArgb(25, 25, 25);
        public static Color Highlight = Color.FromArgb(0, 120, 215);
        public static Color MenuBackground = Color.FromArgb(45, 45, 45);
        public static Color MenuForeground = Color.FromArgb(255, 255, 255);
        public static Color ListViewBackground = Color.FromArgb(32, 32, 32);
        public static Color ListViewForeground = Color.FromArgb(255, 255, 255);
        public static Color ListViewSelected = Color.FromArgb(0, 120, 215);
        public static Color ListViewSelectedText = Color.FromArgb(255, 255, 255);
        public static Color TabBackground = Color.FromArgb(45, 45, 45);
        public static Color TabForeground = Color.FromArgb(255, 255, 255);
        public static Color StatusStripBackground = Color.FromArgb(45, 45, 45);
        public static Color StatusStripForeground = Color.FromArgb(255, 255, 255);
    }

    // Black mode color scheme
    public static class BlackColors
    {
        public static Color Background = Color.FromArgb(0, 0, 0);
        public static Color Foreground = Color.FromArgb(255, 255, 255);
        public static Color Control = Color.FromArgb(20, 20, 20);
        public static Color ControlLight = Color.FromArgb(30, 30, 30);
        public static Color ControlDark = Color.FromArgb(10, 10, 10);
        public static Color Highlight = Color.FromArgb(0, 120, 215);
        public static Color MenuBackground = Color.FromArgb(20, 20, 20);
        public static Color MenuForeground = Color.FromArgb(255, 255, 255);
        public static Color ListViewBackground = Color.FromArgb(0, 0, 0);
        public static Color ListViewForeground = Color.FromArgb(255, 255, 255);
        public static Color ListViewSelected = Color.FromArgb(0, 120, 215);
        public static Color ListViewSelectedText = Color.FromArgb(255, 255, 255);
        public static Color TabBackground = Color.FromArgb(20, 20, 20);
        public static Color TabForeground = Color.FromArgb(255, 255, 255);
        public static Color StatusStripBackground = Color.FromArgb(20, 20, 20);
        public static Color StatusStripForeground = Color.FromArgb(255, 255, 255);
    }

    // Light mode color scheme (default Windows colors)
    public static class LightColors
    {
        public static Color Background = SystemColors.Control;
        public static Color Foreground = SystemColors.ControlText;
        public static Color Control = SystemColors.Control;
        public static Color ControlLight = SystemColors.ControlLight;
        public static Color ControlDark = SystemColors.ControlDark;
        public static Color Highlight = SystemColors.Highlight;
        public static Color MenuBackground = SystemColors.Menu;
        public static Color MenuForeground = SystemColors.MenuText;
        public static Color ListViewBackground = SystemColors.Window;
        public static Color ListViewForeground = SystemColors.WindowText;
        public static Color ListViewSelected = SystemColors.Highlight;
        public static Color ListViewSelectedText = SystemColors.HighlightText;
        public static Color TabBackground = SystemColors.Control;
        public static Color TabForeground = SystemColors.ControlText;
        public static Color StatusStripBackground = SystemColors.Control;
        public static Color StatusStripForeground = SystemColors.ControlText;
    }

    public static void ApplyTheme(Form form, AppTheme theme)
    {
        try
        {
            Logger.Debug($"Applying {theme} theme to form: {form.Name}");
            
            // Apply theme to the form itself
            switch (theme)
            {
                case AppTheme.Dark:
                    form.BackColor = DarkColors.Background;
                    form.ForeColor = DarkColors.Foreground;
                    break;
                case AppTheme.Black:
                    form.BackColor = BlackColors.Background;
                    form.ForeColor = BlackColors.Foreground;
                    break;
                default: // AppTheme.Light
                    form.BackColor = LightColors.Background;
                    form.ForeColor = LightColors.Foreground;
                    break;
            }
            
            // Apply theme to all controls recursively
            ApplyThemeToControl(form, theme);
            
            Logger.Debug($"Theme applied successfully to form: {form.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error applying theme to form: {form.Name}");
        }
    }

    private static void ApplyThemeToControl(Control control, AppTheme theme)
    {
        try
        {
            // Apply theme based on control type
            switch (control)
            {
                case MenuStrip menuStrip:
                    ApplyMenuStripTheme(menuStrip, theme);
                    break;
                case TabControl tabControl:
                    ApplyTabControlTheme(tabControl, theme);
                    break;
                case ListView listView:
                    ApplyListViewTheme(listView, theme);
                    break;
                case ListBox listBox:
                    ApplyListBoxTheme(listBox, theme);
                    break;
                case TextBox textBox:
                    ApplyTextBoxTheme(textBox, theme);
                    break;
                case Button button:
                    ApplyButtonTheme(button, theme);
                    break;
                case TrackBar trackBar:
                    ApplyTrackBarTheme(trackBar, theme);
                    break;
                case ProgressBar progressBar:
                    ApplyProgressBarTheme(progressBar, theme);
                    break;
                case StatusStrip statusStrip:
                    ApplyStatusStripTheme(statusStrip, theme);
                    break;
                case Panel panel:
                    ApplyPanelTheme(panel, theme);
                    break;
                case Label label:
                    ApplyLabelTheme(label, theme);
                    break;
                default:
                    // Apply basic colors to unknown controls
                    switch (theme)
                    {
                        case AppTheme.Dark:
                            control.BackColor = DarkColors.Control;
                            control.ForeColor = DarkColors.Foreground;
                            break;
                        case AppTheme.Black:
                            control.BackColor = BlackColors.Control;
                            control.ForeColor = BlackColors.Foreground;
                            break;
                        default: // AppTheme.Light
                            control.BackColor = LightColors.Control;
                            control.ForeColor = LightColors.Foreground;
                            break;
                    }
                    break;
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, theme);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to apply theme to control: {control.Name} ({control.GetType().Name})");
        }
    }

    private static void ApplyMenuStripTheme(MenuStrip menuStrip, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                menuStrip.BackColor = DarkColors.MenuBackground;
                menuStrip.ForeColor = DarkColors.MenuForeground;
                menuStrip.Renderer = new DarkModeToolStripRenderer(true);
                break;
            case AppTheme.Black:
                menuStrip.BackColor = BlackColors.MenuBackground;
                menuStrip.ForeColor = BlackColors.MenuForeground;
                menuStrip.Renderer = new DarkModeToolStripRenderer(true);
                break;
            default: // AppTheme.Light
                menuStrip.BackColor = LightColors.MenuBackground;
                menuStrip.ForeColor = LightColors.MenuForeground;
                menuStrip.Renderer = new DarkModeToolStripRenderer(false);
                break;
        }
    }

    private static void ApplyTabControlTheme(TabControl tabControl, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                tabControl.BackColor = DarkColors.TabBackground;
                tabControl.ForeColor = DarkColors.TabForeground;
                break;
            case AppTheme.Black:
                tabControl.BackColor = BlackColors.TabBackground;
                tabControl.ForeColor = BlackColors.TabForeground;
                break;
            default: // AppTheme.Light
                tabControl.BackColor = LightColors.TabBackground;
                tabControl.ForeColor = LightColors.TabForeground;
                break;
        }
        
        // Apply theme to individual tab pages
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            switch (theme)
            {
                case AppTheme.Dark:
                    tabPage.BackColor = DarkColors.Background;
                    tabPage.ForeColor = DarkColors.Foreground;
                    break;
                case AppTheme.Black:
                    tabPage.BackColor = BlackColors.Background;
                    tabPage.ForeColor = BlackColors.Foreground;
                    break;
                default: // AppTheme.Light
                    tabPage.BackColor = LightColors.Background;
                    tabPage.ForeColor = LightColors.Foreground;
                    break;
            }
        }
    }

    private static void ApplyListViewTheme(ListView listView, AppTheme theme)
    {
        try
        {
            // Validate that the ListView is properly initialized
            if (listView == null || listView.IsDisposed)
            {
                Logger.Debug("ListView is null or disposed, skipping theme application");
                return;
            }

            switch (theme)
            {
                case AppTheme.Dark:
                    listView.BackColor = DarkColors.ListViewBackground;
                    listView.ForeColor = DarkColors.ListViewForeground;
                    break;
                case AppTheme.Black:
                    listView.BackColor = BlackColors.ListViewBackground;
                    listView.ForeColor = BlackColors.ListViewForeground;
                    break;
                default: // AppTheme.Light
                    listView.BackColor = LightColors.ListViewBackground;
                    listView.ForeColor = LightColors.ListViewForeground;
                    break;
            }
            
            // Set ListView properties for better appearance
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            
            // Note: Header theming is disabled to avoid Windows API issues
            Logger.Debug("ListView basic theme applied successfully");
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Failed to apply ListView theme");
        }
    }

    private static void ApplyListViewHeaderTheme(ListView listView, AppTheme theme)
    {
        try
        {
            // Validate that the ListView is properly initialized
            if (listView == null || listView.IsDisposed)
            {
                Logger.Debug("ListView is null or disposed, skipping header theme");
                return;
            }

            // Note: Windows API calls for ListView header theming are problematic
            // and can cause ExecutionEngineException. We'll skip header theming
            // and rely on the basic ListView theming instead.
            Logger.Debug("Skipping ListView header theming to avoid Windows API issues");
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Failed to apply ListView header theme");
        }
    }

    private static void ApplyListBoxTheme(ListBox listBox, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                listBox.BackColor = DarkColors.ListViewBackground;
                listBox.ForeColor = DarkColors.ListViewForeground;
                break;
            case AppTheme.Black:
                listBox.BackColor = BlackColors.ListViewBackground;
                listBox.ForeColor = BlackColors.ListViewForeground;
                break;
            default: // AppTheme.Light
                listBox.BackColor = LightColors.ListViewBackground;
                listBox.ForeColor = LightColors.ListViewForeground;
                break;
        }
        // Note: ListBox doesn't have SelectedBackColor/SelectedForeColor properties in .NET Framework
    }

    private static void ApplyTextBoxTheme(TextBox textBox, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                textBox.BackColor = DarkColors.ControlLight;
                textBox.ForeColor = DarkColors.Foreground;
                break;
            case AppTheme.Black:
                textBox.BackColor = BlackColors.ControlLight;
                textBox.ForeColor = BlackColors.Foreground;
                break;
            default: // AppTheme.Light
                textBox.BackColor = LightColors.ControlLight;
                textBox.ForeColor = LightColors.Foreground;
                break;
        }
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyButtonTheme(Button button, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                button.BackColor = DarkColors.Control;
                button.ForeColor = DarkColors.Foreground;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = DarkColors.ControlDark;
                button.FlatAppearance.MouseOverBackColor = DarkColors.ControlLight;
                button.FlatAppearance.MouseDownBackColor = DarkColors.Highlight;
                break;
            case AppTheme.Black:
                button.BackColor = BlackColors.Control;
                button.ForeColor = BlackColors.Foreground;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = BlackColors.ControlDark;
                button.FlatAppearance.MouseOverBackColor = BlackColors.ControlLight;
                button.FlatAppearance.MouseDownBackColor = BlackColors.Highlight;
                break;
            default: // AppTheme.Light
                button.BackColor = LightColors.Control;
                button.ForeColor = LightColors.Foreground;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = LightColors.ControlDark;
                button.FlatAppearance.MouseOverBackColor = LightColors.ControlLight;
                button.FlatAppearance.MouseDownBackColor = LightColors.Highlight;
                break;
        }
        
        // Special handling for playback control buttons to ensure they're visible
        if (button.Text.Contains("▶") || button.Text.Contains("⏸") || button.Text.Contains("⏹") || 
            button.Text.Contains("⏮") || button.Text.Contains("⏭") || button.Text.Contains("🔀") || 
            button.Text.Contains("🔁") || button.Text.Contains("Play") || button.Text.Contains("Stop") ||
            button.Text.Contains("Previous") || button.Text.Contains("Next") || button.Text.Contains("Repeat") ||
            button.Text.Contains("Shuffle"))
        {
            // Ensure playback buttons have good contrast
            switch (theme)
            {
                case AppTheme.Dark:
                    button.BackColor = DarkColors.ControlLight;
                    button.ForeColor = DarkColors.Foreground;
                    break;
                case AppTheme.Black:
                    button.BackColor = BlackColors.ControlLight;
                    button.ForeColor = BlackColors.Foreground;
                    break;
                default: // AppTheme.Light
                    button.BackColor = LightColors.ControlLight;
                    button.ForeColor = LightColors.Foreground;
                    break;
            }
        }
    }

    private static void ApplyTrackBarTheme(TrackBar trackBar, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                trackBar.BackColor = DarkColors.Control;
                trackBar.ForeColor = DarkColors.Foreground;
                break;
            case AppTheme.Black:
                trackBar.BackColor = BlackColors.Control;
                trackBar.ForeColor = BlackColors.Foreground;
                break;
            default: // AppTheme.Light
                trackBar.BackColor = LightColors.Control;
                trackBar.ForeColor = LightColors.Foreground;
                break;
        }
    }

    private static void ApplyProgressBarTheme(ProgressBar progressBar, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                progressBar.BackColor = DarkColors.ControlDark;
                progressBar.ForeColor = DarkColors.Highlight;
                break;
            case AppTheme.Black:
                progressBar.BackColor = BlackColors.ControlDark;
                progressBar.ForeColor = BlackColors.Highlight;
                break;
            default: // AppTheme.Light
                progressBar.BackColor = LightColors.ControlDark;
                progressBar.ForeColor = LightColors.Highlight;
                break;
        }
    }

    private static void ApplyStatusStripTheme(StatusStrip statusStrip, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                statusStrip.BackColor = DarkColors.StatusStripBackground;
                statusStrip.ForeColor = DarkColors.StatusStripForeground;
                statusStrip.Renderer = new DarkModeToolStripRenderer(true);
                break;
            case AppTheme.Black:
                statusStrip.BackColor = BlackColors.StatusStripBackground;
                statusStrip.ForeColor = BlackColors.StatusStripForeground;
                statusStrip.Renderer = new DarkModeToolStripRenderer(true);
                break;
            default: // AppTheme.Light
                statusStrip.BackColor = LightColors.StatusStripBackground;
                statusStrip.ForeColor = LightColors.StatusStripForeground;
                statusStrip.Renderer = new DarkModeToolStripRenderer(false);
                break;
        }
    }

    private static void ApplyPanelTheme(Panel panel, AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                panel.BackColor = DarkColors.Background;
                panel.ForeColor = DarkColors.Foreground;
                break;
            case AppTheme.Black:
                panel.BackColor = BlackColors.Background;
                panel.ForeColor = BlackColors.Foreground;
                break;
            default: // AppTheme.Light
                panel.BackColor = LightColors.Background;
                panel.ForeColor = LightColors.Foreground;
                break;
        }
    }

    private static void ApplyLabelTheme(Label label, AppTheme theme)
    {
        label.BackColor = Color.Transparent;
        switch (theme)
        {
            case AppTheme.Dark:
                label.ForeColor = DarkColors.Foreground;
                break;
            case AppTheme.Black:
                label.ForeColor = BlackColors.Foreground;
                break;
            default: // AppTheme.Light
                label.ForeColor = LightColors.Foreground;
                break;
        }
    }
}

// Custom renderer for dark mode tool strips
public class DarkModeToolStripRenderer : ToolStripProfessionalRenderer
{
    private readonly bool _isDarkMode;

    public DarkModeToolStripRenderer(bool isDarkMode)
    {
        _isDarkMode = isDarkMode;
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            var highlightColor = _isDarkMode ? ThemeService.DarkColors.Highlight : ThemeService.LightColors.Highlight;
            using var brush = new SolidBrush(highlightColor);
            e.Graphics.FillRectangle(brush, e.Item.Bounds);
        }
        else
        {
            var menuBackground = _isDarkMode ? ThemeService.DarkColors.MenuBackground : ThemeService.LightColors.MenuBackground;
            using var brush = new SolidBrush(menuBackground);
            e.Graphics.FillRectangle(brush, e.Item.Bounds);
        }
    }

    protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            var highlightColor = _isDarkMode ? ThemeService.DarkColors.Highlight : ThemeService.LightColors.Highlight;
            using var brush = new SolidBrush(highlightColor);
            e.Graphics.FillRectangle(brush, e.Item.Bounds);
        }
        else
        {
            var menuBackground = _isDarkMode ? ThemeService.DarkColors.MenuBackground : ThemeService.LightColors.MenuBackground;
            using var brush = new SolidBrush(menuBackground);
            e.Graphics.FillRectangle(brush, e.Item.Bounds);
        }
    }
} 