using System;
using System.Drawing;

//using System.Drawing;

namespace WhiteStone.GameLib.Spatial
{
   /// <summary>
   /// Represents a rectangular region in a 2D world.  
   /// It consists of both a single coordinate (which represents its base) 
   /// and a rectangle that represents its area.
   /// It's a union of a rectangle and a point.  Generally, everything that has a position also 
   /// has a rectangle that represents its area.
   /// The rectangle is not rotated, and the position is ALWAYS the bottom center of the rectangle.
   /// For convenience, we also store velocity, acceleration, and direction.
   /// Use the factory methods to create positions if the existing constructors are not sufficient,
   /// because it's important to set the position and size at the same time.
   /// </summary>
   public class Position
   {
      #region Private Members

      private Double m_width, m_height;
      private Vector2d m_bottomCenter;
      private Vector2d m_velocity;
      private Vector2d m_acceleration;

      #endregion

      #region Factory methods

      /// <summary>
      /// Creates a position using upper left coordinates.
      /// </summary>
      public static Position CreateFromUL(Double upperLeftX, Double upperLeftY, Double width, Double height)
      {
         Position targetPosition = new Position();
         targetPosition.m_bottomCenter = new Vector2d(upperLeftX + width / 2, upperLeftY + height);
         targetPosition.m_width = width;
         targetPosition.m_height = height;
         return targetPosition;
      }

      /// <summary>
      /// Creates a position using the upper left coordinate information.
      /// </summary>
      public static Position CreatePositionFromULCoordinate(double upperLeftX, double upperLeftY,
         double width, double height)
      {
         Position p = new Position(upperLeftX + width / 2, upperLeftY + height,
            width, height);
         return p;
      }

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new, empty position.
      /// </summary>
      [Obsolete("Creating a position without any initial data is heavily prone to mistakes.  Avoid.")]
      public Position()
      {
         m_bottomCenter = new Vector2d();
         m_velocity = new Vector2d();
         m_acceleration = new Vector2d();
      }

