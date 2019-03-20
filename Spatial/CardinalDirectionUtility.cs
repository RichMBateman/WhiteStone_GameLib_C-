
using System;

namespace WhiteStone.GameLib.Spatial
{
   /// <summary>
   /// Helper functions related to CardinalDirection.
   /// </summary>
   public static class CardinalDirectionUtility
   {
      /// <summary>
      /// Represents a bitmask with all directions set.
      /// </summary>
      public const CardinalDirection DirectionsAll = (CardinalDirection.East | CardinalDirection.North | CardinalDirection.South | CardinalDirection.West);

      /// <summary>
      /// Given a coordinate, returns the new coordinate after applying the CardinalDirection.  
      /// Adjusts the position by 1 unit.
      /// </summary>
      public static void ApplyCardinalDirectionToCoordinate(CardinalDirection dir, Int32 x, Int32 y, out Int32 adjX, out Int32 adjY)
      {
         adjX = x;
         adjY = y;
         switch (dir)
         {
            case CardinalDirection.East: adjX++; break;
            case CardinalDirection.North: adjY--; break;
            case CardinalDirection.South: adjY++; break;
            case CardinalDirection.West: adjX--; break;
         }
      }

      /// <summary>
      /// Returns a new enum value that represents the directions in the supplied bitmask rotated 180 degrees.
      /// </summary>
      public static CardinalDirection RotateDirectionsDegrees180(CardinalDirection original)
      {
         CardinalDirection rotated = CardinalDirection.None;
         if (original.HasFlag(CardinalDirection.North))
         {
            rotated |= CardinalDirection.South;
         }
         if (original.HasFlag(CardinalDirection.East))
         {
            rotated |= CardinalDirection.West;
         }
         if (original.HasFlag(CardinalDirection.South))
         {
            rotated |= CardinalDirection.North;
         }
         if (original.HasFlag(CardinalDirection.West))
         {
            rotated |= CardinalDirection.East;
         }

         return rotated;
      }
   }
}
