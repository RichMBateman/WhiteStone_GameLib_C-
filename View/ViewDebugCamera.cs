using System;
using System.Drawing;
using WhiteStone.GameLib.Drawing;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.Spatial;

namespace WhiteStone.GameLib.View
{
   /// <summary>
   /// Provides a way to debug the camera and help debug problems with displaying game elements appropriately.
   /// </summary>
   public static class ViewDebugCamera
   {
      #region Public Methods

      public static void Draw<T>(T model, Painter<T> painter, Graphics g, 
         String fontName, Int32 fontSize) where T : BaseGameModel
      {
         int CameraInfoWidth = fontSize * 10;
         int CameraInfoHeight = fontSize * 3;
         int currentLineNumber = 0;
         double separationBetweenLines = fontSize / 3.0;

         Point currentMousePos = model.CurrentDisplayMousePosition;
         Font fontNormal = painter.CreateFontForDisplay(fontName, fontSize, model.Camera.UnzoomedScale);

         Double pWidth = CameraInfoWidth * model.Camera.UnzoomedScale;
         Double pHeight = CameraInfoHeight * model.Camera.UnzoomedScale;
         Position p = Position.GetDisplayPositionBasedOnMousePosition(currentMousePos.X, currentMousePos.Y,
             pWidth, pHeight, model.Camera.CurrentFullDisplayWidth, model.Camera.CurrentFullDisplayHeight);
         Drawing2D.DrawRect(g, Brushes.White, p);

         Position pInner = new Position(p);
         pInner.TransformByPixels(-2 * model.Camera.UnzoomedScale);
         Drawing2D.DrawRect(g, Brushes.Black, pInner);

         String text = "Mouse DispPos (" + currentMousePos.X + "," + currentMousePos.Y + ")";
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)pInner.UpperLeftY);
         currentLineNumber++;


         Point logPosMouse = model.Camera.MapDisplayToLogical(currentMousePos, true);
         text = "Mouse LogPos (ignore Camera) = (" + logPosMouse.X + "," + logPosMouse.Y + ")";
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         logPosMouse = model.Camera.MapDisplayToLogical(currentMousePos, false);
         text = "Mouse LogPos (use Camera) = (" + logPosMouse.X + "," + logPosMouse.Y + ")";
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX,
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         text = String.Format("Cam Scale =  {0:0.00}", model.Camera.CurrentScale);
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         text = String.Format("Cam Zoom =  {0:0.00}", model.Camera.ZoomFactorLogical);
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         text = String.Format("Cam Disp Full W/H = W{0},H{1}", model.Camera.CurrentFullDisplayWidth, model.Camera.CurrentFullDisplayHeight);
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         text = "C LogPos: " + model.Camera.LogicalRect;
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         text = "C Display Target: " + model.Camera.DisplayTarget;
         g.DrawString(text, fontNormal, Brushes.White, (Single)pInner.UpperLeftX, 
            (Single)(pInner.UpperLeftY + (currentLineNumber * separationBetweenLines * model.Camera.UnzoomedScale)));
         currentLineNumber++;

         fontNormal.Dispose();
      }

      #endregion
   }
}
