using System;
using System.Collections.Generic;
using System.Drawing;

namespace WhiteStone.GameLib.Sprites
{
   /// <summary>
   /// Represents an animation for a SpriteSheet (or category of similar spritesheets).  Basically an ordered list of points, 
   /// where every point refers to a frame in the sprite sheet.
   /// </summary>
   public class Animation
   {
      #region Public Properties

      /// <summary>
      /// Unique key for this animation.
      /// </summary>
      public String Key { get; set; }

      /// <summary>
      /// Optional category that can be used outside framework to help organize animations.
      /// </summary>
      public String Category { get; set; }

      /// <summary>
      /// The list of points that makes up this animation.
      /// </summary>
      public List<AnimationFrame> Frames { get; set; }

      /// <summary>
      /// Overall delay for this animation.  Before we can proceed to the next frame, "OverallDelay" number of ticks must pass.
      /// </summary>
      public Int32 OverallDelay { get; set; }

      #endregion

      #region Public Api

      /// <summary>
      /// Appends a new frame to this animation.
      /// </summary>
      public void AddFrame(Int32 x, Int32 y, Int32 frameDelay)
      {
         if (Frames == null)
         {
            Frames = new List<AnimationFrame>();
         }
         AnimationFrame frame = new AnimationFrame { Frame = new Point(x, y), FrameDelay = frameDelay };
         Frames.Add(frame);
      }

      #endregion
   }
}
