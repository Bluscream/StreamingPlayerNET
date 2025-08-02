using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class SettingsForm : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Configuration _originalConfig;
    private readonly Configuration _workingConfig;
    private readonly Dictionary<string, Control> _propertyControls = new();
    private readonly TabControl _tabControl;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;
    private readonly Button _resetButton;
    private readonly ToolTip _toolTip;

    public SettingsForm()
    {
        _originalConfig = ConfigurationService.Current;
        _workingConfig = new Configuration();
        
        // Copy current values to working config
        CopyConfiguration(_originalConfig, _workingConfig);
        
        InitializeComponent();
        
        // Set the form icon
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("StreamingPlayerNET.logo.ico");
            if (stream != null)
            {
                Icon = new Icon(stream);
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to load application icon");
        }
        
        _toolTip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true
        };
        
        _tabControl = new TabControl
        {
            Dock = DockStyle.Top,
            Height = ClientSize.Height - 50
        };
        
        _saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Width = 75,
            Height = 23,
            Location = new Point(ClientSize.Width - 170, ClientSize.Height - 35)
        };
        
        _cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 75,
            Height = 23,
            Location = new Point(ClientSize.Width - 85, ClientSize.Height - 35)
        };
        
        _resetButton = new Button
        {
            Text = "Reset to Defaults",
            Width = 120,
            Height = 23,
            Location = new Point(10, ClientSize.Height - 35)
        };
        
        _saveButton.Click += SaveButton_Click;
        _cancelButton.Click += CancelButton_Click;
        _resetButton.Click += ResetButton_Click;
        
        Controls.Add(_tabControl);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        Controls.Add(_resetButton);
        
        GenerateSettingsUI();
        
        // Handle form resize
        Resize += (s, e) =>
        {
            _tabControl.Height = ClientSize.Height - 50;
            _saveButton.Location = new Point(ClientSize.Width - 170, ClientSize.Height - 35);
            _cancelButton.Location = new Point(ClientSize.Width - 85, ClientSize.Height - 35);
            
            // Update control widths when form is resized
            foreach (var control in _propertyControls.Values)
            {
                if (control.Parent is Panel panel)
                {
                    control.Width = panel.ClientSize.Width - 20;
                }
            }
        };
    }

    private void InitializeComponent()
    {
        Text = "Settings";
        Size = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ShowInTaskbar = false;
    }

    private void GenerateSettingsUI()
    {
        var properties = typeof(Configuration).GetProperties()
            .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .OrderBy(p => p.GetCustomAttribute<CategoryAttribute>()?.Category ?? "General")
            .ThenBy(p => p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.Name);

        var categories = properties
            .Select(p => p.GetCustomAttribute<CategoryAttribute>()?.Category ?? "General")
            .Distinct()
            .OrderBy(c => c);

        // Create App tab
        var appTabPage = new TabPage("App");
        var appPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10)
        };

        // Source tabs will be created dynamically

        // Define which categories go to which tab
        var appCategories = new[] { "UI", "Audio", "Playback", "Hotkeys", "Logging", "Advanced", "Download", "Search" };
        var sourceCategories = new string[0]; // Source categories are now handled dynamically

        var appY = 10;

        foreach (var category in categories)
        {
            var categoryProperties = properties
                .Where(p => (p.GetCustomAttribute<CategoryAttribute>()?.Category ?? "General") == category)
                .ToList();

            if (categoryProperties.Count == 0) continue;

            // Determine which tab this category belongs to
            Panel targetPanel;
            ref int y = ref appY;
            
            if (appCategories.Contains(category))
            {
                targetPanel = appPanel;
            }
            else if (sourceCategories.Contains(category))
            {
                // Source categories are now handled in separate tabs
                targetPanel = appPanel;
            }
            else
            {
                // Default to App tab for any uncategorized items
                targetPanel = appPanel;
            }

            // Create GroupBox for this category
            var groupBox = new GroupBox
            {
                Text = category,
                Location = new Point(10, y),
                Width = targetPanel.ClientSize.Width - 20,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                AutoSize = false
            };

            var groupPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                Padding = new Padding(10)
            };

            var groupY = 10;

            foreach (var property in categoryProperties)
            {
                var displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;
                var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                var value = property.GetValue(_workingConfig);

                // Create input control
                var inputControl = CreateInputControl(property, _workingConfig, value, displayName, description);
                if (inputControl != null)
                {
                    inputControl.Location = new Point(10, groupY);
                    inputControl.Width = groupPanel.ClientSize.Width - 20;
                    inputControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    groupPanel.Controls.Add(inputControl);
                    _propertyControls[property.Name] = inputControl;
                    groupY += inputControl.Height + 15;
                }
            }

            // Set the height of the group box based on content
            groupBox.Height = Math.Min(groupY + 20, 400); // Cap at 400px height
            groupBox.Controls.Add(groupPanel);
            targetPanel.Controls.Add(groupBox);

            y += groupBox.Height + 15;
        }

        appTabPage.Controls.Add(appPanel);
        
        // Generate source settings tabs
        GenerateSourceSettingsTabs();
        
        // Add App tab first to ensure it's always first
        _tabControl.TabPages.Insert(0, appTabPage);
    }

    private void GenerateSourceSettingsTabs()
    {
        var sourceProviders = SourceManager.Instance.GetSourceProvidersWithSettings();
        
        if (!sourceProviders.Any())
        {
            // Create a single "Sources" tab with a message
            var sourcesTabPage = new TabPage("Sources");
            var sourcesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            
            var noSourcesLabel = new Label
            {
                Text = "No source providers with settings are currently available.",
                AutoSize = true,
                Location = new Point(10, 10),
                ForeColor = Color.Gray
            };
            sourcesPanel.Controls.Add(noSourcesLabel);
            sourcesTabPage.Controls.Add(sourcesPanel);
            _tabControl.TabPages.Add(sourcesTabPage);
            return;
        }
        
        foreach (var sourceProvider in sourceProviders)
        {
            if (sourceProvider.Settings == null) continue;
            
            var settings = sourceProvider.Settings;
            
            // Create a tab for this source
            var sourceTabPage = new TabPage(sourceProvider.Name);
            var sourcePanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            
            var y = 10;
            
            // Get properties from the settings object
            var properties = settings.GetSettingsProperties();
            
            foreach (var property in properties)
            {
                var displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;
                var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                var value = property.GetValue(settings);
                
                // Create input control for this property
                var inputControl = CreateInputControl(property, settings, value, displayName, description);
                if (inputControl != null)
                {
                    inputControl.Location = new Point(10, y);
                    inputControl.Width = sourcePanel.ClientSize.Width - 20;
                    inputControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    sourcePanel.Controls.Add(inputControl);
                    _propertyControls[$"{sourceProvider.Name}_{property.Name}"] = inputControl;
                    y += inputControl.Height + 15;
                }
            }
            
            sourceTabPage.Controls.Add(sourcePanel);
            _tabControl.TabPages.Add(sourceTabPage);
        }
    }

    private Control? CreateInputControl(PropertyInfo property, object settingsObject, object? value, string displayName, string description)
    {
        if (property.PropertyType == typeof(bool))
        {
            var checkBox = new CheckBox
            {
                Text = displayName,
                Checked = (bool)value,
                AutoSize = true,
                Height = 30
            };
            checkBox.CheckedChanged += (s, e) => property.SetValue(settingsObject, checkBox.Checked);
            
            // Add tooltip
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(checkBox, description);
            }
            
            return checkBox;
        }
        else if (property.PropertyType == typeof(int))
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var numericUpDown = new NumericUpDown
            {
                Location = new Point(0, 25),
                Width = 200,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            int intValue = (int)value;
            if (intValue < numericUpDown.Minimum) intValue = (int)numericUpDown.Minimum;
            if (intValue > numericUpDown.Maximum) intValue = (int)numericUpDown.Maximum;
            numericUpDown.Value = intValue;
            numericUpDown.ValueChanged += (s, e) => property.SetValue(settingsObject, (int)numericUpDown.Value);

            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(numericUpDown);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(numericUpDown, description);
            }
            
            return container;
        }
        else if (property.PropertyType == typeof(string))
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var textBox = new TextBox
            {
                Text = value?.ToString() ?? "",
                Location = new Point(0, 25),
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            textBox.TextChanged += (s, e) => property.SetValue(settingsObject, textBox.Text);
            
            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(textBox);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(textBox, description);
            }
            
            return container;
        }
        else if (property.PropertyType.IsEnum)
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var comboBox = new ComboBox
            {
                Location = new Point(0, 25),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            var enumValues = Enum.GetValues(property.PropertyType);
            foreach (var enumValue in enumValues)
            {
                var enumMember = property.PropertyType.GetField(enumValue.ToString()!);
                var descriptionAttr = enumMember?.GetCustomAttribute<DescriptionAttribute>();
                var displayText = descriptionAttr?.Description ?? enumValue.ToString();
                comboBox.Items.Add(new ComboBoxItem { Value = enumValue, Text = displayText });
            }

            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem item)
                {
                    property.SetValue(settingsObject, item.Value);
                }
            };

            // Set current value
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item && item.Value.Equals(value))
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }

            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(comboBox);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(comboBox, description);
            }
            
            return container;
        }

        return null;
    }

    private Control? CreateInputControl(PropertyInfo property, Configuration config, object? value, string displayName, string description)
    {
        if (property.PropertyType == typeof(bool))
        {
            var checkBox = new CheckBox
            {
                Text = displayName,
                Checked = (bool)value,
                AutoSize = true,
                Height = 30
            };
            checkBox.CheckedChanged += (s, e) => property.SetValue(config, checkBox.Checked);
            
            // Add tooltip
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(checkBox, description);
            }
            
            return checkBox;
        }
        else if (property.PropertyType == typeof(int))
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var numericUpDown = new NumericUpDown
            {
                Location = new Point(0, 25),
                Width = 200,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            int intValue = (int)value;
            if (intValue < numericUpDown.Minimum) intValue = (int)numericUpDown.Minimum;
            if (intValue > numericUpDown.Maximum) intValue = (int)numericUpDown.Maximum;
            numericUpDown.Value = intValue;
            numericUpDown.ValueChanged += (s, e) => property.SetValue(config, (int)numericUpDown.Value);

            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(numericUpDown);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(numericUpDown, description);
            }
            
            return container;
        }
        else if (property.PropertyType == typeof(KeyBind))
        {
            var keyBind = (KeyBind)value;
            
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };
            
            var enabledCheckBox = new CheckBox
            {
                Text = "Enabled",
                Checked = keyBind.Enabled,
                Location = new Point(0, 25),
                Width = 80,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            
            var textBox = new TextBox
            {
                Text = keyBind.ToString(),
                Location = new Point(90, 25),
                Width = 150,
                ReadOnly = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            
            var recButton = new Button
            {
                Text = "REC",
                Location = new Point(260, 25),
                Width = 50,
                Height = 23,
                BackColor = Color.LightGray,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            
            // Update textbox when enabled state changes
            enabledCheckBox.CheckedChanged += (s, e) =>
            {
                keyBind.Enabled = enabledCheckBox.Checked;
                textBox.Text = keyBind.ToString();
                property.SetValue(config, keyBind);
            };
            
            recButton.Click += (s, e) => StartHotkeyRecording(recButton, textBox, keyBind, property, config);
            
            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(enabledCheckBox);
            container.Controls.Add(textBox);
            container.Controls.Add(recButton);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(enabledCheckBox, "Enable or disable this hotkey");
                _toolTip.SetToolTip(textBox, description);
                _toolTip.SetToolTip(recButton, "Click to record a new key combination");
            }
            
            return container;
        }
        else if (property.PropertyType == typeof(string))
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            // Check if this is a directory path property
            if (property.Name.Contains("Directory") || property.Name.Contains("Folder"))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? "",
                    Location = new Point(0, 25),
                    Width = 400,
                    ReadOnly = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                
                var browseButton = new Button
                {
                    Text = "...",
                    Location = new Point(410, 25),
                    Width = 30,
                    Height = 23,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                
                browseButton.Click += (s, e) =>
                {
                    using var folderDialog = new FolderBrowserDialog
                    {
                        Description = $"Select {displayName}",
                        SelectedPath = textBox.Text
                    };
                    
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox.Text = folderDialog.SelectedPath;
                        property.SetValue(config, folderDialog.SelectedPath);
                    }
                };
                
                textBox.TextChanged += (s, e) => property.SetValue(config, textBox.Text);
                
                var container = new Panel
                {
                    Height = 55,
                    Width = 400,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                container.Controls.Add(label);
                container.Controls.Add(textBox);
                container.Controls.Add(browseButton);
                
                // Add tooltips
                if (!string.IsNullOrEmpty(description))
                {
                    _toolTip.SetToolTip(label, description);
                    _toolTip.SetToolTip(textBox, description);
                    _toolTip.SetToolTip(browseButton, description);
                }
                
                return container;
            }
            else if (property.Name.Contains("File"))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? "",
                    Location = new Point(0, 25),
                    Width = 400,
                    ReadOnly = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                var browseButton = new Button
                {
                    Text = "...",
                    Location = new Point(410, 25),
                    Width = 30,
                    Height = 23,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };

                browseButton.Click += (s, e) =>
                {
                    using var fileDialog = new OpenFileDialog
                    {
                        Title = $"Select {displayName}",
                        FileName = textBox.Text
                    };

                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox.Text = fileDialog.FileName;
                        property.SetValue(config, fileDialog.FileName);
                    }
                };

                textBox.TextChanged += (s, e) => property.SetValue(config, textBox.Text);

                var container = new Panel
                {
                    Height = 55,
                    Width = 400,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                container.Controls.Add(label);
                container.Controls.Add(textBox);
                container.Controls.Add(browseButton);
                
                // Add tooltips
                if (!string.IsNullOrEmpty(description))
                {
                    _toolTip.SetToolTip(label, description);
                    _toolTip.SetToolTip(textBox, description);
                    _toolTip.SetToolTip(browseButton, description);
                }
                
                return container;
            }

            else
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? "",
                    Location = new Point(0, 25),
                    Width = 400,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                textBox.TextChanged += (s, e) => property.SetValue(config, textBox.Text);
                
                var container = new Panel
                {
                    Height = 55,
                    Width = 400,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                container.Controls.Add(label);
                container.Controls.Add(textBox);
                
                // Add tooltips
                if (!string.IsNullOrEmpty(description))
                {
                    _toolTip.SetToolTip(label, description);
                    _toolTip.SetToolTip(textBox, description);
                }
                
                return container;
            }
        }
        else if (property.PropertyType.IsEnum)
        {
            var label = new Label
            {
                Text = displayName,
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var comboBox = new ComboBox
            {
                Location = new Point(0, 25),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            var enumValues = Enum.GetValues(property.PropertyType);
            foreach (var enumValue in enumValues)
            {
                var enumMember = property.PropertyType.GetField(enumValue.ToString()!);
                var descriptionAttr = enumMember?.GetCustomAttribute<DescriptionAttribute>();
                var displayText = descriptionAttr?.Description ?? enumValue.ToString();
                comboBox.Items.Add(new ComboBoxItem { Value = enumValue, Text = displayText });
            }

            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem item)
                {
                    property.SetValue(config, item.Value);
                }
            };

            // Set current value
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item && item.Value.Equals(value))
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }

            var container = new Panel
            {
                Height = 55,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            container.Controls.Add(label);
            container.Controls.Add(comboBox);
            
            // Add tooltips
            if (!string.IsNullOrEmpty(description))
            {
                _toolTip.SetToolTip(label, description);
                _toolTip.SetToolTip(comboBox, description);
            }
            
            return container;
        }

        return null;
    }

    private void StartHotkeyRecording(Button recButton, TextBox textBox, KeyBind keyBind, PropertyInfo property, Configuration config)
    {
        // Change button appearance to indicate recording
        recButton.Text = "Press...";
        recButton.BackColor = Color.Red;
        recButton.ForeColor = Color.White;
        
        // Create a temporary form to capture global key events
        var recordingForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            Opacity = 0,
            Size = new Size(1, 1),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-1000, -1000)
        };
        
        recordingForm.KeyDown += (s, e) =>
        {
            var hotkeyString = BuildHotkeyString(e);
            keyBind.Combo = hotkeyString;
            textBox.Text = keyBind.ToString();
            property.SetValue(config, keyBind);
            
            // Reset button appearance
            recButton.Text = "REC";
            recButton.BackColor = Color.LightGray;
            recButton.ForeColor = Color.Black;
            
            recordingForm.Close();
            e.Handled = true;
        };
        
        recordingForm.KeyUp += (s, e) =>
        {
            // Reset button appearance if recording is cancelled
            recButton.Text = "REC";
            recButton.BackColor = Color.LightGray;
            recButton.ForeColor = Color.Black;
            recordingForm.Close();
        };
        
        recordingForm.Show();
        recordingForm.Focus();
        
        // Auto-close after 5 seconds if no key is pressed
        var timer = new System.Windows.Forms.Timer
        {
            Interval = 5000
        };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            timer.Dispose();
            if (!recordingForm.IsDisposed)
            {
                recButton.Text = "REC";
                recButton.BackColor = Color.LightGray;
                recButton.ForeColor = Color.Black;
                recordingForm.Close();
            }
        };
        timer.Start();
    }
    
    private string BuildHotkeyString(KeyEventArgs e)
    {
        var modifiers = new List<string>();
        
        if (e.Control) modifiers.Add("Ctrl");
        if (e.Alt) modifiers.Add("Alt");
        if (e.Shift) modifiers.Add("Shift");
        
        modifiers.Add(e.KeyCode.ToString());
        
        return string.Join("+", modifiers);
    }

    private void CopyConfiguration(Configuration source, Configuration target)
    {
        var properties = typeof(Configuration).GetProperties()
            .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<JsonIgnoreAttribute>() == null);

        foreach (var property in properties)
        {
            var value = property.GetValue(source);
            property.SetValue(target, value);
        }
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Copy working config to current config
            CopyConfiguration(_workingConfig, ConfigurationService.Current);
            ConfigurationService.SaveConfiguration();
            
            // Save source settings
            var sourceProviders = SourceManager.Instance.GetSourceProvidersWithSettings();
            foreach (var provider in sourceProviders)
            {
                if (provider.Settings != null)
                {
                    await provider.Settings.SaveAsync();
                }
            }
            
            Logger.Info("Settings saved successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save settings");
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        // No action needed - changes are discarded
    }

    private void ResetButton_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values?",
            "Reset Settings",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                ConfigurationService.ResetToDefaults();
                ConfigurationService.ReloadConfiguration();
                
                // Refresh the UI with new values
                RefreshSettingsUI();
                
                MessageBox.Show("Settings have been reset to defaults.", "Settings Reset", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to reset settings");
                MessageBox.Show($"Failed to reset settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void RefreshSettingsUI()
    {
        var currentConfig = ConfigurationService.Current;
        
        foreach (var kvp in _propertyControls)
        {
            var propertyName = kvp.Key;
            var control = kvp.Value;
            var property = typeof(Configuration).GetProperty(propertyName);
            
            if (property != null)
            {
                var value = property.GetValue(currentConfig);
                UpdateControlValue(control, value);
            }
        }
    }

    private void UpdateControlValue(Control control, object? value)
    {
        if (control is CheckBox checkBox && value is bool boolValue)
        {
            checkBox.Checked = boolValue;
        }
        else if (control is NumericUpDown numericUpDown && value is int intValue)
        {
            numericUpDown.Value = intValue;
        }
        else if (control is TextBox textBox && value is string stringValue)
        {
            textBox.Text = stringValue;
        }
        else if (control is ComboBox comboBox)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item && item.Value.Equals(value))
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }
        else if (control is Panel panel && value is KeyBind keyBind)
        {
            // Find the controls in the panel for KeyBind
            var enabledCheckBox = panel.Controls.OfType<CheckBox>().FirstOrDefault();
            var keyBindTextBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
            
            if (enabledCheckBox != null)
            {
                enabledCheckBox.Checked = keyBind.Enabled;
            }
            
            if (keyBindTextBox != null)
            {
                keyBindTextBox.Text = keyBind.ToString();
            }
        }
    }

    private class ComboBoxItem
    {
        public object Value { get; set; } = null!;
        public string Text { get; set; } = "";
        
        public override string ToString()
        {
            return Text;
        }
    }
} 