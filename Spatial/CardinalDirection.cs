using System;

namespace WhiteStone.GameLib.Spatial
{
   [Flags]
   /// <summary>
   /// Describes a cardinal direction (or no direction).  Can be used in a bitmask.
   /// </summary>
   public enum CardinalDirection
   {
      None = 0,
      North = 1,
      East = 2,
      West = 4,
      South = 8,
   }
}
