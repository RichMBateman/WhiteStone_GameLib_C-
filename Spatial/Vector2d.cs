using System;

namespace WhiteStone.GameLib.Spatial
{
   /// <summary>
   /// Represents a vector in 2d space.
   /// </summary>
   public class Vector2d
   {
      #region Private Members

      private Double m_magnitude;
      private Double m_angleInRadians;
      private Double m_componentX;
      private Double m_componentY;

      #endregion

      #region Constructors

      public Vector2d()
      {

      }

      public Vector2d(Double componentX, Double componentY)
      {
         SetComponents(componentX, componentY);
      }

      #endregion

      #region Public Properties

      public Double Magnitude { get { return m_magnitude; } }
      public Double AngleInRadians { get { return m_angleInRadians; } }
      public Double ComponentX { get { return m_componentX; } }
      public Double ComponentY { get { return m_componentY; } }

      #endregion

      #region Public Static Methods

      /// <summary>
      /// Adds two vectors, creating a new one.
      /// </summary>
      public static Vector2d operator +(Vector2d a, Vector2d b)
      {
         Vector2d r = new Vector2d();
         Double combinedX = a.ComponentX + b.ComponentX;
         Double combinedY = a.ComponentY + b.ComponentY;
         r.SetComponents(combinedX, combinedY);
         return r;
      }

      #endregion

      #region Public Methods

      #region Setters

      /// <summary>
      /// Sets the vector's components to zero.
      /// </summary>
      public void Clear()
      {
         SetComponents(0, 0);
      }

      /// <summary>
      /// Sets just the x component of the vector.
      /// </summary>
      public void SetComponentX(Double x)
      {
         SetComponents(x, ComponentY);
      }

      /// <summary>
      /// Sets just the y component of the vector.
      /// </summary>
      public void SetComponentY(Double y)
      {
         SetComponents(ComponentX, y);
      }

      /// <summary>
      /// Sets the X and Y components of this vector.
      /// </summary>
      public void SetComponents(Double x, Double y)
      {
         m_componentX = x;
         m_componentY = y;
         m_magnitude = Math2d.ComputeVectorMagnitude(x, y);
         m_angleInRadians = Math2d.ComputeVectorAngleR(x, y);
      }

      /// <summary>
      /// Sets the magnitude and angle of this vector.
      /// </summary>
      public void SetMagAndAngleR(Double mag, Double angleInRadians)
      {
         m_magnitude = mag;
         m_angleInRadians = angleInRadians;
         Math2d.ComputeVectorComponents(mag, angleInRadians, out m_componentX, out m_componentY);
      }

      #endregion

      #region Strings

      /// <summary>
      /// String representation of this vector.
      /// </summary>
      public override String ToString()
      {
         String vector = String.Format("{0:0.00},{1:0.00}", m_componentX, m_componentY);
         return vector;
      }

      #endregion

      #endregion
   }
}
