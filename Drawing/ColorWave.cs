using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WhiteStone.GameLib.Time;
using WhiteStone.GameLib.Interfaces;

namespace WhiteStone.GameLib.Drawing
{
   /// <summary>
   /// An object that can listen to a particular TickWave on the BaseGameModel.  Can slowly move between two color boundaries.
   /// </summary>
   public class ColorWave : ITicker
   {
      #region Public Properties

      /// <summary>
      /// Unique key for this ColorWave
      /// </summary>
      public String Key { get; set; }

      /// <summary>
      /// Color boundary 1.
      /// </summary>
      public Color ColorBoundary1 { get; set; }

      /// <summary>
      /// Color boundary 2.
      /// </summary>
      public Color ColorBoundary2 { get; set; }

      /// <summary>
      /// The current color
      /// </summary>
      public Color CurrentColor { get; private set; }

      /// <summary>
      /// The TickWave this ColorWave uses.
      /// </summary>
      public TickWave AssociatedTickWave { get; set; }

      #endregion

      #region Constructor

      /// <summary>
      /// Creates a new color wave, between two color boundaries, and a tick wave key.
      /// When the TickWave is at it's minimum, the Color produced by the wave will be ColorBoundary1; 
      /// when it is at it's maximum, ColorBoundary2.  Otherwise, somewhere inbetween.
      /// </summary>
      public ColorWave(String colorWaveKey, Color boundary1, Color boundary2, TickWave tickWave)
      {
         Key = colorWaveKey;
         ColorBoundary1 = boundary1;
         ColorBoundary2 = boundary2;
         CurrentColor = boundary1;
         AssociatedTickWave = tickWave;
      }

      #endregion

      #region Public Api

      public void AcceptFrameTick()
      {
         BuildCurrentColor();
      }

      #endregion

      #region Internal Api

      internal void BuildCurrentColor()
      {
         if (AssociatedTickWave != null)
         {
            // Calculate a percentage that represents where we are between the two boundary colors.  0% would be min color; 100% would be max.
            Double percAlongRange = CalculatePercentageAlongRange(AssociatedTickWave.TickCounterMin, AssociatedTickWave.TickCounterMax, AssociatedTickWave.TickCounter);

            // Calculate the r, g, b values for our target color
            Int32 r = CalculatePositionAlongRange(ColorBoundary1.R, ColorBoundary2.R, percAlongRange);
            Int32 g = CalculatePositionAlongRange(ColorBoundary1.G, ColorBoundary2.G, percAlongRange);
            Int32 b = CalculatePositionAlongRange(ColorBoundary1.B, ColorBoundary2.B, percAlongRange);

            CurrentColor = Color.FromArgb(r, g, b);
         }
      }

      #endregion

      #region Private Methods

      /// <summary>
      /// Given a minimum and maximum (for some range), and a current value (which should be between that range),
      /// calculates how far along the range percentage wise the current value is.  0% would mean the current value is the min;
      /// 100% means the max.
      /// </summary>
      private static Double CalculatePercentageAlongRange(Int32 min, Int32 max, Int32 currentValue)
      {
         // Sample calculations (min, max, currentValue):
         // 0, 100, 50:  range = 100, num. shfit = 0,  % = 50/100 = 50.0
         // 30, 60, 50:  range = 30, num. shfit = -30,  % = 20/30 = 66.6
         // -150, -80, -140:  range = 70, num. shfit = 150,  % = 10/70 = 14.2
         // -100, 200, 200:  range = 300, num. shfit = 100,  % = 300/300 = 100.0
         Double percentageAlongRange = 0;
         Double range = max - min;
         if (range > 0 && currentValue >= min && currentValue <= max)
         {
            Double numeratorShift = -min;
            percentageAlongRange = (currentValue + numeratorShift) / (range);
         }
         return percentageAlongRange;
      }

      /// <summary>
      /// Given a left boundary and right boundary (left doesn't have to be smaller than right), and a percentage along going from left to right,
      /// returns the number.
      /// </summary>
      private static Int32 CalculatePositionAlongRange(Int32 leftBoundary, Int32 rightBoundary, Double percentageAlongRange)
      {
         // Sample calculations (left, right, %)
         // 0, 255, 100 => range: 255, positionShift = 255, position = 255
         // 0, 255, 10 => range: 255, positionShift = 25, position = 25
         // 255, 0, 20 => range: 255, positionShift = 51, position = 204
         Double range = Math.Abs(leftBoundary - rightBoundary);
         Int32 positionShift = (Int32)(range * percentageAlongRange);
         Int32 position = 0;
         if (leftBoundary < rightBoundary)
         {
            position = leftBoundary + positionShift;
         }
         else
         {
            position = leftBoundary - positionShift;
         }
         return position;
      }

      #endregion
   }
}
