using System;
using WhiteStone.GameLib.Pathing;

namespace WhiteStone.GameLib.Spatial
{
   /// <summary>
   /// Static library of mathematical functions.
   /// </summary>
   public static class Math2d
   {
      #region Circles

      /// <summary>
      /// Given a circle's origin, radius, and some angle (in DEGREES), computes the x and y positions on the circle.
      /// http://stackoverflow.com/questions/14829621/formula-to-find-points-on-the-circumference-of-a-circle-given-the-center-of-the
      /// </summary>
      public static void ComputeCircumferencePointD(Double centerX, Double centerY, Double radius, Double angleInDegrees, out Double x, out Double y)
      {
         Double radians = ConvertDegreesToRadians(angleInDegrees);
         ComputeCircumferencePointR(centerX, centerY, radius, radians, out x, out y);
      }

      /// <summary>
      /// Given a circle's origin, radius, and some angle (in RADIANS), computes the x and y positions on the circle.
      /// http://stackoverflow.com/questions/14829621/formula-to-find-points-on-the-circumference-of-a-circle-given-the-center-of-the
      /// </summary>
      public static void ComputeCircumferencePointR(Double centerX, Double centerY, Double radius, Double angleInRadians, out Double x, out Double y)
      {
         x = radius * Math.Cos(angleInRadians) + centerX;
         y = radius * Math.Sin(angleInRadians) + centerY;
      }

      /// <summary>
      /// Converts Degrees To Radians
      /// http://www.rapidtables.com/convert/number/degrees-to-radians.htm#how
      /// </summary>
      public static Double ConvertDegreesToRadians(Double degrees)
      {
         Double radians = degrees * (Math.PI / 180.0);
         return radians;
      }

      /// <summary>
      /// Converts Radians to Degrees
      /// </summary>
      public static Double ConvertRadiansToDegrees(Double radians)
      {
         Double degrees = (radians * 180.0) / Math.PI;
         return degrees;
      }

      #endregion

      #region Distance

      /// <summary>
      /// Calculates the distance between the bottom center coordinate of two positions.
      /// </summary>
      public static Double GetDistanceBetweenTwoPositions(Position a, Position b)
      {
         Double distance = GetDistanceBetweenTwoCoordinatePairs(a.BottomCenterX, a.BottomCenterY, b.BottomCenterX, b.BottomCenterY);
         return distance;
      }

      public static Double GetDistanceBetweenTwoNodes(IAStarNode a, IAStarNode b)
      {
         Double distance = GetDistanceBetweenTwoCoordinatePairs(a.X, a.Y, b.X, b.Y);
         return distance;
      }

      public static Double GetDistanceBetweenTwoCoordinatePairs(Double x1, Double y1, Double x2, Double y2)
      {
         Double distance = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
         return distance;
      }

      #endregion

      #region Number Ranges

      /// <summary>
      /// Returns whether a test number is between two values, inclusive.  val1 can be either the smaller or larger number.
      /// </summary>
      public static Boolean IsNumberWithinRange(Double test, Double val1, Double val2)
      {
         Double min = Math.Min(val1, val2);
         Double max = Math.Max(val1, val2);
         return (test >= min && test <= max);
      }

      #endregion

      #region Quadratic Formula

      /// <summary>
      /// Runs the quadratic formula for constants a, b, c.  ax^2 + bx + c
      /// Returns whether it is solvable.
      /// </summary>
      public static Boolean QuadraticFormula(Double a, Double b, Double c, out Double x1, out Double x2)
      {
         Double part1 = (Math.Pow(b, 2) - (4 * a * c));
         Boolean solvable = (part1 >= 0);
         if (solvable)
         {
            part1 = Math.Sqrt(part1);
            x1 = (-b + part1) / (2 * a);
            x2 = (-b - part1) / (2 * a);
         }
         else
         {
            x1 = 0;
            x2 = 0;
         }
         return solvable;
      }

      #endregion

      #region Rectangles

      /// <summary>
      /// Returns whether two positions intersect
      /// </summary>
      public static Boolean RectanglesIntersect(Position r1, Position r2)
      {
         Boolean intersects = !(
            r2.UpperLeftX > r1.UpperRightX ||
            r2.UpperRightX < r1.UpperLeftX ||
            r2.UpperRightY > r1.BottomCenterY ||
            r2.BottomCenterY < r1.UpperRightY
         );
         return intersects;
      }

      /// <summary>
      /// Given a desired aspect ratio (like 4:3) and a source rectangle,
      /// determines the largest rectangle that will fit within the desired ratio, centered either vertically
      /// or horizontally.  (The largest rectangle will always be touching at least two sides of the source rectangle).
      /// </summary>
      public static void GetLargestRectangleThatFits(Double desiredWidthOverHeightRatio,
         Position sourceRect, out Position targetRect)
      {
         // Assume we'll draw destination display rectangle starting at 0,0 
         // (meaning the rectangle matches our ideal aspect ratio)
         targetRect = new Position(sourceRect.Width / 2, sourceRect.Height, sourceRect.Width, sourceRect.Height);

         // What's the current ratio of source rectangle
         Double sourceRatio = sourceRect.Width / sourceRect.Height;

         // i.e, the source rectangle is wider than we would like
         if (sourceRatio > desiredWidthOverHeightRatio)
         {
            // Adjust the width and the starting x position
            targetRect.Width = sourceRect.Height * desiredWidthOverHeightRatio;
            targetRect.UpperLeftX = ((sourceRect.Width) - targetRect.Width) / 2;
         }
         // i.e., the source rectangle is taller than we would like
         else if (sourceRatio < desiredWidthOverHeightRatio)
         {
            // Adjust the height and the starting y position
            targetRect.Height = sourceRect.Width / desiredWidthOverHeightRatio;
            targetRect.UpperLeftY = ((sourceRect.Height) - targetRect.Height) / 2;
         }
      }

      #endregion

      #region Vectors

      /// <summary>
      /// Computes the magnitude, or length, of a vector.
      /// http://www.sparknotes.com/testprep/books/sat2/physics/chapter4section5.rhtml
      /// </summary>
      public static Double ComputeVectorMagnitude(Double xComponent, Double yComponent)
      {
         Double mag = Math.Sqrt(Math.Pow(xComponent, 2) + Math.Pow(yComponent, 2));
         return mag;
      }

      /// <summary>
      /// Computes the vector components.
      /// http://www.dummies.com/how-to/content/how-to-find-vector-components.html
      /// </summary>
      public static void ComputeVectorComponents(Double mag, Double angleInRadians, out Double xComp, out Double yComp)
      {
         xComp = mag * Math.Cos(angleInRadians);
         yComp = mag * Math.Sin(angleInRadians);
      }

      public static Double ComputeVectorAngleR(Double x1, Double y1, Double x2, Double y2)
      {
         Double x = x2 - x1;
         Double y = y2 - y1;
         Double angle = ComputeVectorAngleR(x, y);
         return angle;
      }

      /// <summary>
      /// Returns the angle of a vector, in radians, from a horizontal right-ward facing vector.
      /// Always returns a positive result, from 0 to 2PI
      /// Sample results: 
      /// (1,0)  = 0     radians
      /// (0,1)  = pi/2  radians
      /// (-1,0) = pi    radians
      /// (0,-1) = 3pi/2 radians
      /// </summary>
      public static Double ComputeVectorAngleR(Double xComponent, Double yComponent)
      {
         Double radians = Math.Atan2(yComponent, xComponent);
         if (radians < 0)
         {
            radians += 2 * Math.PI;
         }
         return radians;
      }

      #endregion
   }
}
