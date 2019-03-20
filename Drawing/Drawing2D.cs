using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WhiteStone.GameLib.Spatial;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.Drawing
{
   /// <summary>
   /// Helper class that wraps some drawing methods.
   /// </summary>
   public static class Drawing2D
   {
      #region Public Methods

      #region Rectangles

      /// <summary>
      /// Draws a solid/filled rectangle.  Maps this rectangle to the display window based on the camera.
      /// </summary>
      public static void DrawRect(BaseGameModel model, Graphics g, Brush b, Position rect)
      {
         Position rectAdjusted = model.Camera.MapLogicalToDisplay(rect, true);
         Drawing2D.DrawRect(g, b, rectAdjusted);
      }

      /// <summary>
      /// Draws a solid/filled rectangle.
      /// </summary>
      /// <param name="g">Graphics object</param>
      /// <param name="b">Brush for color</param>
      /// <param name="rect">The position to draw.</param>
      public static void DrawRect(Graphics g, Brush b, Position rect)
      {
         g.FillRectangle(b, (float)rect.UpperLeftX, (float)rect.UpperLeftY, (float)rect.Width, (float)rect.Height);
      }


      /// <summary>
      /// Draws a rectangle with a border.  Basically draws a larger rectangle with the border color, and then draws a small
      /// rectangle inside with the fill color.
      /// Returns the display position for the inner rectangle.
      /// If the fill color is semi-transparent, realize that due to the implementation of this method, the border color will appear through
      /// the fill color.
      /// </summary>
      public static void FillRectWithBorder(BaseGameModel model, Graphics g, Position rectDisplay, Brush borderColor, Brush fillColor,
         Double borderSizeLogical, Boolean ignoreCameraPos, out Position rectInnerDisplay)
      {
         Position p = rectDisplay;
         Drawing2D.DrawRect(g, borderColor, p);

         rectInnerDisplay = new Position(p);
         Double scale = model.Camera.GetScale(ignoreCameraPos);
         rectInnerDisplay.TransformByPixels(-borderSizeLogical * scale);
         Drawing2D.DrawRect(g, fillColor, rectInnerDisplay);
      }


      /// <summary>
      /// Draws a border for a rectangle.  Remember: Pens have a width.
      /// </summary>
      public static void DrawRectBorder(Graphics g, Pen p, Position displayPos)
      {
         g.DrawRectangle(p, (Single)displayPos.UpperLeftX, (Single)displayPos.UpperLeftY,
            (Single)displayPos.Width, (Single)displayPos.Height);
      }

      /// <summary>
      /// Draws a border for a rectangle.  Remember: Pens have a width.
      /// </summary>
      public static void DrawRectBorder(BaseGameModel m, Graphics g, Pen p, Position logicalPos, Boolean ignoreCameraPos)
      {
         Position displayPos = m.Camera.MapLogicalToDisplay(logicalPos, ignoreCameraPos);
         DrawRectBorder(g, p, displayPos);
      }

      public static void FillCircle(BaseGameModel model, Graphics g, Brush b, Single xCenterLog, Single yCenterLog, Single radiusLog, Boolean ignoreCameraPosition)
      {
         //PointF logPoint = new PointF(xCenterLog, yCenterLog);
         //PointF dispPoint = model.Camera.MapLogicalToDisplay(logPoint, ignoreCameraPosition);
         //Double scale = model.Camera.GetScale(ignoreCameraPosition);
         //Single radiusDisp = (Single)(radiusLog * scale);
         //g.FillEllipse(b, dispPoint.X - radiusDisp, dispPoint.Y - radiusDisp, radiusDisp * 2, radiusDisp * 2);
      }

      public static void DrawCircle(Graphics g, Pen p, Single x, Single y, Single radius)
      {
         g.DrawEllipse(p, x - radius, y - radius, radius * 2, radius * 2);
      }

      #endregion

      #endregion
   }
}