      /// <summary>
      /// Creates a new position.
      /// </summary>
      public Position(Double bottomCenterX, Double bottomCenterY, Double width, Double height)
      {
         m_bottomCenter = new Vector2d(bottomCenterX, bottomCenterY);
         m_velocity = new Vector2d();
         m_acceleration = new Vector2d();
         m_width = width;
         m_height = height;
      }
      /// <summary>
      /// Creates a new position by copying an existing one.
      /// </summary>
      public Position(Position otherPosition)
      {
         m_bottomCenter = otherPosition.m_bottomCenter;
         m_width = otherPosition.m_width;
         m_height = otherPosition.m_height;
         m_velocity = otherPosition.m_velocity;
         m_acceleration = otherPosition.m_acceleration;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Convenience method for determining whether this position is facing in one of the cardinal directions.
      /// Not used in any mathematical operations; just for convenience.
      /// </summary>
      public CardinalDirection FacingDirection { get; set; }

      /// <summary>
      /// The center x coordinate.
      /// </summary>
      public Double CenterX
      {
         get { return m_bottomCenter.ComponentX; }
         set { BottomCenterX = value; }
      }

      /// <summary>
      /// Center y coordinate.
      /// </summary>
      public Double CenterY
      {
         get { return m_bottomCenter.ComponentY - m_height / 2; }
         set { BottomCenterY = value + m_height / 2; }
      }

      /// <summary>
      /// The upper left x coordinate.
      /// </summary>
      public Double UpperLeftX
      {
         get { return m_bottomCenter.ComponentX - m_width / 2; }
         set { m_bottomCenter.SetComponentX(value + m_width / 2); }
      }

      /// <summary>
      /// Upper left Y coordinate.
      /// </summary>
      public Double UpperLeftY
      {
         get { return m_bottomCenter.ComponentY - m_height; }
         set { m_bottomCenter.SetComponentY(value + m_height); }
      }

      /// <summary>
      /// The upper right x coordinate.
      /// </summary>
      public Double UpperRightX
      {
         get { return m_bottomCenter.ComponentX + m_width / 2; }
         set { m_bottomCenter.SetComponentX(value - m_width / 2); }
      }

      /// <summary>
      /// Upper right Y coordinate.
      /// </summary>
      public Double UpperRightY
      {
         get { return m_bottomCenter.ComponentY - m_height; }
         set { m_bottomCenter.SetComponentY(value + m_height); }
      }
      /// <summary>
      /// Bottom center x coordinate.
      /// </summary>
      public Double BottomCenterX
      {
         get { return m_bottomCenter.ComponentX; }
         set { m_bottomCenter.SetComponentX(value); }
      }

      /// <summary>
      /// Bottom center y coordinate
      /// </summary>
      public Double BottomCenterY
      {
         get { return m_bottomCenter.ComponentY; }
         set { m_bottomCenter.SetComponentY(value); }
      }

      /// <summary>
      /// The width of this position rectangle.
      /// </summary>
      public Double Width
      {
         get { return m_width; }
         set { m_width = value; }
      }

      /// <summary>
      /// height of this position rectangle.
      /// </summary>
      public Double Height
      {
         get { return m_height; }
         set { m_height = value; }
      }

      /// <summary>
      /// The aspect ratio of this position.
      /// </summary>
      public Double WidthOverHeightRatio
      {
         get { return m_width / m_height; }
      }

      /// <summary>
      /// Velocity vector.
      /// </summary>
      public Vector2d Velocity
      {
         get { return m_velocity; }
         set { m_velocity = value; }
      }

      /// <summary>
      /// Acceleration vector
      /// </summary>
      public Vector2d Acceleration
      {
         get { return m_acceleration; }
         set { m_acceleration = value; }
      }

      /// <summary>
      /// A RectangleF Version of this Position.
      /// </summary>
      public RectangleF RectF
      {
         get
         {
            RectangleF rf = new RectangleF((Single)UpperLeftX, (Single)UpperLeftY, (Single)Width, (Single)Height);
            return rf;
         }
      }

      #endregion

      #region Public Api

      #region CardinalDirections

      /// <summary>
      /// Returns whether this position should go east, west, or nowhere to reach another position.
      /// For example, if this function returns east, the other position is east of this position.
      /// </summary>
      public CardinalDirection GetEastWestDirectionToTarget(Position otherP)
      {
         CardinalDirection d = CardinalDirection.None;
         if (BottomCenterX < otherP.BottomCenterX)
         {
            d = CardinalDirection.East;
         }
         else
         {
            if (BottomCenterX > otherP.BottomCenterX)
            {
               d = CardinalDirection.West;
            }
         }
         return d;
      }

      /// <summary>
      /// Given a velocity and a cardinal direction, updates the appropriate elements of the velocity.
      /// </summary>
      public void UpdateVelocityByDirection(CardinalDirection d, Double velocity)
      {
         Double x = 0, y = 0;
         switch (d)
         {
            case CardinalDirection.East: x = velocity; break;
            case CardinalDirection.West: x = -velocity; break;
            case CardinalDirection.North: y = -velocity; break;
            case CardinalDirection.South: y = velocity; break;
         }
         Velocity.SetComponents(x, y);
      }

      #endregion

      #region Mathematical Queries

      /// <summary>
      /// Returns whether this position is between the two supplied x positions, inclusive.
      /// (which can be any order; this function will figure out which is lesser).
      /// </summary>
      public Boolean IsBetweenX(Double x1, Double x2)
      {
         Double minX = Math.Min(x1, x2);
         Double maxX = Math.Max(x1, x2);
         Boolean meetsCondition = (m_bottomCenter.ComponentX >= minX && m_bottomCenter.ComponentX <= maxX);
         return meetsCondition;
      }

      /// <summary>
      /// Returns whether this rectangle contains this point.
      /// </summary>
      public Boolean ContainsPoint(Double x, Double y)
      {
         Boolean containsPoint = false;
         if (x >= UpperLeftX && x <= UpperLeftX + Width)
         {
            if (y >= UpperLeftY && y <= BottomCenterY)
            {
               containsPoint = true;
            }
         }
         return containsPoint;
      }

      #endregion

      #region Deriving New Positions

      /// <summary>
      /// Creates a new position, based on this one, after adding another position to it.
      /// Only spatial coordinates are considered; not size or velocity.
      /// </summary>
      public Position AddPosition(Position otherPosition)
      {
         Position pResult = new Position(this);
         pResult.m_bottomCenter += otherPosition.m_bottomCenter;
         return pResult;
      }

      /// <summary>
      /// Creates a new position, based on this one, after adding the supplied positions upper left x/y to its BottomCenter X/Y.
      /// </summary>
      public Position AddPositionUpperLeftToBottomCenter(Position otherPosition)
      {
         Position pResult = new Position(this);
         Double updatedBottomCenterX = pResult.m_bottomCenter.ComponentX + otherPosition.UpperLeftX;
         Double updatedBottomCenterY = pResult.m_bottomCenter.ComponentY + otherPosition.UpperLeftY;
         pResult.m_bottomCenter = new Vector2d(updatedBottomCenterX, updatedBottomCenterY);
         return pResult;
      }


      /// <summary>
      /// Creates a new position, based on this one, after adding the supplied positions upper left x/y to its upper left x/y.
      /// </summary>
      public Position AddPositionUpperLeft(Position otherPosition)
      {
         Position pResult = new Position(this);
         Double updatedBottomCenterX = (pResult.UpperLeftX + otherPosition.UpperLeftX) + (m_width / 2);
         Double updatedBottomCenterY = pResult.UpperLeftY + otherPosition.UpperLeftY + (m_height);
         pResult.m_bottomCenter = new Vector2d(updatedBottomCenterX, updatedBottomCenterY);
         return pResult;
      }

      #endregion

      #region Region / Mouse / Screen

      /// <summary>
      /// Given a mouse display position within a display screen, returns a new position rectangle such that the rectangle
      /// is optimally positioned given the mouse's position.  In other words, if the mouse is in the upper left of the screen,
      /// a position will be returned that with an upper left coordinate to the lower right of the mouse; if the mouse
      /// is in the upper right of the screen, the rectangle will be to the lower left of the mouse.
      /// </summary>
      public static Position GetDisplayPositionBasedOnMousePosition(Double mouseX, Double mouseY, Double posWidth, Double posHeight,
         Int32 displayWidth, Int32 displayHeight)
      {
         Double posUpperLeftX, posUpperLeftY;
         if (mouseX >= 0 && mouseX <= displayWidth / 2)
         {
            if (mouseY >= 0 && mouseY <= displayHeight / 2)
            {
               // mouse is in upper left
               posUpperLeftX = mouseX;
               posUpperLeftY = mouseY;
            }
            else
            {
               // mouse is in lower left
               posUpperLeftX = mouseX;
               posUpperLeftY = mouseY - posHeight;
            }
         }
         else
         {
            if (mouseY >= 0 && mouseY <= displayHeight / 2)
            {
               // mouse is in upper right
               posUpperLeftX = mouseX - posWidth;
               posUpperLeftY = mouseY;
            }
            else
            {
               // mouse is in lower right
               posUpperLeftX = mouseX - posWidth;
               posUpperLeftY = mouseY - posHeight;
            }
         }

         Position p = new Position(0, 0, posWidth, posHeight);
         p.UpperLeftX = posUpperLeftX;
         p.UpperLeftY = posUpperLeftY;
         return p;
      }

      #endregion

      #region String Processing

      /// <summary>
      /// String representation of this position.
      /// </summary>
      public override String ToString()
      {
         String rep = String.Format("(bX:{0:0.0},bY:{1:0.0},W:{2:0.0},H:{3:0.0})", m_bottomCenter.ComponentX, m_bottomCenter.ComponentY, m_width, m_height);
         return rep;
      }

      #endregion

      #region Transformation

      /// <summary>
      /// Grows or shrinks a Position by some percentage.
      /// </summary>
      public void TransformByPercent(Double percent)
      {
         Double newWidth = Width * percent;
         Double newHeight = Height * percent;
         Double heightDiffHalf = (Height - newHeight) / 2;
         if (newWidth < 0) newWidth = 0;
         if (newHeight < 0) newHeight = 0;
         Width = newWidth;
         Height = newHeight;
         // After changing the width and height, the Bottom Centers of both positions are equal.
         // Need to shift the bottom center Y up
         BottomCenterY -= heightDiffHalf;
      }

      /// <summary>
      /// Returns a new position that either adds or subtracts a pixel border
      /// </summary>
      public void TransformByPixels(Double pixelExtension)
      {
         Double newWidth = Width + pixelExtension * 2;
         Double newHeight = Height + pixelExtension * 2;
         if (newWidth < 0) newWidth = 0;
         if (newHeight < 0) newHeight = 0;
         Width = newWidth;
         Height = newHeight;
         // After changing the width and height, the Bottom Centers of both positions are equal.
         // Need to shift the bottom center Y up
         BottomCenterY += pixelExtension;
      }

      #endregion

      #region Velocity

      /// <summary>
      /// Causes this position to update its velocity due to acceleration, and position due to velocity.
      /// </summary>
      public void UpdatePositionAndSpeed()
      {
         m_velocity = m_velocity + m_acceleration;
         m_bottomCenter += m_velocity;
      }

      #endregion

      #endregion
   }
}
