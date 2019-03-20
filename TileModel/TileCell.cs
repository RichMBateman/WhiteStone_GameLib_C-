using System;
using WhiteStone.GameLib.Pathing;

namespace WhiteStone.GameLib.TileModel
{
   /// <summary>
   /// Represents a single cell on a TileMap.  A node.
   /// </summary>
   public class TileCell : IAStarNode
   {
      #region Public Properties

      /// <summary>
      /// A key for the parent tile map.  Used for testing equality.
      /// </summary>
      public String ParentKey { get; set; }
      /// <summary>
      /// The logical X position of this tile.
      /// </summary>
      public Int32 X { get; set; }
      /// <summary>
      /// The logical Y position of this tile.
      /// </summary>
      public Int32 Y { get; set; }
      /// <summary>
      /// Whether this is a void tile (an alternative to using a "null" to represent a non-tile).
      /// </summary>
      public Boolean IsVoid { get; set; }

      #endregion

      #region Public Methods

      #region IAStarNode Methods

      /// <summary>
      /// Returns whether we consider the supplied TileCell to equal this one.  They must have the
      /// same position.
      /// </summary>
      public override Boolean Equals(Object other)
      {
         Boolean equals = false;
         TileCell otherCell = other as TileCell;
         if (otherCell != null)
         {
            if (X == otherCell.X && Y == otherCell.Y && String.Equals(ParentKey, otherCell.ParentKey))
            {
               equals = true;
            }
         }
         return equals;
      }

      /// <summary>
      /// Used in hashing algorithms.
      /// </summary>
      public override Int32 GetHashCode()
      {
         return String.Format("{0}-{1}-{2}", ParentKey, X, Y).GetHashCode();
      }

      #endregion

      #endregion
   }
}
