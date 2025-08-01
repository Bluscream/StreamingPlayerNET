using System.Drawing;
using System.Windows.Forms;
using NLog;

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

    public static void ApplyTheme(Form form, bool isDarkMode)
    {
        try
        {
            Logger.Debug($"Applying {(isDarkMode ? "dark" : "light")} theme to form: {form.Name}");
            
            // Apply theme to the form itself
            form.BackColor = isDarkMode ? DarkColors.Background : LightColors.Background;
            form.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
            
            // Apply theme to all controls recursively
            ApplyThemeToControl(form, isDarkMode);
            
            Logger.Debug($"Theme applied successfully to form: {form.Name}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error applying theme to form: {form.Name}");
        }
    }

    private static void ApplyThemeToControl(Control control, bool isDarkMode)
    {
        try
        {
            // Apply theme based on control type
            switch (control)
            {
                case MenuStrip menuStrip:
                    ApplyMenuStripTheme(menuStrip, isDarkMode);
                    break;
                case TabControl tabControl:
                    ApplyTabControlTheme(tabControl, isDarkMode);
                    break;
                case ListView listView:
                    ApplyListViewTheme(listView, isDarkMode);
                    break;
                case ListBox listBox:
                    ApplyListBoxTheme(listBox, isDarkMode);
                    break;
                case TextBox textBox:
                    ApplyTextBoxTheme(textBox, isDarkMode);
                    break;
                case Button button:
                    ApplyButtonTheme(button, isDarkMode);
                    break;
                case TrackBar trackBar:
                    ApplyTrackBarTheme(trackBar, isDarkMode);
                    break;
                case ProgressBar progressBar:
                    ApplyProgressBarTheme(progressBar, isDarkMode);
                    break;
                case StatusStrip statusStrip:
                    ApplyStatusStripTheme(statusStrip, isDarkMode);
                    break;
                case Panel panel:
                    ApplyPanelTheme(panel, isDarkMode);
                    break;
                case Label label:
                    ApplyLabelTheme(label, isDarkMode);
                    break;
                default:
                    // Apply basic colors to unknown controls
                    control.BackColor = isDarkMode ? DarkColors.Control : LightColors.Control;
                    control.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
                    break;
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, isDarkMode);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to apply theme to control: {control.Name} ({control.GetType().Name})");
        }
    }

    private static void ApplyMenuStripTheme(MenuStrip menuStrip, bool isDarkMode)
    {
        menuStrip.BackColor = isDarkMode ? DarkColors.MenuBackground : LightColors.MenuBackground;
        menuStrip.ForeColor = isDarkMode ? DarkColors.MenuForeground : LightColors.MenuForeground;
        menuStrip.Renderer = new DarkModeToolStripRenderer(isDarkMode);
    }

    private static void ApplyTabControlTheme(TabControl tabControl, bool isDarkMode)
    {
        tabControl.BackColor = isDarkMode ? DarkColors.TabBackground : LightColors.TabBackground;
        tabControl.ForeColor = isDarkMode ? DarkColors.TabForeground : LightColors.TabForeground;
        
        // Apply theme to individual tab pages
        foreach (TabPage tabPage in tabControl.TabPages)
        {
            tabPage.BackColor = isDarkMode ? DarkColors.Background : LightColors.Background;
            tabPage.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
        }
    }

    private static void ApplyListViewTheme(ListView listView, bool isDarkMode)
    {
        try
        {
            // Validate that the ListView is properly initialized
            if (listView == null || listView.IsDisposed)
            {
                Logger.Debug("ListView is null or disposed, skipping theme application");
                return;
            }

            listView.BackColor = isDarkMode ? DarkColors.ListViewBackground : LightColors.ListViewBackground;
            listView.ForeColor = isDarkMode ? DarkColors.ListViewForeground : LightColors.ListViewForeground;
            
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

    private static void ApplyListViewHeaderTheme(ListView listView, bool isDarkMode)
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

    private static void ApplyListBoxTheme(ListBox listBox, bool isDarkMode)
    {
        listBox.BackColor = isDarkMode ? DarkColors.ListViewBackground : LightColors.ListViewBackground;
        listBox.ForeColor = isDarkMode ? DarkColors.ListViewForeground : LightColors.ListViewForeground;
        // Note: ListBox doesn't have SelectedBackColor/SelectedForeColor properties in .NET Framework
    }

    private static void ApplyTextBoxTheme(TextBox textBox, bool isDarkMode)
    {
        textBox.BackColor = isDarkMode ? DarkColors.ControlLight : LightColors.ControlLight;
        textBox.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyButtonTheme(Button button, bool isDarkMode)
    {
        button.BackColor = isDarkMode ? DarkColors.Control : LightColors.Control;
        button.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = isDarkMode ? DarkColors.ControlDark : LightColors.ControlDark;
        button.FlatAppearance.MouseOverBackColor = isDarkMode ? DarkColors.ControlLight : LightColors.ControlLight;
        button.FlatAppearance.MouseDownBackColor = isDarkMode ? DarkColors.Highlight : LightColors.Highlight;
        
        // Special handling for playback control buttons to ensure they're visible
        if (button.Text.Contains("▶") || button.Text.Contains("⏸") || button.Text.Contains("⏹") || 
            button.Text.Contains("⏮") || button.Text.Contains("⏭") || button.Text.Contains("🔀") || 
            button.Text.Contains("🔁") || button.Text.Contains("Play") || button.Text.Contains("Stop") ||
            button.Text.Contains("Previous") || button.Text.Contains("Next") || button.Text.Contains("Repeat") ||
            button.Text.Contains("Shuffle"))
        {
            // Ensure playback buttons have good contrast
            button.BackColor = isDarkMode ? DarkColors.ControlLight : LightColors.ControlLight;
            button.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
        }
    }

    private static void ApplyTrackBarTheme(TrackBar trackBar, bool isDarkMode)
    {
        trackBar.BackColor = isDarkMode ? DarkColors.Control : LightColors.Control;
        trackBar.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
    }

    private static void ApplyProgressBarTheme(ProgressBar progressBar, bool isDarkMode)
    {
        progressBar.BackColor = isDarkMode ? DarkColors.ControlDark : LightColors.ControlDark;
        progressBar.ForeColor = isDarkMode ? DarkColors.Highlight : LightColors.Highlight;
    }

    private static void ApplyStatusStripTheme(StatusStrip statusStrip, bool isDarkMode)
    {
        statusStrip.BackColor = isDarkMode ? DarkColors.StatusStripBackground : LightColors.StatusStripBackground;
        statusStrip.ForeColor = isDarkMode ? DarkColors.StatusStripForeground : LightColors.StatusStripForeground;
        statusStrip.Renderer = new DarkModeToolStripRenderer(isDarkMode);
    }

    private static void ApplyPanelTheme(Panel panel, bool isDarkMode)
    {
        panel.BackColor = isDarkMode ? DarkColors.Background : LightColors.Background;
        panel.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
    }

    private static void ApplyLabelTheme(Label label, bool isDarkMode)
    {
        label.BackColor = Color.Transparent;
        label.ForeColor = isDarkMode ? DarkColors.Foreground : LightColors.Foreground;
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