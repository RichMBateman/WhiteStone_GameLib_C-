/* HOW TO USE THE CAMERA
   ---------------------
   In my 2D game library, displaying graphics can be highly confusing, because there are a lot of different
   coordinate systems to think about.  There's the actual client display within the user's monitor;
   there's the logical world with its own coordinate system, and the camera which shows a portion of that world.
   To make things simpler, consider the following.

   >The upper left coordinate of your game world should ALWAYS be (0,0).
      -Therefore, no part of the logical world will ever have negative coordinates.
   >If your logical world is smaller than the camera, and you want it to be CENTERED on the display,
      set the camera's position so that it's centered on the logical world.
      For example, say your world is 100, 100.  Your camera is larger (100w by 120h).
      Set your camera's bottom center to (50,100)... the display will then automatically be centered.

*/

using System;
using WhiteStone.GameLib.Spatial;
using System.Drawing;

namespace WhiteStone.GameLib.View
{
   /// <summary>
   /// The camera controls what part of our environment we're currently seeing on screen.
   /// Can map from logical coordinates to on-screen coordinates.  Also handles the aspect ratio.
   /// Think of the camera's height and width as a logical size relative to the logical size of the world.
   /// For example, let's say our world is logically 10k by 10k units.  And we only want to show 
   /// the upper right corner.  Our camera size would be 2.5k by 2.5k units, and it's position would
   /// (bottomCenterX = 8750, bottomCenterY = 2500);
   /// </summary>
   public class Camera
   {
      #region Private Members

      private const Double ZoomFactorInitial = 1.0;
      private const Int32 DefaultIdealDisplayWidth = 800;
      private const Int32 DefaultIdealDisplayHeight = 600;

      private Int32 m_idealDisplayWidth;
      private Int32 m_idealDisplayHeight;

      private Int32 m_currentFullDisplayWidth;
      private Int32 m_currentFullDisplayHeight;

      private Double m_unzoomedLogicalWidth;
      private Double m_unzoomedLogicalHeight;

      private Position m_logicalPosition;
      private Position m_displayTargetPosition;

      /// <summary>
      /// The scale of the current logical width of the camera (which includes logical ZoomFactor),
      /// compared to the display size of the client.
      /// </summary>
      private Double m_currentScale;
      private Double m_unzoomedScale;
      private Double m_zoomFactor;

      #endregion

      #region Constructors

      public Camera()
      {
         m_idealDisplayWidth = DefaultIdealDisplayWidth;
         m_idealDisplayHeight = DefaultIdealDisplayHeight;
         m_logicalPosition = new Position();
         m_zoomFactor = ZoomFactorInitial;
      }

      #endregion

      #region Properties


      /// <summary>
      /// Zooms the camera.  This basically adjusts the width and height of the camera, while keeping it centered.
      /// So if the camera is normally 3200W by 2400H, and you set the zoom factor by 2,
      /// you're doubling the size of the camera.  
      /// </summary>
      public Double ZoomFactorLogical
      {
         get { return m_zoomFactor; }
         set
         {
            m_zoomFactor = value;
            Double originalBx = m_logicalPosition.BottomCenterX;
            Double originalBy = m_logicalPosition.BottomCenterY;
            m_logicalPosition.Width = m_unzoomedLogicalWidth * ZoomFactorLogical;
            m_logicalPosition.Height = m_unzoomedLogicalHeight * ZoomFactorLogical;
            m_logicalPosition.BottomCenterX = originalBx * ZoomFactorLogical;
            m_logicalPosition.BottomCenterY = originalBy * ZoomFactorLogical;

            UpdateCurrentScale();
            UpdateUnzoomedScale();
         }
      }

      /// <summary>
      /// Desired width over height ratio.
      /// </summary>
      public Double AspectRatio
      {
         get
         {
            Double aspectRatio = 0;
            if (m_idealDisplayHeight > 0)
            {
               aspectRatio = m_idealDisplayWidth / ((Double)m_idealDisplayHeight);
            }
            return aspectRatio;
         }
      }

