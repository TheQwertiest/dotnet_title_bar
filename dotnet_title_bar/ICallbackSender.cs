using Qwr.ComponentInterface;

namespace fooTitle
{

    public delegate void QuitEventHandler();
    public delegate void InitializedEventHandler();
    public delegate void TrackPlaybackPositionChangedEventHandler(double time);
    public delegate void PlaybackAdvancedToNewTrackEventHandler(IMetadbHandle metadb);
    public delegate void PlaybackStoppedEventhandler(PlaybackStopReason reason);
    public delegate void PlaybackPausedStateChangedEventHandler(bool isPaused);
    public delegate void DynamicTrackInfoChangedEventHandler(IFileInfo fileInfo);

    public interface ICallbackSender
    {
        event QuitEventHandler Quit;
        event InitializedEventHandler Initialized;
        event TrackPlaybackPositionChangedEventHandler TrackPlaybackPositionChanged;
        event PlaybackAdvancedToNewTrackEventHandler PlaybackAdvancedToNewTrack;
        event PlaybackStoppedEventhandler PlaybackStopped;
        event PlaybackPausedStateChangedEventHandler PlaybackPausedStateChanged;
        event DynamicTrackInfoChangedEventHandler DynamicTrackInfoChanged;
    }
}