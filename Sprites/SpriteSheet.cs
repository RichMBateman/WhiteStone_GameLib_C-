using System;
using System.Collections.Generic;
using System.Drawing;

namespace WhiteStone.GameLib.Sprites
{
   /// <summary>
   /// Represents an image file that is a sprite sheet.  Knows the number of frames across and down, the border width,
   /// and the size of each frame.  Has a key to uniquely identify it.  The assumption is that every frame on the spritesheet is the same size.
   /// </summary>
   public class SpriteSheet
   {
      #region Private Members

      private Dictionary<String, Animation> m_animations;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new spritesheet.
      /// </summary>
      public SpriteSheet()
      {

      }

      #endregion

      #region Public Properties

      /// <summary>
      /// A unique key to differentiate this sprite sheet from others.
      /// </summary>
      public String Key { get; set; }

      /// <summary>
      /// A label that identifies the category to which this spritesheet belongs.  Used to group similar sprite sheets together.
      /// </summary>
      public String Category { get; set; }

      /// <summary>
      /// The actual image data, loaded into a bitmap.
      /// </summary>
      public Bitmap ImageData { get; set; }

      /// <summary>
      /// The number of frames going from left to right.
      /// </summary>
      public Int32 NumFramesX { get; set; }

      /// <summary>
      /// The number of frames going from top to bottom.
      /// </summary>
      public Int32 NumFramesY { get; set; }

      /// <summary>
      /// The size of the border separating frames.
      /// </summary>
      public Int32 BorderWidth { get; set; }

      /// <summary>
      /// The width of each frame.
      /// </summary>
      public Int32 FrameWidth { get; set; }

      /// <summary>
      /// The height of each frame.
      /// </summary>
      public Int32 FrameHeight { get; set; }

      /// <summary>
      /// The animations associated with this sprite sheet.
      /// </summary>
      public Dictionary<String, Animation> Animations { get { return m_animations; } set { m_animations = value; } }

      #endregion

      #region Public Api

      /// <summary>
      /// Simply draws a frame of the spritesheet.  The destination x/y coordinates refer to base position, not upper left.
      /// </summary>
      public void DrawFrame(Graphics g, String animationKey, Int32 animationIndex, Single dx, Single dy, Single dw, Single dh)
      {
         Animation animation = m_animations[animationKey];
         AnimationFrame sourceFrame = animation.Frames[animationIndex];
         Int32 sourceX = (sourceFrame.Frame.X * (FrameWidth + BorderWidth)) + BorderWidth;
         Int32 sourceY = (sourceFrame.Frame.Y * (FrameHeight + BorderWidth)) + BorderWidth;
         RectangleF src = new RectangleF(sourceX, sourceY, FrameWidth, FrameHeight);
         Single destX = dx - dw / 2;
         Single destY = dy - dh;
         RectangleF dst = new RectangleF(destX, destY, dw, dh);
         g.DrawImage(ImageData, dst, src, GraphicsUnit.Pixel);
      }

      #endregion
   }
}
