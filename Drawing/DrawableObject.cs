using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Forms;
using WhiteStone.GameLib.Spatial;
using WhiteStone.GameLib.Sprites;
using System;
using System.Drawing;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.View;

namespace WhiteStone.GameLib.Drawing
{
   /// <summary>
   /// Represents something that can be drawn on screen.  Has a logical and display position, and a sprite.
   /// </summary>
   public class DrawableObject
   {
      #region Private Members

      private readonly BaseGameModel m_model;
      private readonly Camera m_camera;
      private String m_spriteKey;
      private Sprite m_sprite;

      #endregion

      #region Constructor

      public DrawableObject(BaseGameModel m)
      {
         m_model = m;
         m_camera = m.Camera;
         Visible = true;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Whether this object is visible or hidden.  If hidden, will never display; if visible, will only be drawn if it's within camera bounds.
      /// </summary>
      public Boolean Visible { get; set; }
      /// <summary>
      /// The key for this sprite.  Is set when a sprite instance is spawned.
      /// </summary>
      public String SpriteKey { get { return m_spriteKey; } }
      /// <summary>
      /// The actual sprite for this object.
      /// </summary>
      public Sprite Sprite { get { return m_sprite; } }
      /// <summary>
      /// This object's logical position.
      /// </summary>
      public Position PositionLogical { get; set; }
      /// <summary>
      /// (Derived) Where this object should appear on screen.
      /// </summary>
      public Position PositionDisplay
      {
         get
         {
            Position displayPos = m_camera.MapLogicalToDisplay(PositionLogical);
            return displayPos;
         }
      }
      /// <summary>
      /// (Derived) Takes the logical position and simply applies a camera shift.
      /// </summary>
      public Position PositionLogicalShift
      {
         get
         {
            //Position logicalShiftPos = m_camera.MapLogicalToLogicalShift(PositionLogical);
            //return logicalShiftPos;
            return null;
         }
      }

      /// <summary>
      /// An optional field that can be used to indicate the order in which objects should be drawn.
      /// </summary>
      public Int32 DrawOrder { get; set; }

      #endregion

      #region Public Methods

      #region Sprite

      /// <summary>
      /// Sets the sprite key, and creates a new sprite for that key.
      /// </summary>
      /// <param name="spriteKey"></param>
      public void InitSprite(String spriteKey)
      {
         m_spriteKey = spriteKey;
         m_sprite = m_model.SpriteFactory.SpawnSpriteInstance(m_spriteKey);
      }

      #endregion

      #region Drawing

      /// <summary>
      /// Whether this object even appears on the camera.
      /// </summary>
      /// <returns></returns>
      public Boolean ShouldBeDrawn()
      {
         Boolean shouldDraw = Visible && Math2d.RectanglesIntersect(m_camera.LogicalRect, PositionLogical);
         return shouldDraw;
      }

      /// <summary>
      /// Draws this sprite onto the screen.  Uses the object's display position.
      /// </summary>
      public void DrawDisplay(Graphics g)
      {
         if (ShouldBeDrawn())
         {
            Sprite.Draw(g, m_camera.CurrentScale, PositionDisplay);
         }
      }

      /// <summary>
      /// Draws this sprite onto the screen, using the logical position without any shifting.
      /// </summary>
      public void DrawLogical(Graphics g)
      {
         if (ShouldBeDrawn())
         {
            Sprite.Draw(g, m_camera.CurrentScale, PositionLogical);
         }
      }

      /// <summary>
      /// Draws this sprite, using the logical shift position.  No scaling.
      /// </summary>
      public void DrawLogicalShift(Graphics g)
      {
         if (ShouldBeDrawn())
         {
            Sprite.Draw(g, m_camera.CurrentScale, PositionLogicalShift);
         }
      }

      #endregion

      #endregion
   }
}
