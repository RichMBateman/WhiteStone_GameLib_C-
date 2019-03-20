using System;
using System.Drawing;
using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.Spatial;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace WhiteStone.GameLib.View
{
   #region Delegates

   public delegate void InvalidateDrawingSurfaceHandler();

   #endregion

   /// <summary>
   /// Manages drawing.
   /// </summary>
   public class Painter<T>
      where T : BaseGameModel
   {
      #region Members

      private Color m_backFillColor = Color.Black;
      private Color m_targetFillColor = Color.FromArgb(32, 32, 32);

      private SolidBrush m_brushBackFillColor;
      private SolidBrush m_brushTargetFillColor;

      private readonly PrivateFontCollection m_privateFontCollection = new PrivateFontCollection();
      private readonly Dictionary<String, FontFamily> m_keyToFontFamily = new Dictionary<String, FontFamily>();

      private InvalidateDrawingSurfaceHandler m_invalidateDrawingSurfaceHandler;

      #endregion

      #region Private Properties

      /// <summary>
      /// Brush used to fill the entire screen.
      /// </summary>
      private SolidBrush BrushBackFillColor
      {
         get
         {
            if (m_brushBackFillColor == null)
            {
               m_brushBackFillColor = new SolidBrush(m_backFillColor);
            }
            return m_brushBackFillColor;
         }
      }

      /// <summary>
      /// Brush used to fill the target area of our entire screen, which contains our rectangle at a certain aspect ratio.
      /// </summary>
      private SolidBrush BrushTargetFillColor
      {
         get
         {
            if (m_brushTargetFillColor == null)
            {
               m_brushTargetFillColor = new SolidBrush(m_targetFillColor);
            }
            return m_brushTargetFillColor;
         }
      }

      #endregion

      #region Public Api

      #region Callback Installation

      /// <summary>
      /// Installs a callback that can be used to invalidate the screen.
      /// </summary>
      /// <param name="handler"></param>
      public void InstallInvalidateDrawingSurfaceHandler(InvalidateDrawingSurfaceHandler handler)
      {
         m_invalidateDrawingSurfaceHandler = handler;
      }

      #endregion

      #region Drawing

      /// <summary>
      /// Creates a bitmap using standard settings.
      /// </summary>
      public static Bitmap CreateBitmap(Int32 width, Int32 height)
      {
         // http://www.codeproject.com/Tips/66909/Rendering-fast-with-GDI-What-to-do-and-what-not-to
         Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
         return b;
      }



      /// <summary>
      /// Configures the Graphics object mode.
      /// </summary>
      public static void ConfigureGraphicsObjects(Graphics g)
      {
         // http://www.codeproject.com/Tips/66909/Rendering-fast-with-GDI-What-to-do-and-what-not-to
         // SourceCopy causes problems with transparency.
         g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

         g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

         g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
         //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
         //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

         g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
         //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

         g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
         //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

         g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;

         // Original
         //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
         //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
         //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
      }

      /// <summary>
      /// Clears the whole screen, then, for each view in this game state, draws to the screen.
      /// </summary>
      public void Draw(T model, GameState<T> currentState, Bitmap gameBuffer)
      {
         if (gameBuffer != null)
         {
            using (Graphics g = Graphics.FromImage(gameBuffer))
            {
               //ConfigureGraphicsObjects(g);
               MarkGraphicsSettings(g);
               PaintBackFill(g, model);
               //baseView.Draw(model, this, g, gameBuffer.Width, gameBuffer.Height);
               foreach (var view in currentState.Views)
               {
                  view.Draw(model, this, g);
               }
               PaintObscuringBars(g, model);
               // After we draw the obscuring bars, give the views an opportunity to paint above them.
               // This is useful in cases where we want to display debug information.
               foreach(var view in currentState.Views)
               {
                  view.DrawOnTopMostLayer(model, this, g);
               }
            }

            if (m_invalidateDrawingSurfaceHandler != null)
            {
               m_invalidateDrawingSurfaceHandler();
            }
         }
      }

      #endregion

      #region Fonts

      /// <summary>
      /// Installs a custom font for the application.
      /// </summary>
      public void InstallFontFromFile(String filepath, String fontName)
      {
         m_privateFontCollection.AddFontFile(filepath);
         foreach (FontFamily family in m_privateFontCollection.Families)
         {
            if (family.Name == fontName)
            {
               m_keyToFontFamily.Add(fontName, family);
               break;
            }
         }
      }

      /// <summary>
      /// Returns a custom font family.
      /// </summary>
      public FontFamily GetCustomFontFamily(String fontName)
      {
         return m_keyToFontFamily[fontName];
      }

      /// <summary>
      /// Returns a new font object.
      /// </summary>
      public Font CreateFontForDisplay(String fontName, Int32 baseSize, Double scale)
      {
         Font font = new Font(GetCustomFontFamily(fontName), (Single)(baseSize * scale), FontStyle.Regular, GraphicsUnit.Pixel);
         return font;
      }

      #endregion

      #endregion

      #region Private Methods

      private void MarkGraphicsSettings(Graphics g)
      {
         g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
         g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
         g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
      }

      /// <summary>
      /// When drawing our game, fills the background.  It will be clear where the target rectangle is in the full display rectangle.
      /// </summary>
      private void PaintBackFill(Graphics g, T model)
      {
         g.Clear(m_backFillColor);

         Position targetPos = model.Camera.DisplayTarget;
         g.FillRectangle(BrushTargetFillColor, (Single)targetPos.UpperLeftX, (Single)targetPos.UpperLeftY,
            (Single)targetPos.Width, (Single)targetPos.Height);
      }

      /// <summary>
      /// After we've drawn our game, draw bars to prevent art from the target rectangle from leaking into the full display rectangle.
      /// </summary>
      /// <param name="g"></param>
      /// <param name="model"></param>
      private void PaintObscuringBars(Graphics g, T model)
      {
         if (model.Camera.CurrentFullDisplayWidth > model.Camera.DisplayTarget.Width)
         {
            Double coverageBarWidth = (model.Camera.CurrentFullDisplayWidth - model.Camera.DisplayTarget.Width) / 2;
            g.FillRectangle(BrushBackFillColor, 0, 0, (Single)coverageBarWidth, model.Camera.CurrentFullDisplayHeight);
            g.FillRectangle(BrushBackFillColor, (Single)(model.Camera.DisplayTarget.Width + coverageBarWidth), 0, (Single)coverageBarWidth, model.Camera.CurrentFullDisplayHeight);
         }
         if (model.Camera.CurrentFullDisplayHeight > model.Camera.DisplayTarget.Height)
         {
            Double coverageBarHeight = (model.Camera.CurrentFullDisplayHeight - model.Camera.DisplayTarget.Height) / 2;
            g.FillRectangle(BrushBackFillColor, 0, 0, model.Camera.CurrentFullDisplayWidth, (Single)coverageBarHeight);
            g.FillRectangle(BrushBackFillColor, 0, (Single)(model.Camera.DisplayTarget.Height + coverageBarHeight), model.Camera.CurrentFullDisplayWidth, (Single)coverageBarHeight);
         }
      }

      #endregion
   }
}