      public Int32 CurrentFullDisplayWidth
      {
         get { return m_currentFullDisplayWidth; }
      }

      public Int32 CurrentFullDisplayHeight
      {
         get { return m_currentFullDisplayHeight; }
      }

      public Int32 IdealDisplayWidth
      {
         get { return m_idealDisplayWidth; }
         set { m_idealDisplayWidth = value; }
      }

      public Int32 IdealDisplayHeight
      {
         get { return m_idealDisplayHeight; }
         set { m_idealDisplayHeight = value; }
      }

      /// <summary>
      /// This represents the part of the client area where we are actually targeting for drawing.
      /// </summary>
      public Position DisplayTarget
      {
         get { return m_displayTargetPosition; }
      }

      public Position LogicalRect
      {
         get { return m_logicalPosition; }
      }

      /// <summary>
      /// The current scale represents the ratio of the actual width of the display screen (the size of the 
      /// user's client window) over the logical width of the camera.  For example, the camera may be 3200 pixels
      /// wide, but the user's client window may only be 800 pixels wide.  The current scale in that case would be
      /// 0.25.
      /// </summary>
      public Double CurrentScale
      {
         get { return m_currentScale; }
      }

      public Double UnzoomedScale
      {
         get { return m_unzoomedScale; }
      }


      #endregion

      #region Mappings

      /// <summary>
      /// Maps a logical position to a display position.
      /// Ignoring camera position also ignores camera zoom.
      /// </summary>
      public Position MapLogicalToDisplay(Position logicalObjectPosition, Boolean ignoreCameraPosition = false)
      {
         if (m_logicalPosition.Width == 0 || m_logicalPosition.Height == 0)
         {
            throw new Exception("The logical position of the camera has been set with zero width or height, which is not allowed.");
         }

         Double scale = (ignoreCameraPosition ? m_unzoomedScale : m_currentScale);

         // Algorithm Overview:
         // To map a logical point to a point on the display:
         //  -Determine the scale change from logical to display (like how much bigger/small is the display compared to the logical size?)
         //  -To determine the X&Y coordinates, first subtract the camera's X & Y, then add Camera Width/2, Camera Height to the X,Y
         //  -Don't forget to also add the shift caused by centering the target rectangle in the full display rectangle.

         Position targetDisplayRectangle = new Position();
         // Compute the size of the target rectangle
         targetDisplayRectangle.Width = logicalObjectPosition.Width * scale;
         targetDisplayRectangle.Height = logicalObjectPosition.Height * scale;

         Double displayTargetPosUpperLeftX = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftX : 0);
         Double displayTargetPosUpperLeftY = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftY : 0);

         // Compute its base.
         // Notice that all operations between logical positions happen first, then we apply the scale (which brings it into the display realm) and then we add the display shifts.
         if (ignoreCameraPosition)
         {
            targetDisplayRectangle.BottomCenterX = (logicalObjectPosition.BottomCenterX * scale) + displayTargetPosUpperLeftX;
            targetDisplayRectangle.BottomCenterY = (logicalObjectPosition.BottomCenterY * scale) + displayTargetPosUpperLeftY;
         }
         else
         {
            targetDisplayRectangle.BottomCenterX = 
               (
                  (logicalObjectPosition.BottomCenterX - m_logicalPosition.BottomCenterX + m_logicalPosition.Width / 2) 
                  * scale
               ) 
               + displayTargetPosUpperLeftX;
            targetDisplayRectangle.BottomCenterY = 
               (
                  (logicalObjectPosition.BottomCenterY - m_logicalPosition.BottomCenterY + m_logicalPosition.Height) 
                  * scale
               ) 
               + displayTargetPosUpperLeftY;
         }

