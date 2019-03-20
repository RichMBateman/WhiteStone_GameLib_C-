using System;
using System.Collections.Generic;
using WhiteStone.GameLib.Pathing;
using WhiteStone.GameLib.Spatial;

namespace WhiteStone.GameLib.TileModel
{
   /// <summary>
   /// Represents a 2d map of TileCells.
   /// </summary>
   public class TileMap<T> : IAStarMap
      where T : TileCell, new()
   {
      #region Constructors

      /// <summary>
      /// Constructs a new tile map.
      /// </summary>
      public TileMap(String key, Int32 cellSize, Int32 widthInCells, Int32 heightInCells)
      {
         Key = key;
         TileSize = cellSize;
         WidthInCells = widthInCells;
         HeightInCells = heightInCells;
         InitializeMapMatrix();
         InitializeLogicalPosition();
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Unique key that identifies this map.  If you change the key, realize that anything referring to this key will need to be updated.
      /// </summary>
      public String Key { get; set; }

      /// <summary>
      /// Logical size of each square tile.
      /// </summary>
      public Int32 TileSize { get; private set; }

      /// <summary>
      /// The number of cells wide this map is
      /// </summary>
      public Int32 WidthInCells { get; private set; }

      /// <summary>
      /// The number of cells high this map is
      /// </summary>
      public Int32 HeightInCells { get; private set; }

      /// <summary>
      /// The logical position of this map within the camera space.  The camera space may be larger or small than the map logical size.
      /// </summary>
      public Position PositionLogical { get; protected set; }

      /// <summary>
      /// A matrix of map cells that describes this environment.  First index is the column, followed by the row.
      /// </summary>
      public T[,] MapMatrix { get; private set; }

      #endregion

      #region Public Static Functions

      /// <summary>
      /// Returns the cardinal direction opposite the supplied one.
      /// </summary>
      public static CardinalDirection GetOppositeCardinalDirection(CardinalDirection source)
      {
         CardinalDirection opposite = CardinalDirection.None;
         switch (source)
         {
            case CardinalDirection.North: opposite = CardinalDirection.South; break;
            case CardinalDirection.East: opposite = CardinalDirection.West; break;
            case CardinalDirection.South: opposite = CardinalDirection.North; break;
            case CardinalDirection.West: opposite = CardinalDirection.East; break;
         }
         return opposite;
      }

      #endregion

      #region Public Methods

      #region IAStar Public Methods

      public Double HeuristicCost(IAStarNode start, IAStarNode goal)
      {
         Double approximateCost = Math2d.GetDistanceBetweenTwoNodes(start, goal);
         return approximateCost;
      }

      public List<IAStarNode> NeighborNodes(IAStarNode current)
      {
         List<IAStarNode> neighbors = new List<IAStarNode>();

         TileCell northCell = GetCellCoordinateInDirection(current.X, current.Y, CardinalDirection.North);
         TileCell eastCell = GetCellCoordinateInDirection(current.X, current.Y, CardinalDirection.East);
         TileCell southCell = GetCellCoordinateInDirection(current.X, current.Y, CardinalDirection.South);
         TileCell westCell = GetCellCoordinateInDirection(current.X, current.Y, CardinalDirection.West);

         if (northCell != null && northCell.IsVoid) { northCell = null; }
         if (eastCell != null && eastCell.IsVoid) { eastCell = null; }
         if (southCell != null && southCell.IsVoid) { southCell = null; }
         if (westCell != null && westCell.IsVoid) { westCell = null; }

         if (northCell != null) { neighbors.Add(northCell); }
         if (eastCell != null) { neighbors.Add(eastCell); }
         if (southCell != null) { neighbors.Add(southCell); }
         if (westCell != null) { neighbors.Add(westCell); }

         return neighbors;
      }

      public double TrueCost(IAStarNode current, IAStarNode adjacent)
      {
         return 1; // Distance is always 1.
      }

      #endregion

      #region Cell Inspection

      /// <summary>
      /// Returns whether this cell coordinate is in map.
      /// </summary>
      public Boolean IsCellCoordinateWithinMap(Int32 x, Int32 y)
      {
         Boolean isInMap = (x >= 0 && x < WidthInCells && y >= 0 && y < HeightInCells);
         return isInMap;
      }

      public CardinalDirection GetAdjacentDirectionFromSourceToTargetCell(TileCell source, TileCell target)
      {
         CardinalDirection dir = CardinalDirection.None;

         if (source != null && target != null)
         {
            if (source.X == target.X)
            {
               if (target.Y > source.Y)
               {
                  dir = CardinalDirection.South;
               }
               else if (target.Y < source.Y)
               {
                  dir = CardinalDirection.North;
               }
            }
            else if (source.Y == target.Y)
            {
               if (target.X > source.X)
               {
                  dir = CardinalDirection.East;
               }
               else if (target.X < source.X)
               {
                  dir = CardinalDirection.West;
               }
            }
         }

         return dir;
      }

      #endregion

      #region Cell Retrieval

      public TileCell GetCellAtPosition(Position p)
      {
         TileCell cell = GetCellAtRealCoordinate(p.BottomCenterX, p.BottomCenterY);
         return cell;
      }

      public TileCell GetCellAtRealCoordinate(Double x, Double y)
      {
         Int32 mapColumn = (Int32)(x / TileSize);
         Int32 mapRow = (Int32)(y / TileSize);
         TileCell cell = GetCellAtCoordinate(mapColumn, mapRow);
         return cell;
      }

      /// <summary>
      /// Returns the cell at the given coordinate.  Returns null if the location is not valid.
      /// </summary>
      public T GetCellAtCoordinate(Int32 x, Int32 y)
      {
         T cell = null;
         if (IsCellCoordinateWithinMap(x, y))
         {
            cell = MapMatrix[x, y];
         }
         return cell;
      }

      public TileCell GetCellCoordinateInDirection(Int32 sourceX, Int32 sourceY, CardinalDirection dir)
      {
         Int32 targetX, targetY;
         TileCell cell = GetCellCoordinateInDirection(sourceX, sourceY, dir, out targetX, out targetY);
         return cell;
      }

      /// <summary>
      /// From some source position, given a direction, returns the cell coordinates in that direction.
      /// The returned coordinates may not be on the map, so null will be returned.
      /// </summary>
      public TileCell GetCellCoordinateInDirection(Int32 sourceX, Int32 sourceY, CardinalDirection dir, out Int32 targetX, out Int32 targetY)
      {
         targetX = sourceX;
         targetY = sourceY;

         switch (dir)
         {
            case CardinalDirection.North: targetY--; break;
            case CardinalDirection.East: targetX++; break;
            case CardinalDirection.South: targetY++; break;
            case CardinalDirection.West: targetX--; break;
         }

         TileCell targetCell = GetCellAtCoordinate(targetX, targetY);
         return targetCell;
      }

      #endregion

      #endregion

      #region Private Methods

      private void InitializeMapMatrix()
      {
         MapMatrix = new T[WidthInCells, HeightInCells];
         for (Int32 x = 0; x < WidthInCells; x++)
         {
            for (Int32 y = 0; y < HeightInCells; y++)
            {
               T cell = new T();
               cell.ParentKey = Key;
               MapMatrix[x, y] = cell;
               cell.X = x;
               cell.Y = y;
            }
         }
      }

      private void InitializeLogicalPosition()
      {
         // Position logical is initialized such that 0,0 is the upper left part of the tile map.
         PositionLogical = new Position(TileSize * WidthInCells / 2, TileSize * HeightInCells,
            TileSize * WidthInCells, TileSize * HeightInCells);
      }

      #endregion
   }
}
