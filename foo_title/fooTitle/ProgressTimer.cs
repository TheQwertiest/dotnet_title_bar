using System;
using Timer = System.Windows.Forms.Timer;

namespace fooTitle {

    public class ProgressTimer
    {
        public float Progress = 0;
        private readonly Timer _progressTimer;
        private readonly EventHandler<float> _progressCallback;

        public ProgressTimer(EventHandler<float> progressCallback)
        {
            _progressTimer = new Timer { Interval = 500 };
            _progressCallback = progressCallback;
            _progressTimer.Tick += Update;
        }

        public void Start()
        {
            _progressTimer?.Stop();
            _progressTimer?.Start();
        }

        public void Update(object obj, EventArgs e)
        {
            _progressCallback(obj, Progress);
        }

        public void Stop()
        {
            _progressTimer?.Stop();
        }
    }
}
