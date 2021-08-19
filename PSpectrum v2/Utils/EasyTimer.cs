﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSpectrum.Utils
{
    /// <summary>
    /// Simplified version of the System.Timers.Timer-Type.
    /// Exported from PLib (not released yet)
    /// </summary>
    class EasyTimer : System.Timers.Timer
    {
        /// <summary>
        /// Simplified version of the System.Timers.Timer-Type.
        /// Exported from PLib (not released yet)
        /// </summary>
        public EasyTimer(double interval)
        {
            this.Interval = interval;
            this.Enabled = true;
            this.AutoReset = true;
        }
    }
}
