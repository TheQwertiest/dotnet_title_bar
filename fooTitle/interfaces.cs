using System;

using fooManagedWrapper;

namespace fooTitle {

    public delegate void OnPlaybackTimeDelegate(double time);
    public delegate void OnPlaybackNewTrackDelegate(MetaDBHandle song);
    public delegate void OnQuitDelegate();
    public delegate void OnInitDelegate();
    public delegate void OnPlaybackStopDelegate(IPlayControl.StopReason reason);
    public delegate void OnPlaybackPauseDelegate(bool state);


    public interface IPlayCallbackSender {
        event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        event OnPlaybackStopDelegate OnPlaybackStopEvent;
        event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        event OnQuitDelegate OnQuitEvent;
        event OnInitDelegate OnInitEvent;
    }
}