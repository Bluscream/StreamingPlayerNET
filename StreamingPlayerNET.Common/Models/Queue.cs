namespace StreamingPlayerNET.Common.Models;

public enum RepeatMode
{
    None,
    One,
    All
}

public class Queue : Playlist
{
    private int _currentIndex = -1;
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleEnabled = false;
    private List<int> _originalOrder = new();
    private List<int> _shuffledOrder = new();
    
    public Queue() : base()
    {
        Id = "queue";
        Name = "Now Playing";
        Description = "Current playback queue";
    }
    
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (value >= -1 && value < Songs.Count)
            {
                var oldIndex = _currentIndex;
                _currentIndex = value;
                if (oldIndex != _currentIndex)
                {
                    OnCurrentIndexChanged?.Invoke(this, _currentIndex);
                }
            }
        }
    }
    
    public RepeatMode RepeatMode
    {
        get => _repeatMode;
        set
        {
            _repeatMode = value;
            OnRepeatModeChanged?.Invoke(this, value);
        }
    }
    
    public bool ShuffleEnabled
    {
        get => _shuffleEnabled;
        set
        {
            if (_shuffleEnabled != value)
            {
                _shuffleEnabled = value;
                if (value)
                {
                    EnableShuffle();
                }
                else
                {
                    DisableShuffle();
                }
                OnShuffleChanged?.Invoke(this, value);
            }
        }
    }
    
    public event EventHandler<RepeatMode>? OnRepeatModeChanged;
    public event EventHandler<bool>? OnShuffleChanged;
    public event Action? OnSongsChanged;
    public event EventHandler<int>? OnCurrentIndexChanged;
    
    public Song? CurrentSong => _currentIndex >= 0 && _currentIndex < Songs.Count ? Songs[_currentIndex] : null;
    
    public Song? NextSong => _currentIndex + 1 < Songs.Count ? Songs[_currentIndex + 1] : null;
    
    public Song? PreviousSong => _currentIndex - 1 >= 0 ? Songs[_currentIndex - 1] : null;
    
    public bool HasNext => _currentIndex + 1 < Songs.Count;
    
    public bool HasPrevious => _currentIndex - 1 >= 0;
    
    public new void AddSong(Song song)
    {
        // Set the AddedToQueueAt timestamp for the song
        song.AddedToQueueAt = DateTime.Now;
        Songs.Add(song);
        if (_shuffleEnabled)
        {
            UpdateShuffleOrder();
        }
        OnSongsChanged?.Invoke();
    }
    
    public void AddSongs(IEnumerable<Song> songs)
    {
        foreach (var song in songs)
        {
            // Set the AddedToQueueAt timestamp for the song
            song.AddedToQueueAt = DateTime.Now;
            Songs.Add(song);
        }
        if (_shuffleEnabled)
        {
            UpdateShuffleOrder();
        }
        OnSongsChanged?.Invoke();
    }
    
    public void InsertSong(int index, Song song)
    {
        if (index >= 0 && index <= Songs.Count)
        {
            // Set the AddedToQueueAt timestamp for the song
            song.AddedToQueueAt = DateTime.Now;
            Songs.Insert(index, song);
            if (_currentIndex >= index)
            {
                _currentIndex++;
            }
            if (_shuffleEnabled)
            {
                UpdateShuffleOrder();
            }
            OnSongsChanged?.Invoke();
        }
    }
    
    public void RemoveSong(int index)
    {
        if (index >= 0 && index < Songs.Count)
        {
            Songs.RemoveAt(index);
            if (_currentIndex == index)
            {
                _currentIndex = -1; // No current song
            }
            else if (_currentIndex > index)
            {
                _currentIndex--;
            }
            if (_shuffleEnabled)
            {
                UpdateShuffleOrder();
            }
            OnSongsChanged?.Invoke();
        }
    }
    
    public void RemoveSong(Song song)
    {
        var index = Songs.IndexOf(song);
        if (index >= 0)
        {
            RemoveSong(index);
        }
    }
    
    public void RemoveSongAt(int index)
    {
        RemoveSong(index);
    }
    
    public void MoveSong(int fromIndex, int toIndex)
    {
        if (fromIndex >= 0 && fromIndex < Songs.Count && 
            toIndex >= 0 && toIndex < Songs.Count && 
            fromIndex != toIndex)
        {
            var song = Songs[fromIndex];
            Songs.RemoveAt(fromIndex);
            Songs.Insert(toIndex, song);
            
            // Update current index if needed
            if (_currentIndex == fromIndex)
            {
                _currentIndex = toIndex;
            }
            else if (_currentIndex > fromIndex && _currentIndex <= toIndex)
            {
                _currentIndex--;
            }
            else if (_currentIndex < fromIndex && _currentIndex >= toIndex)
            {
                _currentIndex++;
            }
            
            // Update shuffle order if shuffle is enabled
            if (_shuffleEnabled)
            {
                UpdateShuffleOrder();
            }
            OnSongsChanged?.Invoke();
        }
    }
    
    public new void Clear()
    {
        Songs.Clear();
        _currentIndex = -1;
        _originalOrder.Clear();
        _shuffledOrder.Clear();
        OnSongsChanged?.Invoke();
    }
    
    public void MoveToNext()
    {
        if (HasNext)
        {
            _currentIndex++;
        }
        else if (_repeatMode == RepeatMode.All)
        {
            // Loop back to the beginning
            _currentIndex = 0;
        }
    }
    
    public void MoveToPrevious()
    {
        if (HasPrevious)
        {
            _currentIndex--;
        }
        else if (_repeatMode == RepeatMode.All)
        {
            // Loop to the end
            _currentIndex = Songs.Count - 1;
        }
    }
    
    public void MoveToIndex(int index)
    {
        if (index >= -1 && index < Songs.Count)
        {
            var oldIndex = _currentIndex;
            _currentIndex = index;
            // Trigger event only if index actually changed
            if (oldIndex != _currentIndex)
            {
                OnCurrentIndexChanged?.Invoke(this, _currentIndex);
            }
        }
    }
    
    public void ToggleRepeatMode()
    {
        RepeatMode = RepeatMode switch
        {
            RepeatMode.None => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            RepeatMode.One => RepeatMode.None,
            _ => RepeatMode.None
        };
    }
    
    public void ToggleShuffle()
    {
        ShuffleEnabled = !ShuffleEnabled;
    }
    
    private void EnableShuffle()
    {
        if (Songs.Count > 0)
        {
            // Store original order
            _originalOrder = Enumerable.Range(0, Songs.Count).ToList();
            
            // Create shuffled order
            UpdateShuffleOrder();
            
            // If we have a current song, find its new position in shuffled order
            if (_currentIndex >= 0)
            {
                var newIndex = _shuffledOrder.IndexOf(_currentIndex);
                if (newIndex >= 0)
                {
                    _currentIndex = newIndex;
                }
            }
        }
    }
    
    private void DisableShuffle()
    {
        if (Songs.Count > 0 && _currentIndex >= 0)
        {
            // Find the current song in the original order
            var originalIndex = _shuffledOrder[_currentIndex];
            _currentIndex = originalIndex;
        }
        
        _originalOrder.Clear();
        _shuffledOrder.Clear();
    }
    
    private void UpdateShuffleOrder()
    {
        if (!_shuffleEnabled || Songs.Count == 0) return;
        
        _originalOrder = Enumerable.Range(0, Songs.Count).ToList();
        _shuffledOrder = _originalOrder.ToList();
        
        var random = new Random();
        // Fisher-Yates shuffle
        for (int i = _shuffledOrder.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = _shuffledOrder[i];
            _shuffledOrder[i] = _shuffledOrder[j];
            _shuffledOrder[j] = temp;
        }
    }
    
    public void Shuffle()
    {
        var random = new Random();
        var songsArray = Songs.ToArray();
        
        // Fisher-Yates shuffle
        for (int i = songsArray.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = songsArray[i];
            songsArray[i] = songsArray[j];
            songsArray[j] = temp;
        }
        
        Songs.Clear();
        foreach (var song in songsArray)
        {
            Songs.Add(song);
        }
        
        // Reset current index
        _currentIndex = -1;
        
        // Update shuffle order if shuffle is enabled
        if (_shuffleEnabled)
        {
            UpdateShuffleOrder();
        }
        OnSongsChanged?.Invoke();
    }
    
    public void Reverse()
    {
        var songsArray = Songs.ToArray();
        Array.Reverse(songsArray);
        
        Songs.Clear();
        foreach (var song in songsArray)
        {
            Songs.Add(song);
        }
        
        // Update current index
        if (_currentIndex >= 0)
        {
            _currentIndex = Songs.Count - 1 - _currentIndex;
        }
        
        // Update shuffle order if shuffle is enabled
        if (_shuffleEnabled)
        {
            UpdateShuffleOrder();
        }
        OnSongsChanged?.Invoke();
    }
    
    public List<Song> GetSongsAfterCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= Songs.Count - 1)
            return new List<Song>();
        
        return Songs.Skip(_currentIndex + 1).ToList();
    }
    
    public List<Song> GetSongsBeforeCurrent()
    {
        if (_currentIndex <= 0)
            return new List<Song>();
        
        return Songs.Take(_currentIndex).ToList();
    }
    
    public string GetRepeatModeText()
    {
        return RepeatMode switch
        {
            RepeatMode.None => "No Repeat",
            RepeatMode.One => "Repeat One",
            RepeatMode.All => "Repeat All",
            _ => "Unknown"
        };
    }
    
    public string GetShuffleText()
    {
        return ShuffleEnabled ? "Shuffle On" : "Shuffle Off";
    }
    
    /// <summary>
    /// Saves the current position of the current song
    /// </summary>
    public void SaveCurrentSongPosition()
    {
        if (CurrentSong != null)
        {
            CurrentSong.SaveCurrentPosition();
        }
    }
    
    /// <summary>
    /// Restores the saved position of the current song
    /// </summary>
    public void RestoreCurrentSongPosition()
    {
        if (CurrentSong != null)
        {
            CurrentSong.RestorePosition();
        }
    }
    
    /// <summary>
    /// Records the start of playback for the current song
    /// </summary>
    public void RecordCurrentSongPlaybackStart()
    {
        if (CurrentSong != null)
        {
            CurrentSong.RecordPlaybackStart();
        }
    }
    
    /// <summary>
    /// Records the pause/stop of playback for the current song
    /// </summary>
    public void RecordCurrentSongPlaybackPause()
    {
        if (CurrentSong != null)
        {
            CurrentSong.RecordPlaybackPause();
        }
    }
}