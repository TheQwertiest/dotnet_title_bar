using Qwr.ComponentInterface;

namespace fooTitle
{

    public delegate void OnPlaybackTimeDelegate(double time);
    public delegate void OnPlaybackNewTrackDelegate(IMetadbHandle song);
    public delegate void OnQuitDelegate();
    public delegate void OnInitDelegate();
    public delegate void OnPlaybackStopDelegate(PlaybackStopReason reason);
    public delegate void OnPlaybackPauseDelegate(bool isPaused);
    public delegate void OnPlaybackDynamicInfoTrackDelegate(IFileInfo fileInfo);

    public interface ICallbackSender
    {
        event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        event OnPlaybackStopDelegate OnPlaybackStopEvent;
        event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;
        event OnQuitDelegate OnQuitEvent;
        event OnInitDelegate OnInitEvent;
    }
}