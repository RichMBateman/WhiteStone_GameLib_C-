using System;
using System.Drawing;

namespace WhiteStone.GameLib.Sprites
{
   /// <summary>
   /// Describes a single frame of animation.
   /// </summary>
   public class AnimationFrame
   {
      /// <summary>
      /// Describes the X,Y position on a sprite sheet that represents this frame of animation.
      /// </summary>
      public Point Frame;
      /// <summary>
      /// The number of ticks to wait before advancing.
      /// </summary>
      public Int32 FrameDelay;
   }
}
