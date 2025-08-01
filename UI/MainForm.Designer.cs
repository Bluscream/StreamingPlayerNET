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


    private SplitContainer playlistSplitContainer;
    private ListBox playlistsListBox;
    private Panel playerPanel;
    private SplitContainer playerSplitContainer;
    private Panel playbackControlsPanel;
    private Panel seekBarPanel;
    private Button playPauseButton;
    private Button stopButton;
    private Button previousButton;
    private Button nextButton;
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
    private ToolStripStatusLabel volumeLabel;
    private TrackBar volumeTrackBar;


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
        settingsMenu = new ToolStripMenuItem();
        helpMenu = new ToolStripMenuItem();
        helpMenuItem = new ToolStripMenuItem();
        aboutMenuItem = new ToolStripMenuItem();
        mainTabControl = new TabControl();
        searchTabPage = new TabPage();
        searchListView = new ListView();
        searchTitleColumn = new ColumnHeader();
        searchArtistColumn = new ColumnHeader();
        searchDurationColumn = new ColumnHeader();
        searchSourceColumn = new ColumnHeader();
        searchControlsPanel = new Panel();
        searchTextBox = new TextBox();
        searchButton = new Button();
        queueTabPage = new TabPage();
        queueListView = new ListView();
        queueTitleColumn = new ColumnHeader();
        queueArtistColumn = new ColumnHeader();
        queueDurationColumn = new ColumnHeader();
        queueSourceColumn = new ColumnHeader();
        playlistTabPage = new TabPage();
        playlistSplitContainer = new SplitContainer();
        playlistsListBox = new ListBox();
        playlistListView = new ListView();
        playlistTitleColumn = new ColumnHeader();
        playlistArtistColumn = new ColumnHeader();
        playlistDurationColumn = new ColumnHeader();
        playlistSourceColumn = new ColumnHeader();
        playerPanel = new Panel();
        playerSplitContainer = new SplitContainer();
        currentSongLabel = new Label();
        playbackControlsPanel = new Panel();
        previousButton = new Button();
        nextButton = new Button();
        repeatButton = new Button();
        shuffleButton = new Button();
        stopButton = new Button();
        playPauseButton = new Button();
        seekBarPanel = new Panel();
        seekBar = new ProgressBar();
        elapsedTimeLabel = new Label();
        remainingTimeLabel = new Label();
        statusStrip = new StatusStrip();
        downloadProgressBar = new ToolStripProgressBar();
        statusLabel = new ToolStripStatusLabel();
        timingLabel = new ToolStripStatusLabel();
        volumeLabel = new ToolStripStatusLabel();
        volumeTrackBar = new TrackBar();
        menuStrip.SuspendLayout();
        mainTabControl.SuspendLayout();
        searchTabPage.SuspendLayout();
        searchControlsPanel.SuspendLayout();
        queueTabPage.SuspendLayout();
        playlistTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)playlistSplitContainer).BeginInit();
        playlistSplitContainer.Panel1.SuspendLayout();
        playlistSplitContainer.Panel2.SuspendLayout();
        playlistSplitContainer.SuspendLayout();
        playerPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)playerSplitContainer).BeginInit();
        playerSplitContainer.Panel1.SuspendLayout();
        playerSplitContainer.Panel2.SuspendLayout();
        playerSplitContainer.SuspendLayout();
        playbackControlsPanel.SuspendLayout();
        seekBarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)volumeTrackBar).BeginInit();
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
        reloadPlaylistsMenuItem.Size = new Size(174, 22);
        reloadPlaylistsMenuItem.Text = "&Reload Playlists";
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
        exitMenuItem.Size = new Size(174, 22);
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
        playMenuItem.Size = new Size(213, 22);
        playMenuItem.Text = "&Play";
        // 
        // pauseMenuItem
        // 
        pauseMenuItem.Name = "pauseMenuItem";
        pauseMenuItem.Size = new Size(213, 22);
        pauseMenuItem.Text = "&Pause";
        // 
        // stopMenuItem
        // 
        stopMenuItem.Name = "stopMenuItem";
        stopMenuItem.Size = new Size(213, 22);
        stopMenuItem.Text = "&Stop";
        // 
        // playbackSeparator1
        // 
        playbackSeparator1.Name = "playbackSeparator1";
        playbackSeparator1.Size = new Size(210, 6);
        // 
        // nextMenuItem
        // 
        nextMenuItem.Name = "nextMenuItem";
        nextMenuItem.ShortcutKeys = Keys.Control | Keys.Right;
        nextMenuItem.Size = new Size(213, 22);
        nextMenuItem.Text = "&Next Track";
        // 
        // previousMenuItem
        // 
        previousMenuItem.Name = "previousMenuItem";
        previousMenuItem.ShortcutKeys = Keys.Control | Keys.Left;
        previousMenuItem.Size = new Size(213, 22);
        previousMenuItem.Text = "&Previous Track";
        // 
        // volumeUpMenuItem
        // 
        volumeUpMenuItem.Name = "volumeUpMenuItem";
        volumeUpMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
        volumeUpMenuItem.Size = new Size(213, 22);
        volumeUpMenuItem.Text = "Volume &Up";
        // 
        // volumeDownMenuItem
        // 
        volumeDownMenuItem.Name = "volumeDownMenuItem";
        volumeDownMenuItem.ShortcutKeys = Keys.Control | Keys.Down;
        volumeDownMenuItem.Size = new Size(213, 22);
        volumeDownMenuItem.Text = "Volume &Down";
        // 
        // playbackSeparator2
        // 
        playbackSeparator2.Name = "playbackSeparator2";
        playbackSeparator2.Size = new Size(210, 6);
        // 
        // repeatMenuItem
        // 
        repeatMenuItem.Name = "repeatMenuItem";
        repeatMenuItem.ShortcutKeys = Keys.Control | Keys.R;
        repeatMenuItem.Size = new Size(213, 22);
        repeatMenuItem.Text = "&Repeat Mode";
        // 
        // shuffleMenuItem
        // 
        shuffleMenuItem.Name = "shuffleMenuItem";
        shuffleMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        shuffleMenuItem.Size = new Size(213, 22);
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
        showPlaylistsMenuItem.CheckState = CheckState.Checked;
        showPlaylistsMenuItem.Name = "showPlaylistsMenuItem";
        showPlaylistsMenuItem.Size = new Size(148, 22);
        showPlaylistsMenuItem.Text = "Show &Playlists";
        // 
        // showSearchMenuItem
        // 
        showSearchMenuItem.Checked = true;
        showSearchMenuItem.CheckOnClick = true;
        showSearchMenuItem.CheckState = CheckState.Checked;
        showSearchMenuItem.Name = "showSearchMenuItem";
        showSearchMenuItem.Size = new Size(148, 22);
        showSearchMenuItem.Text = "Show &Search";
        // 
        // toggleDarkModeMenuItem
        // 
        toggleDarkModeMenuItem.Name = "toggleDarkModeMenuItem";
        toggleDarkModeMenuItem.Size = new Size(77, 20);
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
        helpMenuItem.Size = new Size(107, 22);
        helpMenuItem.Text = "&Help";
        // 
        // aboutMenuItem
        // 
        aboutMenuItem.Name = "aboutMenuItem";
        aboutMenuItem.Size = new Size(107, 22);
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
        mainTabControl.Size = new Size(1000, 464);
        mainTabControl.TabIndex = 1;
        // 
        // searchTabPage
        // 
        searchTabPage.Controls.Add(searchListView);
        searchTabPage.Controls.Add(searchControlsPanel);
        searchTabPage.Location = new Point(4, 24);
        searchTabPage.Name = "searchTabPage";
        searchTabPage.Padding = new Padding(3);
        searchTabPage.Size = new Size(992, 436);
        searchTabPage.TabIndex = 0;
        searchTabPage.Text = "🔍 Search";
        searchTabPage.UseVisualStyleBackColor = true;
        // 
        // searchListView
        // 
        searchListView.Columns.AddRange(new ColumnHeader[] { searchTitleColumn, searchArtistColumn, searchDurationColumn, searchSourceColumn });
        searchListView.Dock = DockStyle.Fill;
        searchListView.FullRowSelect = true;
        searchListView.GridLines = true;
        searchListView.Location = new Point(3, 53);
        searchListView.Name = "searchListView";
        searchListView.Size = new Size(986, 380);
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
        searchTextBox.Size = new Size(882, 23);
        searchTextBox.TabIndex = 0;
        // 
        // searchButton
        // 
        searchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        searchButton.Location = new Point(898, 13);
        searchButton.Name = "searchButton";
        searchButton.Size = new Size(75, 25);
        searchButton.TabIndex = 1;
        searchButton.Text = "Search";
        searchButton.UseVisualStyleBackColor = true;
        // 
        // queueTabPage
        // 
        queueTabPage.Controls.Add(queueListView);
        queueTabPage.Dock = DockStyle.Fill;
        queueTabPage.Location = new Point(4, 24);
        queueTabPage.Name = "queueTabPage";
        queueTabPage.Padding = new Padding(3);
        queueTabPage.Size = new Size(992, 438);
        queueTabPage.TabIndex = 1;
        queueTabPage.Text = "📋 Queue";
        queueTabPage.UseVisualStyleBackColor = true;
        // 
        // queueListView
        // 
        queueListView.Columns.AddRange(new ColumnHeader[] { queueTitleColumn, queueArtistColumn, queueDurationColumn, queueSourceColumn });
        queueListView.Dock = DockStyle.Fill;
        queueListView.FullRowSelect = true;
        queueListView.GridLines = true;
        queueListView.Location = new Point(3, 3);
        queueListView.Name = "queueListView";
        queueListView.Size = new Size(986, 432);
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
        // playlistTabPage
        // 
        playlistTabPage.Controls.Add(playlistSplitContainer);
        playlistTabPage.Location = new Point(4, 24);
        playlistTabPage.Name = "playlistTabPage";
        playlistTabPage.Padding = new Padding(3);
        playlistTabPage.Size = new Size(992, 438);
        playlistTabPage.TabIndex = 2;
        playlistTabPage.Text = "🎵 Playlists";
        playlistTabPage.UseVisualStyleBackColor = true;
        // 
        // playlistSplitContainer
        // 
        playlistSplitContainer.Dock = DockStyle.Fill;
        playlistSplitContainer.Location = new Point(3, 3);
        playlistSplitContainer.Name = "playlistSplitContainer";
        // 
        // playlistSplitContainer.Panel1
        // 
        playlistSplitContainer.Panel1.Controls.Add(playlistsListBox);
        // 
        // playlistSplitContainer.Panel2
        // 
        playlistSplitContainer.Panel2.Controls.Add(playlistListView);
        playlistSplitContainer.Size = new Size(986, 432);
        playlistSplitContainer.SplitterDistance = 250;
        playlistSplitContainer.TabIndex = 0;
        // 
        // playlistsListBox
        // 
        playlistsListBox.Dock = DockStyle.Fill;
        playlistsListBox.FormattingEnabled = true;
        playlistsListBox.Location = new Point(0, 0);
        playlistsListBox.Name = "playlistsListBox";
        playlistsListBox.Size = new Size(250, 432);
        playlistsListBox.TabIndex = 0;
        // 
        // playlistListView
        // 
        playlistListView.Columns.AddRange(new ColumnHeader[] { playlistTitleColumn, playlistArtistColumn, playlistDurationColumn, playlistSourceColumn });
        playlistListView.Dock = DockStyle.Fill;
        playlistListView.FullRowSelect = true;
        playlistListView.GridLines = true;
        playlistListView.Location = new Point(0, 0);
        playlistListView.Name = "playlistListView";
        playlistListView.Size = new Size(732, 432);
        playlistListView.TabIndex = 0;
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
        playerPanel.Controls.Add(playerSplitContainer);
        playerPanel.Dock = DockStyle.Bottom;
        playerPanel.Location = new Point(0, 488);
        playerPanel.Name = "playerPanel";
        playerPanel.Size = new Size(1000, 80);
        playerPanel.TabIndex = 2;
        // 
        // playerSplitContainer
        // 
        playerSplitContainer.Dock = DockStyle.Fill;
        playerSplitContainer.Location = new Point(0, 0);
        playerSplitContainer.Name = "playerSplitContainer";
        playerSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // playerSplitContainer.Panel1
        // 
        playerSplitContainer.Panel1.Controls.Add(currentSongLabel);
        // 
        // playerSplitContainer.Panel2
        // 
        playerSplitContainer.Panel2.Controls.Add(playbackControlsPanel);
        playerSplitContainer.Panel2.Controls.Add(seekBarPanel);
        playerSplitContainer.Size = new Size(1000, 80);
        playerSplitContainer.SplitterDistance = 25;
        playerSplitContainer.TabIndex = 0;
        // 
        // currentSongLabel
        // 
        currentSongLabel.Dock = DockStyle.Fill;
        currentSongLabel.Location = new Point(0, 0);
        currentSongLabel.Name = "currentSongLabel";
        currentSongLabel.Size = new Size(1000, 25);
        currentSongLabel.TabIndex = 0;
        currentSongLabel.Text = "No song selected";
        currentSongLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // playbackControlsPanel
        // 
        playbackControlsPanel.Controls.Add(previousButton);
        playbackControlsPanel.Controls.Add(nextButton);
        playbackControlsPanel.Controls.Add(repeatButton);
        playbackControlsPanel.Controls.Add(shuffleButton);
        playbackControlsPanel.Controls.Add(stopButton);
        playbackControlsPanel.Controls.Add(playPauseButton);
        playbackControlsPanel.Dock = DockStyle.Bottom;
        playbackControlsPanel.Location = new Point(0, 21);
        playbackControlsPanel.Name = "playbackControlsPanel";
        playbackControlsPanel.Size = new Size(1000, 30);
        playbackControlsPanel.TabIndex = 6;
        // 
        // previousButton
        // 
        previousButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        previousButton.Location = new Point(122, 2);
        previousButton.Name = "previousButton";
        previousButton.Size = new Size(50, 25);
        previousButton.TabIndex = 3;
        previousButton.Text = "⏮";
        previousButton.UseVisualStyleBackColor = true;
        // 
        // nextButton
        // 
        nextButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        nextButton.Location = new Point(178, 2);
        nextButton.Name = "nextButton";
        nextButton.Size = new Size(50, 25);
        nextButton.TabIndex = 4;
        nextButton.Text = "⏭";
        nextButton.UseVisualStyleBackColor = true;
        // 
        // repeatButton
        // 
        repeatButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        repeatButton.Location = new Point(234, 2);
        repeatButton.Name = "repeatButton";
        repeatButton.Size = new Size(50, 25);
        repeatButton.TabIndex = 5;
        repeatButton.Text = "🔁";
        repeatButton.UseVisualStyleBackColor = true;
        // 
        // shuffleButton
        // 
        shuffleButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        shuffleButton.Location = new Point(290, 2);
        shuffleButton.Name = "shuffleButton";
        shuffleButton.Size = new Size(50, 25);
        shuffleButton.TabIndex = 6;
        shuffleButton.Text = "🔀";
        shuffleButton.UseVisualStyleBackColor = true;
        // 
        // stopButton
        // 
        stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        stopButton.Location = new Point(66, 2);
        stopButton.Name = "stopButton";
        stopButton.Size = new Size(50, 25);
        stopButton.TabIndex = 2;
        stopButton.Text = "⏹";
        stopButton.UseVisualStyleBackColor = true;
        // 
        // playPauseButton
        // 
        playPauseButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        playPauseButton.Location = new Point(10, 2);
        playPauseButton.Name = "playPauseButton";
        playPauseButton.Size = new Size(50, 25);
        playPauseButton.TabIndex = 0;
        playPauseButton.Text = "▶";
        playPauseButton.UseVisualStyleBackColor = true;
        // 
        // seekBarPanel
        // 
        seekBarPanel.Controls.Add(seekBar);
        seekBarPanel.Controls.Add(elapsedTimeLabel);
        seekBarPanel.Controls.Add(remainingTimeLabel);
        seekBarPanel.Dock = DockStyle.Fill;
        seekBarPanel.Location = new Point(0, 0);
        seekBarPanel.Name = "seekBarPanel";
        seekBarPanel.Size = new Size(1000, 51);
        seekBarPanel.TabIndex = 0;
        // 
        // seekBar
        // 
        seekBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        seekBar.Location = new Point(50, 1);
        seekBar.Name = "seekBar";
        seekBar.Size = new Size(880, 23);
        seekBar.TabIndex = 1;
        // 
        // elapsedTimeLabel
        // 
        elapsedTimeLabel.AutoSize = true;
        elapsedTimeLabel.Location = new Point(10, 4);
        elapsedTimeLabel.Name = "elapsedTimeLabel";
        elapsedTimeLabel.Size = new Size(34, 15);
        elapsedTimeLabel.TabIndex = 2;
        elapsedTimeLabel.Text = "00:00";
        elapsedTimeLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // remainingTimeLabel
        // 
        remainingTimeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        remainingTimeLabel.AutoSize = true;
        remainingTimeLabel.Cursor = Cursors.Hand;
        remainingTimeLabel.Location = new Point(946, 4);
        remainingTimeLabel.Name = "remainingTimeLabel";
        remainingTimeLabel.Size = new Size(34, 15);
        remainingTimeLabel.TabIndex = 3;
        remainingTimeLabel.Text = "00:00";
        remainingTimeLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { downloadProgressBar, statusLabel, timingLabel, volumeLabel });
        statusStrip.Location = new Point(0, 568);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1000, 24);
        statusStrip.TabIndex = 3;
        statusStrip.Text = "statusStrip1";
        // 
        // downloadProgressBar
        // 
        downloadProgressBar.Name = "downloadProgressBar";
        downloadProgressBar.Size = new Size(100, 18);
        downloadProgressBar.Style = ProgressBarStyle.Continuous;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(849, 19);
        statusLabel.Spring = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // timingLabel
        // 
        timingLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        timingLabel.BorderStyle = Border3DStyle.Etched;
        timingLabel.Name = "timingLabel";
        timingLabel.Size = new Size(4, 19);
        timingLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // volumeLabel
        // 
        volumeLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        volumeLabel.BorderStyle = Border3DStyle.Etched;
        volumeLabel.Name = "volumeLabel";
        volumeLabel.Size = new Size(30, 19);
        volumeLabel.Text = "Vol:";
        volumeLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // volumeTrackBar
        // 
        volumeTrackBar.Anchor = AnchorStyles.Right;
        volumeTrackBar.Location = new Point(900, 3);
        volumeTrackBar.Maximum = 100;
        volumeTrackBar.Name = "volumeTrackBar";
        volumeTrackBar.Size = new Size(100, 45);
        volumeTrackBar.TabIndex = 0;
        volumeTrackBar.Value = 50;
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
        Controls.Add(volumeTrackBar);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(800, 630);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Simple Music Player";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        mainTabControl.ResumeLayout(false);
        searchTabPage.ResumeLayout(false);
        searchControlsPanel.ResumeLayout(false);
        searchControlsPanel.PerformLayout();
        queueTabPage.ResumeLayout(false);
        playlistTabPage.ResumeLayout(false);
        playlistSplitContainer.Panel1.ResumeLayout(false);
        playlistSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)playlistSplitContainer).EndInit();
        playlistSplitContainer.ResumeLayout(false);
        playerPanel.ResumeLayout(false);
        playerSplitContainer.Panel1.ResumeLayout(false);
        playerSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)playerSplitContainer).EndInit();
        playerSplitContainer.ResumeLayout(false);
        playbackControlsPanel.ResumeLayout(false);
        seekBarPanel.ResumeLayout(false);
        seekBarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)volumeTrackBar).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
