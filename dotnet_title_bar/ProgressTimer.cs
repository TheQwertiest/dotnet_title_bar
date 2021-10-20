/*
*  This file is part of foo_title.
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*  
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*  
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
*  
*  See the file COPYING included with this distribution for more
*  information.
*/

using System;
using Timer = System.Windows.Forms.Timer;

namespace fooTitle
{

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
