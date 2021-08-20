namespace PSpectrum.Utils
{
    /// <summary>
    /// Simplified version of the System.Timers.Timer-Type.
    /// Exported from PLib (not released yet)
    /// </summary>
    internal class EasyTimer : System.Timers.Timer
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