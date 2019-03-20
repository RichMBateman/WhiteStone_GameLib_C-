using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhiteStone.GameLib.Interfaces;

namespace WhiteStone.GameLib.Time
{
   /// <summary>
   /// An object used to track numbers that should oscillate between two values with every tick of the model.
   /// You can provide it with a velocity, min, max, which will affect how quickly the number moves, and its bounds.
   /// </summary>
   public class TickWave : ITicker
   {
      #region Constants

      public const Int32 DefaultTickCounter = 0;
      public const Int32 DefaultTickCounterMin = 0;
      public const Int32 DefaultTickCounterMax = 255;
      public const Int32 DefaultTickVelocity = +1;

      #endregion

      #region Private Members

      private readonly String m_key;

      #endregion

      #region Constructors

      /// <summary>
      /// Instantiates a new Tick Wave.
      /// </summary>
      /// <param name="key"></param>
      internal TickWave(String key)
      {
         m_key = key;
         TickCounter = DefaultTickCounter;
         TickCounterMin = DefaultTickCounterMin;
         TickCounterMax = DefaultTickCounterMax;
         TickVelocity = DefaultTickVelocity;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Where this tick wave currently is.
      /// </summary>
      public Int32 TickCounter { get; set; }

      /// <summary>
      /// How low can the tick counter go?
      /// </summary>
      public Int32 TickCounterMin { get; set; }

      /// <summary>
      /// How high can the tick counter go?
      /// </summary>
      public Int32 TickCounterMax { get; set; }

      /// <summary>
      /// Velocity toward boundaries.  Positive or negative indicates direction.
      /// </summary>
      public Int32 TickVelocity { get; set; }

      /// <summary>
      /// The positive difference between the max and the min.
      /// </summary>
      public Int32 TickCounterRange
      {
         get
         {
            Int32 diff = TickCounterMax - TickCounterMin;
            return diff;
         }
      }

      #endregion

      #region Public Methods

      public void AcceptFrameTick()
      {
         TickCounter += TickVelocity;
         if (TickCounter >= TickCounterMax)
         {
            TickCounter = TickCounterMax;
            TickVelocity *= -1; // change direction
         }
         else
         {
            if (TickCounter <= TickCounterMin)
            {
               TickCounter = TickCounterMin;
               TickVelocity *= -1; // change direction
            }
         }
      }

      #endregion
   }
}