         return targetDisplayRectangle;
      }

      /// <summary>
      /// Maps a logical point to a display point.
      /// Ignoring camera position ALSO ignores camera zoom.
      /// </summary>
      public PointF MapLogicalToDisplay(PointF pointLogical, Boolean ignoreCameraPosition = false)
      {
         Single xDisp = 0;
         Single yDisp = 0;

         Double displayTargetPosUpperLeftX = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftX : 0);
         Double displayTargetPosUpperLeftY = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftY : 0);

         Double scale = (ignoreCameraPosition ? m_unzoomedScale : m_currentScale);

         // Compute its base.
         // Notice that all operations between logical positions happen first, then we apply the scale (which brings it into the display realm) and then we add the display shifts.
         if (ignoreCameraPosition)
         {
            xDisp = (Single)(pointLogical.X * scale + displayTargetPosUpperLeftX);
            yDisp = (Single)(pointLogical.Y * scale + displayTargetPosUpperLeftY);
         }
         else
         {
            xDisp = (Single)((pointLogical.X - m_logicalPosition.BottomCenterX + m_logicalPosition.Width / 2) * scale + displayTargetPosUpperLeftX);
            yDisp = (Single)((pointLogical.Y - m_logicalPosition.BottomCenterY + m_logicalPosition.Height) * scale + displayTargetPosUpperLeftY);
         }

         PointF displayPoint = new PointF(xDisp, yDisp);
         return displayPoint;
      }

      /// <summary>
      /// Maps a logical polygon to a display polygon.
      /// Ignoring camera position ALSO ignores camera zoom.
      /// </summary>
      public Point[] MapLogicalToDisplay(Point[] polygonLogical, Boolean ignoreCameraPosition = false)
      {
         Point[] displayPolygon = new Point[polygonLogical.Length];

         Double displayTargetPosUpperLeftX = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftX : 0);
         Double displayTargetPosUpperLeftY = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftY : 0);

         Double scale = (ignoreCameraPosition ? m_unzoomedScale : m_currentScale);

         for (Int32 pIndex = 0; pIndex < polygonLogical.Length; pIndex++)
         {
            // Compute its base.
            // Notice that all operations between logical positions happen first, then we apply the scale (which brings it into the display realm) and then we add the display shifts.
            if (ignoreCameraPosition)
            {
               displayPolygon[pIndex] = new Point(
                   (Int32)(polygonLogical[pIndex].X * scale + displayTargetPosUpperLeftX),
                   (Int32)(polygonLogical[pIndex].Y * scale + displayTargetPosUpperLeftY));
            }
            else
            {
               displayPolygon[pIndex] = new Point(
                   (Int32)((polygonLogical[pIndex].X - m_logicalPosition.BottomCenterX + m_logicalPosition.Width / 2) * scale + displayTargetPosUpperLeftX),
                   (Int32)((polygonLogical[pIndex].Y - m_logicalPosition.BottomCenterY + m_logicalPosition.Height) * scale + displayTargetPosUpperLeftY));
            }


         }
         return displayPolygon;
      }

      /// <summary>
      /// Simply shifts the position over for the camera.  Does no scaling.  Useful if you're drawing to a logical bitmap.
      /// </summary>
      public Position MapLogicalToLogicalShift(Position logicalObjectPosition)
      {
         Int32 bottomCenterX = (Int32)(logicalObjectPosition.BottomCenterX + LogicalRect.Width / 2);
         Int32 bottomCenterY = (Int32)(logicalObjectPosition.BottomCenterY + LogicalRect.Height);
         Int32 width = (Int32)logicalObjectPosition.Width;
         Int32 height = (Int32)logicalObjectPosition.Height;
         Position logicalShiftPos = new Position(bottomCenterX, bottomCenterY, width, height);
         return logicalShiftPos;
      }

      /// <summary>
      /// Maps a logical polygon to a logical shift polygon.  (No scaling).
      /// </summary>
      public Point[] MapLogicalToLogicalShift(Point[] polygonLogical)
      {
         Point[] displayPolygon = new Point[polygonLogical.Length];

         Double displayTargetPosUpperLeftX = 0; // (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftX : 0);
         Double displayTargetPosUpperLeftY = 0; // (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftY : 0);

         for (Int32 pIndex = 0; pIndex < polygonLogical.Length; pIndex++)
         {
            displayPolygon[pIndex] =
                new Point(
                    (Int32)((polygonLogical[pIndex].X + m_logicalPosition.Width / 2) + displayTargetPosUpperLeftX),
                    (Int32)((polygonLogical[pIndex].Y + m_logicalPosition.Height) + displayTargetPosUpperLeftY));
         }
         return displayPolygon;
      }

      /// <summary>
      /// Maps a display point to a logical point.  (Optionally ignoring camera position)
      /// Ignoring camera position ALSO ignores camera zoom.
      /// </summary>
      public Point MapDisplayToLogical(Point displayPoint, Boolean ignoreCameraPosition)
      {
         if (m_logicalPosition.Width == 0 || m_logicalPosition.Height == 0)
         {
            throw new Exception("The logical position of the camera has been set with zero width or height, which is not allowed.");
         }

         Int32 logX = 0;
         Int32 logY = 0;

         Double displayTargetPosUpperLeftX = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftX : 0);
         Double displayTargetPosUpperLeftY = (m_displayTargetPosition != null ? m_displayTargetPosition.UpperLeftY : 0);

         Double scale = (ignoreCameraPosition ? m_unzoomedScale : m_currentScale);

         // Compute its base.
         // Notice that all operations between logical positions happen first, then we apply the scale (which brings it into the display realm) and then we add the display shifts.
         if (ignoreCameraPosition)
         {
            logX = (Int32)((displayPoint.X - displayTargetPosUpperLeftX) / scale);
            logY = (Int32)((displayPoint.Y - displayTargetPosUpperLeftY) / scale);
         }
         else
         {
            logX = (Int32)((displayPoint.X - displayTargetPosUpperLeftX) / scale - m_logicalPosition.BottomCenterX);
            logY = (Int32)((displayPoint.Y - displayTargetPosUpperLeftY) / scale - m_logicalPosition.BottomCenterY);
         }

         Point logicalPoint = new Point(logX, logY);
         return logicalPoint;
      }

      #endregion

      #region Text Aligning

      /// <summary>
      /// Given some text we want to draw on the screen with some font, horizontally centers it within a rectangle.
      /// </summary>
      public Position HorizontallyCenterTextWithinRect(Position containerRect, String text, Font f, Graphics g, Boolean ignoreCameraPosition = false)
      {
         Position scaledRect = MapLogicalToDisplay(containerRect, ignoreCameraPosition);
         SizeF textSizeF = g.MeasureString(text, f);
         scaledRect.UpperLeftX = scaledRect.UpperLeftX + (scaledRect.Width - textSizeF.Width) / 2;
         scaledRect.UpperLeftY = scaledRect.UpperLeftY + (scaledRect.Height - textSizeF.Height);
         return scaledRect;
      }

      /// <summary>
      /// Given some text we want to draw on the screen with some font, horizontally and vertically centers it within a rectangle.
      /// </summary>
      public Position FullyCenterTextWithinRect(Position containerRect, String text, Font f, Graphics g, Boolean ignoreCameraPosition = false)
      {
         Position scaledRect = MapLogicalToDisplay(containerRect, ignoreCameraPosition);
         SizeF textSizeF = g.MeasureString(text, f);
         scaledRect.UpperLeftX = scaledRect.UpperLeftX + (scaledRect.Width - textSizeF.Width) / 2;
         scaledRect.UpperLeftY = scaledRect.UpperLeftY + (scaledRect.Height - textSizeF.Height) / 2;
         return scaledRect;
      }

      /// <summary>
      /// Given some text we want to draw on the screen with some font; aligns it to the right of the rectangle.
      /// </summary>
      public Position RightAlignTextWithinRect(Position containerRect, String text, Font f, Graphics g, Boolean ignoreCameraPosition = false)
      {
         Position scaledRect = MapLogicalToDisplay(containerRect, ignoreCameraPosition);
         SizeF textSizeF = g.MeasureString(text, f);
         scaledRect.UpperLeftX = scaledRect.UpperLeftX + scaledRect.Width - textSizeF.Width;
         scaledRect.UpperLeftY = scaledRect.UpperLeftY + (scaledRect.Height - textSizeF.Height);
         return scaledRect;
      }

      /// <summary>
      /// Given some text we want to draw on the screen with some font; aligns it to the bottom left of the rectangle.
      /// </summary>
      public Position BottomLeftAlignTextWithinRect(Position containerRect, String text, Font f, Graphics g, Boolean ignoreCameraPosition = false)
      {
         Position scaledRect = MapLogicalToDisplay(containerRect, ignoreCameraPosition);
         SizeF textSizeF = g.MeasureString(text, f);
         scaledRect.UpperLeftY = scaledRect.UpperLeftY + (scaledRect.Height - textSizeF.Height);
         return scaledRect;
      }

      /// <summary>
      /// Given some text we want to draw on the screen with some font; aligns it to the bottom left of the rectangle.
      /// </summary>
      public Position TopLeftAlignTextWithinRect(Position containerRect, String text, Font f, Graphics g, Boolean ignoreCameraPosition = false)
      {
         Position scaledRect = MapLogicalToDisplay(containerRect, ignoreCameraPosition);
         SizeF textSizeF = g.MeasureString(text, f);
         scaledRect.UpperLeftY = scaledRect.UpperLeftY;
         return scaledRect;
      }

      #endregion

      #region Ticks & Updating

      /// <summary>
      /// Tells the camera the current available space for displaying your game.  The camera will then build the largest rectangle that maintains a certain aspect ratio.
      /// Returns true iff the supplied display sizes are different from what was previously stored.
      /// </summary>
      public Boolean UpdateDisplayInformation(Int32 displayWidth, Int32 displayHeight)
      {
         Boolean displayChanged = false;
         if (m_currentFullDisplayHeight != displayHeight || m_currentFullDisplayWidth != displayWidth)
         {
            displayChanged = true;
            m_currentFullDisplayWidth = displayWidth;
            m_currentFullDisplayHeight = displayHeight;
            Position displayPosition = new Position(displayWidth / 2, displayHeight, displayWidth, displayHeight);

            Math2d.GetLargestRectangleThatFits(AspectRatio, displayPosition, out m_displayTargetPosition);

            // First, determine the scale factor from the logical viewport to the display viewport.
            // So if the display window is actually 8 times the size of the logical window, you'll have to scale everything by 8
            m_currentScale = m_displayTargetPosition.Width / m_logicalPosition.Width;
         }
         return displayChanged;
      }

      /// <summary>
      /// This method needs to be called whenever the logical camera size has changed, or the display's window size has changed.
      /// </summary>
      public void UpdateCurrentScale()
      {
         // First, determine the scale factor from the logical viewport to the display viewport.
         // So if the display window is actually 8 times the size of the logical window, you'll have to scale everything by 8
         if (m_displayTargetPosition != null && m_logicalPosition != null && m_logicalPosition.Width != 0)
         {
            m_currentScale = m_displayTargetPosition.Width / m_logicalPosition.Width;
         }
      }

      #endregion

      #region Zoom

      /// <summary>
      /// Gets the appropriate scale to use, depending on whether you are ignoring the camera's position or not.
      /// </summary>
      public Double GetScale(Boolean ignoreCameraPosition)
      {
         return (ignoreCameraPosition ? UnzoomedScale : CurrentScale);
      }

      public void SetUnzoomedLogicalSize()
      {
         m_unzoomedLogicalHeight = m_logicalPosition.Height;
         m_unzoomedLogicalWidth = m_logicalPosition.Width;
         UpdateUnzoomedScale();
      }

      private void UpdateUnzoomedScale()
      {
         if (m_displayTargetPosition != null)
         {
            m_unzoomedScale = m_displayTargetPosition.Width / m_unzoomedLogicalWidth;
         }
      }

      #endregion
   }
}
