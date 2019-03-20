using System;
using System.Timers;
using WhiteStone.GameLib.Interfaces;

/* Links:
 * http://stackoverflow.com/questions/1416803/system-timers-timer-vs-system-threading-timer
 * http://msdn.microsoft.com/en-us/library/system.timers.timer.enabled.aspx
 *      -Describes how setting Enabled to false/true is the SAME as calling Stop()/Start()
 */

namespace WhiteStone.GameLib.Core
{
   /// <summary>
   /// A low level object that envelops a logical module that can accept frame ticks.  Handles a game loop that runs at some interval.
   /// Used internally by the GameCore to run itself in a loop.
   /// </summary>
   internal class Simulation
   {
      #region Members

      private const Int32 DefaultTickDurationMs = 5;

      private Timer m_timer;
      private ITicker m_module;
      private Int32 m_tickDuration;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new simulation at some tick duration.
      /// </summary>
      /// <param name="tickDuration"></param>
      public Simulation(Int32 tickDuration = DefaultTickDurationMs)
      {
         m_tickDuration = tickDuration;
         SetupTimer();
      }

      #endregion

      #region Public Methods

      public void Start()
      {
         m_timer.Start();
      }

      public void Stop()
      {
         m_timer.Stop();
      }

      public void InstallModule(ITicker module)
      {
         m_module = module;
      }

      #endregion

      #region Initialization

      private void SetupTimer()
      {
         // By setting AutoReset to false, the timer will not continously fire the Elapsed event every Interval seconds.
         // Imagine that the work done during each Tick far exceeded the interval.  Now we can be certain that the work has finished before the timer fires again.
         // Link: http://www.codeproject.com/Questions/405564/Syste-Timers-Timer-single-threaded-usage
         m_timer = new Timer();
         m_timer.Interval = m_tickDuration;
         m_timer.AutoReset = false;
         m_timer.Elapsed += TimerElapsed;
      }

      #endregion

      #region Event-Handling

      private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
      {
         m_module.AcceptFrameTick();
         m_timer.Start();
      }

      #endregion
   }
}
