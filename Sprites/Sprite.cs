using WhiteStone.GameLib.Spatial;
using System;
using System.Drawing;

namespace WhiteStone.GameLib.Sprites
{
   /// <summary>
   /// An instance of a particular sprite sheet.  Knows its current animation, the current frame, and when to update.
   /// </summary>
   public class Sprite
   {
      #region Private Members

      private Boolean m_animateOnce = false;
      private Boolean m_continueAnimating = true;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new sprite sheet.
      /// </summary>
      /// <param name="sheet"></param>
      public Sprite(SpriteSheet sheet)
      {
         Sheet = sheet;
         CurrentAnimationKey = SpriteConstants.DefaultAnimationKey;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// The spreadsheet this sprite is based on.
      /// </summary>
      public SpriteSheet Sheet { get; set; }

      /// <summary>
      /// The current animation key.
      /// </summary>
      public String CurrentAnimationKey { get; set; }

      /// <summary>
      /// What frame of the current animation are we on.
      /// </summary>
      public Int32 CurrentFrame { get; set; }

      /// <summary>
      /// How many ticks before we should advance to the next frame.
      /// </summary>
      public Int32 AnimationDelayCounter { get; set; }

      #endregion

      #region Public Methods

      /// <summary>
      /// Draws this sprite at the given position.  Assumes this position has been mapped from a logical to a display position, already.
      /// Uses the BottomCenterX/Y and width and height of the position.
      /// </summary>
      public void Draw(Graphics g, Double scaleFactor, Position position)
      {
         Draw(g, scaleFactor, (Single)position.BottomCenterX, (Single)position.BottomCenterY, (Single)position.Width, (Single)position.Height);
      }

      /// <summary>
      /// Draws this sprite sheet at the given coordinates.
      /// </summary>
      public void Draw(Graphics g, Double scaleFactor, Single dx, Single dy, Single dw, Single dh)
      {
         Sheet.DrawFrame(g, CurrentAnimationKey, CurrentFrame, dx, dy, dw, dh);
      }

      /// <summary>
      /// Advances the animation by one frame tick.
      /// </summary>
      public void Tick()
      {
         if (m_continueAnimating)
         {
            AnimationDelayCounter--;
            if (AnimationDelayCounter < 0)
            {
               CurrentFrame++;
               Animation animation = Sheet.Animations[CurrentAnimationKey];
               if (animation != null)
               {
                  if (this.CurrentFrame >= animation.Frames.Count)
                  {
                     if (!m_animateOnce)
                     {
                        CurrentFrame = 0;
                     }
                     else
                     {
                        CurrentFrame = animation.Frames.Count - 1;
                        m_continueAnimating = false;
                     }
                  }
                  AnimationDelayCounter = animation.Frames[this.CurrentFrame].FrameDelay;
               }
               else
               {
                  throw new Exception(String.Format("Unknown animation type: {0}", CurrentAnimationKey));
               }
            }
         }
      }

      /// <summary>
      /// Specifies what animation to use.  If the same as current animation, nothing will change.
      /// </summary>
      public void SetAnimation(String animationKey, Boolean animateOnce)
      {
         if (CurrentAnimationKey != animationKey)
         {
            var animation = Sheet.Animations[animationKey];

            m_continueAnimating = true;
            m_animateOnce = animateOnce;
            CurrentAnimationKey = animationKey;
            this.AnimationDelayCounter = animation.Frames[0].FrameDelay;

            Reset();
         }
      }

      /// <summary>
      /// Resets the current animation.
      /// </summary>
      public void Reset()
      {
         CurrentFrame = 0;
      }

      #endregion
   }
}
