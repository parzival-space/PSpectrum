namespace PLib
{
    /// <summary>
    /// Provides simplified types.
    /// </summary>
    public static class Generic
    {
        /// <summary>
        /// Simplified version of the System.Timers.Timer-Type
        /// </summary>
        public class EasyTimer : System.Timers.Timer
        {
            /// <summary>
            /// Simplified version of the System.Timers.Timer-Type
            /// </summary>
            public EasyTimer(double delay)
            {
                this.Interval = delay;
                this.Enabled = true;
                this.AutoReset = true;
                this.Start();
            }
        }
    }
}