namespace StreamingPlayerNET.UI;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem reloadPlaylistsMenuItem;
    private ToolStripMenuItem exitMenuItem;
    private ToolStripMenuItem playbackMenu;
    private ToolStripMenuItem playMenuItem;
    private ToolStripMenuItem pauseMenuItem;
    private ToolStripMenuItem stopMenuItem;
    private ToolStripSeparator playbackSeparator1;
    private ToolStripMenuItem nextMenuItem;
    private ToolStripMenuItem previousMenuItem;
    private ToolStripMenuItem volumeUpMenuItem;
    private ToolStripMenuItem volumeDownMenuItem;
    private ToolStripSeparator playbackSeparator2;
    private ToolStripMenuItem repeatMenuItem;
    private ToolStripMenuItem shuffleMenuItem;
    private ToolStripMenuItem viewMenu;
    private ToolStripMenuItem showPlaylistsMenuItem;
    private ToolStripMenuItem showSearchMenuItem;
    private ToolStripMenuItem toggleDarkModeMenuItem;
    private ToolStripMenuItem helpMenu;
    private ToolStripMenuItem aboutMenuItem;
    private ToolStripMenuItem helpMenuItem;
    private ToolStripMenuItem settingsMenu;
    private TabControl mainTabControl;
    private TabPage searchTabPage;
    private TabPage queueTabPage;
    private TabPage playlistTabPage;

    private TextBox searchTextBox;
    private Button searchButton;
    private ListView searchListView;
    private ColumnHeader searchTitleColumn;
    private ColumnHeader searchArtistColumn;
    private ColumnHeader searchDurationColumn;
    private ColumnHeader searchSourceColumn;
    private ListView queueListView;
    private ColumnHeader queueTitleColumn;
    private ColumnHeader queueArtistColumn;
    private ColumnHeader queueDurationColumn;
    private ColumnHeader queueSourceColumn;
    private ListView playlistListView;
    private ColumnHeader playlistTitleColumn;
    private ColumnHeader playlistArtistColumn;
    private ColumnHeader playlistDurationColumn;
    private ColumnHeader playlistSourceColumn;

    private Panel searchControlsPanel;


    private ListBox playlistsListBox;
    private Panel playerPanel;
    private Panel playbackControlsPanel;
    private Button playPauseButton;
    private Button stopButton;
    private Button previousButton;
    private Button nextButton;
    private TrackBar volumeTrackBar;
    private Button repeatButton;
    private Button shuffleButton;
    private Label currentSongLabel;
    private ProgressBar seekBar;
    private Label elapsedTimeLabel;
    private Label remainingTimeLabel;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripStatusLabel timingLabel;
    private ToolStripProgressBar downloadProgressBar;
    private Label volumeLabel;


    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        reloadPlaylistsMenuItem = new ToolStripMenuItem();
        exitMenuItem = new ToolStripMenuItem();
        playbackMenu = new ToolStripMenuItem();
        playMenuItem = new ToolStripMenuItem();
        pauseMenuItem = new ToolStripMenuItem();
        stopMenuItem = new ToolStripMenuItem();
        playbackSeparator1 = new ToolStripSeparator();
        nextMenuItem = new ToolStripMenuItem();
        previousMenuItem = new ToolStripMenuItem();
        volumeUpMenuItem = new ToolStripMenuItem();
        volumeDownMenuItem = new ToolStripMenuItem();
        playbackSeparator2 = new ToolStripSeparator();
        repeatMenuItem = new ToolStripMenuItem();
        shuffleMenuItem = new ToolStripMenuItem();
        viewMenu = new ToolStripMenuItem();
        showPlaylistsMenuItem = new ToolStripMenuItem();
        showSearchMenuItem = new ToolStripMenuItem();
        toggleDarkModeMenuItem = new ToolStripMenuItem();
        helpMenu = new ToolStripMenuItem();
        aboutMenuItem = new ToolStripMenuItem();
        helpMenuItem = new ToolStripMenuItem();
        settingsMenu = new ToolStripMenuItem();
        mainTabControl = new TabControl();
        searchTabPage = new TabPage();
        queueTabPage = new TabPage();
        playlistTabPage = new TabPage();

        searchControlsPanel = new Panel();
        searchTextBox = new TextBox();
        searchButton = new Button();
        searchListView = new ListView();
        searchTitleColumn = new ColumnHeader();
        searchArtistColumn = new ColumnHeader();
        searchDurationColumn = new ColumnHeader();
        searchSourceColumn = new ColumnHeader();


        queueListView = new ListView();
        queueTitleColumn = new ColumnHeader();
        queueArtistColumn = new ColumnHeader();
        queueDurationColumn = new ColumnHeader();
        queueSourceColumn = new ColumnHeader();
        playlistsListBox = new ListBox();
        playlistListView = new ListView();
        playlistTitleColumn = new ColumnHeader();
        playlistArtistColumn = new ColumnHeader();
        playlistDurationColumn = new ColumnHeader();
        playlistSourceColumn = new ColumnHeader();
        playerPanel = new Panel();
        playbackControlsPanel = new Panel();
        playPauseButton = new Button();
        stopButton = new Button();
        previousButton = new Button();
        nextButton = new Button();
        repeatButton = new Button();
        shuffleButton = new Button();
        volumeTrackBar = new TrackBar();
        currentSongLabel = new Label();
        seekBar = new ProgressBar();
        elapsedTimeLabel = new Label();
        remainingTimeLabel = new Label();

        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        timingLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        volumeLabel = new Label();
        menuStrip.SuspendLayout();
        mainTabControl.SuspendLayout();
        searchTabPage.SuspendLayout();
        queueTabPage.SuspendLayout();
        playlistTabPage.SuspendLayout();

        searchControlsPanel.SuspendLayout();

        playerPanel.SuspendLayout();
        playbackControlsPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)volumeTrackBar).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, playbackMenu, viewMenu, toggleDarkModeMenuItem, settingsMenu, helpMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1000, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip";
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { reloadPlaylistsMenuItem, exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "&File";
        // 
        // reloadPlaylistsMenuItem
        // 
        reloadPlaylistsMenuItem.Name = "reloadPlaylistsMenuItem";
        reloadPlaylistsMenuItem.ShortcutKeys = Keys.F5;
        reloadPlaylistsMenuItem.Size = new Size(180, 22);
        reloadPlaylistsMenuItem.Text = "&Reload Playlists";
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
        exitMenuItem.Size = new Size(180, 22);
        exitMenuItem.Text = "E&xit";
        // 
        // playbackMenu
        // 
        playbackMenu.DropDownItems.AddRange(new ToolStripItem[] { playMenuItem, pauseMenuItem, stopMenuItem, playbackSeparator1, nextMenuItem, previousMenuItem, volumeUpMenuItem, volumeDownMenuItem, playbackSeparator2, repeatMenuItem, shuffleMenuItem });
        playbackMenu.Name = "playbackMenu";
        playbackMenu.Size = new Size(66, 20);
        playbackMenu.Text = "&Playback";
        // 
        // playMenuItem
        // 
        playMenuItem.Name = "playMenuItem";
        playMenuItem.Size = new Size(180, 22);
        playMenuItem.Text = "&Play";
        // 
        // pauseMenuItem
        // 
        pauseMenuItem.Name = "pauseMenuItem";
        pauseMenuItem.Size = new Size(180, 22);
        pauseMenuItem.Text = "&Pause";
        // 
        // stopMenuItem
        // 
        stopMenuItem.Name = "stopMenuItem";
        stopMenuItem.Size = new Size(180, 22);
        stopMenuItem.Text = "&Stop";
        // 
        // playbackSeparator1
        // 
        playbackSeparator1.Name = "playbackSeparator1";
        playbackSeparator1.Size = new Size(177, 6);
        // 
        // nextMenuItem
        // 
        nextMenuItem.Name = "nextMenuItem";
        nextMenuItem.ShortcutKeys = Keys.Control | Keys.Right;
        nextMenuItem.Size = new Size(180, 22);
        nextMenuItem.Text = "&Next Track";
        // 
        // previousMenuItem
        // 
        previousMenuItem.Name = "previousMenuItem";
        previousMenuItem.ShortcutKeys = Keys.Control | Keys.Left;
        previousMenuItem.Size = new Size(180, 22);
        previousMenuItem.Text = "&Previous Track";
        // 
        // volumeUpMenuItem
        // 
        volumeUpMenuItem.Name = "volumeUpMenuItem";
        volumeUpMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
        volumeUpMenuItem.Size = new Size(180, 22);
        volumeUpMenuItem.Text = "Volume &Up";
        // 
        // volumeDownMenuItem
        // 
        volumeDownMenuItem.Name = "volumeDownMenuItem";
        volumeDownMenuItem.ShortcutKeys = Keys.Control | Keys.Down;
        volumeDownMenuItem.Size = new Size(180, 22);
        volumeDownMenuItem.Text = "Volume &Down";
        // 
        // playbackSeparator2
        // 
        playbackSeparator2.Name = "playbackSeparator2";
        playbackSeparator2.Size = new Size(177, 6);
        // 
        // repeatMenuItem
        // 
        repeatMenuItem.Name = "repeatMenuItem";
        repeatMenuItem.ShortcutKeys = Keys.Control | Keys.R;
        repeatMenuItem.Size = new Size(180, 22);
        repeatMenuItem.Text = "&Repeat Mode";
        // 
        // shuffleMenuItem
        // 
        shuffleMenuItem.Name = "shuffleMenuItem";
        shuffleMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        shuffleMenuItem.Size = new Size(180, 22);
        shuffleMenuItem.Text = "&Shuffle";
        // 
        // viewMenu
        // 
        viewMenu.DropDownItems.AddRange(new ToolStripItem[] { showPlaylistsMenuItem, showSearchMenuItem });
        viewMenu.Name = "viewMenu";
        viewMenu.Size = new Size(44, 20);
        viewMenu.Text = "&View";
        // 
        // showPlaylistsMenuItem
        // 
        showPlaylistsMenuItem.Checked = true;
        showPlaylistsMenuItem.CheckOnClick = true;
        showPlaylistsMenuItem.Name = "showPlaylistsMenuItem";
        showPlaylistsMenuItem.Size = new Size(180, 22);
        showPlaylistsMenuItem.Text = "Show &Playlists";
        // 
        // showSearchMenuItem
        // 
        showSearchMenuItem.Checked = true;
        showSearchMenuItem.CheckOnClick = true;
        showSearchMenuItem.Name = "showSearchMenuItem";
        showSearchMenuItem.Size = new Size(180, 22);
        showSearchMenuItem.Text = "Show &Search";
        // 
        // toggleDarkModeMenuItem
        // 
        toggleDarkModeMenuItem.Name = "toggleDarkModeMenuItem";
        toggleDarkModeMenuItem.Size = new Size(61, 20);
        toggleDarkModeMenuItem.Text = "&Dark Mode";
        // 
        // settingsMenu
        // 
        settingsMenu.Name = "settingsMenu";
        settingsMenu.Size = new Size(61, 20);
        settingsMenu.Text = "&Settings";
        // 
        // helpMenu
        // 
        helpMenu.DropDownItems.AddRange(new ToolStripItem[] { helpMenuItem, aboutMenuItem });
        helpMenu.Name = "helpMenu";
        helpMenu.Size = new Size(44, 20);
        helpMenu.Text = "&Help";
        // 
        // helpMenuItem
        // 
        helpMenuItem.Name = "helpMenuItem";
        helpMenuItem.Size = new Size(180, 22);
        helpMenuItem.Text = "&Help";
        // 
        // aboutMenuItem
        // 
        aboutMenuItem.Name = "aboutMenuItem";
        aboutMenuItem.Size = new Size(180, 22);
        aboutMenuItem.Text = "&About";
        // 
        // mainTabControl
        // 
        mainTabControl.Controls.Add(searchTabPage);
        mainTabControl.Controls.Add(queueTabPage);
        mainTabControl.Controls.Add(playlistTabPage);
        mainTabControl.Dock = DockStyle.Fill;
        mainTabControl.Location = new Point(0, 24);
        mainTabControl.Name = "mainTabControl";
        mainTabControl.SelectedIndex = 0;
        mainTabControl.Size = new Size(1000, 450);
        mainTabControl.TabIndex = 1;
        // 
        // searchTabPage
        // 
        searchTabPage.Controls.Add(searchListView);
        searchTabPage.Controls.Add(searchControlsPanel);
        searchTabPage.Location = new Point(4, 24);
        searchTabPage.Name = "searchTabPage";
        searchTabPage.Padding = new Padding(3);
        searchTabPage.Size = new Size(992, 422);
        searchTabPage.TabIndex = 0;
        searchTabPage.Text = "🔍 Search";
        searchTabPage.UseVisualStyleBackColor = true;
        // 
        // queueTabPage
        // 
        queueTabPage.Controls.Add(queueListView);
        // queueTabPage.Location = new Point(4, 24);
        queueTabPage.Name = "queueTabPage";
        queueTabPage.Padding = new Padding(3);
        // queueTabPage.Size = new Size(992, 422);
        queueTabPage.TabIndex = 1;
        queueTabPage.Text = "📋 Queue";
        queueTabPage.UseVisualStyleBackColor = true;
        queueTabPage.Dock = DockStyle.Fill;
        // 
        // playlistTabPage
        // 
        playlistTabPage.Controls.Add(playlistListView);
        playlistTabPage.Controls.Add(playlistsListBox);
        playlistTabPage.Location = new Point(4, 24);
        playlistTabPage.Name = "playlistTabPage";
        playlistTabPage.Padding = new Padding(3);
        playlistTabPage.Size = new Size(992, 422);
        playlistTabPage.TabIndex = 2;
        playlistTabPage.Text = "🎵 Playlists";
        playlistTabPage.UseVisualStyleBackColor = true;
        // 

        // 
        // searchControlsPanel
        // 
        searchControlsPanel.Controls.Add(searchTextBox);
        searchControlsPanel.Controls.Add(searchButton);
        searchControlsPanel.Dock = DockStyle.Top;
        searchControlsPanel.Location = new Point(3, 3);
        searchControlsPanel.Name = "searchControlsPanel";
        searchControlsPanel.Size = new Size(986, 50);
        searchControlsPanel.TabIndex = 0;
        // 
        // searchTextBox
        // 
        searchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        searchTextBox.Location = new Point(10, 15);
        searchTextBox.Name = "searchTextBox";
        searchTextBox.PlaceholderText = "Search for songs, artists, albums...";
        searchTextBox.Size = new Size(800, 23);
        searchTextBox.TabIndex = 0;
        // 
        // searchButton
        // 
        searchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        searchButton.Location = new Point(820, 14);
        searchButton.Name = "searchButton";
        searchButton.Size = new Size(75, 25);
        searchButton.TabIndex = 1;
        searchButton.Text = "Search";
        searchButton.UseVisualStyleBackColor = true;
        // 
        // searchListView
        // 
        searchListView.Columns.AddRange(new ColumnHeader[] { searchTitleColumn, searchArtistColumn, searchDurationColumn, searchSourceColumn });
        searchListView.Dock = DockStyle.Fill;
        searchListView.FullRowSelect = true;
        searchListView.GridLines = true;
        searchListView.MultiSelect = true;
        searchListView.Name = "searchListView";
        searchListView.Size = new Size(986, 359);
        searchListView.TabIndex = 1;
        searchListView.UseCompatibleStateImageBehavior = false;
        searchListView.View = View.Details;
        // 
        // searchTitleColumn
        // 
        searchTitleColumn.Text = "Title";
        searchTitleColumn.Width = 300;
        // 
        // searchArtistColumn
        // 
        searchArtistColumn.Text = "Artist";
        searchArtistColumn.Width = 200;
        // 
        // searchDurationColumn
        // 
        searchDurationColumn.Text = "Duration";
        searchDurationColumn.TextAlign = HorizontalAlignment.Right;
        searchDurationColumn.Width = 80;
        // 
        // searchSourceColumn
        // 
        searchSourceColumn.Text = "Source";
        searchSourceColumn.Width = 100;

        // 
        // queueListView
        // 
        queueListView.Columns.AddRange(new ColumnHeader[] { queueTitleColumn, queueArtistColumn, queueDurationColumn, queueSourceColumn });
        queueListView.Dock = DockStyle.Fill;
        queueListView.FullRowSelect = true;
        queueListView.GridLines = true;
        queueListView.MultiSelect = true;
        queueListView.Name = "queueListView";
        // queueListView.Size = new Size(986, 359);
        queueListView.TabIndex = 1;
        queueListView.UseCompatibleStateImageBehavior = false;
        queueListView.View = View.Details;
        // 
        // queueTitleColumn
        // 
        queueTitleColumn.Text = "Title";
        queueTitleColumn.Width = 300;
        // 
        // queueArtistColumn
        // 
        queueArtistColumn.Text = "Artist";
        queueArtistColumn.Width = 200;
        // 
        // queueDurationColumn
        // 
        queueDurationColumn.Text = "Duration";
        queueDurationColumn.TextAlign = HorizontalAlignment.Right;
        queueDurationColumn.Width = 80;
        // 
        // queueSourceColumn
        // 
        queueSourceColumn.Text = "Source";
        queueSourceColumn.Width = 100;
        // 

        // 

        // 
        // playlistsListBox
        // 
        playlistsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        playlistsListBox.FormattingEnabled = true;
        playlistsListBox.ItemHeight = 15;
        playlistsListBox.Location = new Point(10, 10);
        playlistsListBox.Name = "playlistsListBox";
        playlistsListBox.Size = new Size(250, 400);
        playlistsListBox.TabIndex = 1;
        // 
        // playlistListView
        // 
        playlistListView.Columns.AddRange(new ColumnHeader[] { playlistTitleColumn, playlistArtistColumn, playlistDurationColumn, playlistSourceColumn });
        playlistListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        playlistListView.FullRowSelect = true;
        playlistListView.GridLines = true;
        playlistListView.MultiSelect = true;
        playlistListView.Name = "playlistListView";
        playlistListView.Location = new Point(270, 10);
        playlistListView.Size = new Size(720, 400);
        playlistListView.TabIndex = 1;
        playlistListView.UseCompatibleStateImageBehavior = false;
        playlistListView.View = View.Details;
        // 
        // playlistTitleColumn
        // 
        playlistTitleColumn.Text = "Title";
        playlistTitleColumn.Width = 300;
        // 
        // playlistArtistColumn
        // 
        playlistArtistColumn.Text = "Artist";
        playlistArtistColumn.Width = 200;
        // 
        // playlistDurationColumn
        // 
        playlistDurationColumn.Text = "Duration";
        playlistDurationColumn.TextAlign = HorizontalAlignment.Right;
        playlistDurationColumn.Width = 80;
        // 
        // playlistSourceColumn
        // 
        playlistSourceColumn.Text = "Source";
        playlistSourceColumn.Width = 100;
        // 
        // playerPanel
        // 
        playerPanel.Controls.Add(playbackControlsPanel);
        playerPanel.Controls.Add(currentSongLabel);
        playerPanel.Controls.Add(seekBar);
        playerPanel.Controls.Add(elapsedTimeLabel);
        playerPanel.Controls.Add(remainingTimeLabel);
        playerPanel.Dock = DockStyle.Bottom;
        playerPanel.Location = new Point(0, 450);
        playerPanel.Name = "playerPanel";
        playerPanel.Size = new Size(1000, 80);
        playerPanel.TabIndex = 2;
        // 
        // playbackControlsPanel
        // 
        playbackControlsPanel.Controls.Add(previousButton);
        playbackControlsPanel.Controls.Add(nextButton);
        playbackControlsPanel.Controls.Add(repeatButton);
        playbackControlsPanel.Controls.Add(shuffleButton);
        playbackControlsPanel.Controls.Add(volumeTrackBar);
        playbackControlsPanel.Controls.Add(volumeLabel);
        playbackControlsPanel.Controls.Add(stopButton);
        playbackControlsPanel.Controls.Add(playPauseButton);
        playbackControlsPanel.Dock = DockStyle.Bottom;
        playbackControlsPanel.Location = new Point(0, 80);
        playbackControlsPanel.Name = "playbackControlsPanel";
        playbackControlsPanel.Size = new Size(1000, 40);
        playbackControlsPanel.TabIndex = 6;
        // 
        // playPauseButton
        // 
        playPauseButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        playPauseButton.Location = new Point(10, 5);
        playPauseButton.Name = "playPauseButton";
        playPauseButton.Size = new Size(50, 30);
        playPauseButton.TabIndex = 0;
        playPauseButton.Text = "▶";
        playPauseButton.UseVisualStyleBackColor = true;
        // 
        // stopButton
        // 
        stopButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        stopButton.Location = new Point(66, 5);
        stopButton.Name = "stopButton";
        stopButton.Size = new Size(50, 30);
        stopButton.TabIndex = 2;
        stopButton.Text = "⏹";
        stopButton.UseVisualStyleBackColor = true;
        // 
        // previousButton
        // 
        previousButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        previousButton.Location = new Point(122, 5);
        previousButton.Name = "previousButton";
        previousButton.Size = new Size(50, 30);
        previousButton.TabIndex = 3;
        previousButton.Text = "⏮";
        previousButton.UseVisualStyleBackColor = true;
        // 
        // nextButton
        // 
        nextButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        nextButton.Location = new Point(178, 5);
        nextButton.Name = "nextButton";
        nextButton.Size = new Size(50, 30);
        nextButton.TabIndex = 4;
        nextButton.Text = "⏭";
        nextButton.UseVisualStyleBackColor = true;
        // 
        // repeatButton
        // 
        repeatButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        repeatButton.Location = new Point(234, 5);
        repeatButton.Name = "repeatButton";
        repeatButton.Size = new Size(50, 30);
        repeatButton.TabIndex = 5;
        repeatButton.Text = "🔁";
        repeatButton.UseVisualStyleBackColor = true;
        // 
        // shuffleButton
        // 
        shuffleButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        shuffleButton.Location = new Point(290, 5);
        shuffleButton.Name = "shuffleButton";
        shuffleButton.Size = new Size(50, 30);
        shuffleButton.TabIndex = 6;
        shuffleButton.Text = "🔀";
        shuffleButton.UseVisualStyleBackColor = true;
        // 
        // volumeTrackBar
        // 
        volumeTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        volumeTrackBar.Location = new Point(870, 5);
        volumeTrackBar.Maximum = 100;
        volumeTrackBar.Name = "volumeTrackBar";
        volumeTrackBar.Size = new Size(120, 30);
        volumeTrackBar.TabIndex = 7;
        volumeTrackBar.Value = 50;
        // 
        // currentSongLabel
        // 
        currentSongLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        currentSongLabel.Location = new Point(10, 10);
        currentSongLabel.Name = "currentSongLabel";
        currentSongLabel.Size = new Size(980, 20);
        currentSongLabel.TabIndex = 0;
        currentSongLabel.Text = "No song selected";
        currentSongLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // seekBar
        // 
        seekBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        seekBar.Location = new Point(60, 35);
        seekBar.Name = "seekBar";
        seekBar.Size = new Size(880, 23);
        seekBar.TabIndex = 1;
        // 
        // elapsedTimeLabel
        // 
        elapsedTimeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        elapsedTimeLabel.AutoSize = true;
        elapsedTimeLabel.Location = new Point(10, 38);
        elapsedTimeLabel.Name = "elapsedTimeLabel";
        elapsedTimeLabel.Size = new Size(44, 15);
        elapsedTimeLabel.TabIndex = 2;
        elapsedTimeLabel.Text = "00:00";
        elapsedTimeLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // remainingTimeLabel
        // 
        remainingTimeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        remainingTimeLabel.AutoSize = true;
        remainingTimeLabel.Cursor = Cursors.Hand;
        remainingTimeLabel.Location = new Point(946, 38);
        remainingTimeLabel.Name = "remainingTimeLabel";
        remainingTimeLabel.Size = new Size(44, 15);
        remainingTimeLabel.TabIndex = 3;
        remainingTimeLabel.Text = "00:00";
        remainingTimeLabel.TextAlign = ContentAlignment.MiddleRight;
        // 

        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { downloadProgressBar, statusLabel, timingLabel });
        statusStrip.Dock = DockStyle.Bottom;
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1000, 22);
        statusStrip.TabIndex = 3;
        statusStrip.Text = "statusStrip1";
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
        statusLabel.Spring = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // timingLabel
        // 
        timingLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        timingLabel.BorderStyle = Border3DStyle.Etched;
        timingLabel.Name = "timingLabel";
        timingLabel.Size = new Size(150, 17);
        timingLabel.Text = "";
        timingLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // downloadProgressBar
        // 
        downloadProgressBar.Name = "downloadProgressBar";
        downloadProgressBar.Size = new Size(100, 16);
        downloadProgressBar.Style = ProgressBarStyle.Continuous;
        downloadProgressBar.Visible = true;
        // 
        // volumeLabel
        // 
        volumeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        volumeLabel.AutoSize = true;
        volumeLabel.Location = new Point(870, 10);
        volumeLabel.Name = "volumeLabel";
        volumeLabel.Size = new Size(60, 15);
        volumeLabel.TabIndex = 6;
        volumeLabel.Text = "";
        volumeLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 592);
        Controls.Add(mainTabControl);
        Controls.Add(playerPanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 630);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Simple Music Player";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        mainTabControl.ResumeLayout(false);
        searchTabPage.ResumeLayout(false);
        queueTabPage.ResumeLayout(false);
        playlistTabPage.ResumeLayout(false);

        searchControlsPanel.ResumeLayout(false);
        searchControlsPanel.PerformLayout();

        playerPanel.ResumeLayout(false);
        playerPanel.PerformLayout();
        playbackControlsPanel.ResumeLayout(false);
        playbackControlsPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)volumeTrackBar).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
