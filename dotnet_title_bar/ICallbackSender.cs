using Qwr.ComponentInterface;

namespace fooTitle
{
    public delegate void Quit_EventHandler();
    public delegate void Initialized_EventHandler();
    public delegate void TrackPlaybackPositionChanged_EventHandler(double time);
    public delegate void PlaybackAdvancedToNewTrack_EventHandler(IMetadbHandle metadb);
    public delegate void PlaybackStoppedEventhandler(PlaybackStopReason reason);
    public delegate void PlaybackPausedStateChanged_EventHandler(bool isPaused);
    public delegate void DynamicTrackInfoChanged_EventHandler(IFileInfo fileInfo);

    public interface ICallbackSender
    {
        event Quit_EventHandler Quit;
        event Initialized_EventHandler Initialized;
        event TrackPlaybackPositionChanged_EventHandler TrackPlaybackPositionChanged;
        event PlaybackAdvancedToNewTrack_EventHandler PlaybackAdvancedToNewTrack;
        event PlaybackStoppedEventhandler PlaybackStopped;
        event PlaybackPausedStateChanged_EventHandler PlaybackPausedStateChanged;
        event DynamicTrackInfoChanged_EventHandler DynamicTrackInfoChanged;
    }
}
