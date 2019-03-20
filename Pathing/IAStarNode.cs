using System;

namespace WhiteStone.GameLib.Pathing
{
   /// <summary>
   /// Represents a node in A*.
   /// </summary>
   public interface IAStarNode
   {
      Int32 X { get; }
      Int32 Y { get; }
   }
}
