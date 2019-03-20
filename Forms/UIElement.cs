using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Spatial;
using System;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.Forms
{
   /// <summary>
   /// Represents something that can be drawn on screen.  Can keep track of whether it's moused over.
   /// </summary>
   public class UIElement
   {
      #region Private Members

      private readonly BaseGameModel m_model;
      private readonly Position m_logPos;
      private Boolean m_isMousedOver;

      #endregion

      #region Constructors

      public UIElement(BaseGameModel model, Double bottomCenterX, Double bottomCenterY, Double width, Double height)
      {
         m_model = model;
         m_logPos = new Position(bottomCenterX, bottomCenterY, width, height);
      }

      #endregion

      #region Public Properties

      public Position LogicalPosition { get { return m_logPos; } }
      /// <summary>
      /// If BaseShiftPos is unset, just returns Logical Position.
      /// Else, returns the logical position after adding the base position.
      /// </summary>
      public Position LogicalPositionShifted
      {
         get
         {
            Position logPosToUse = LogicalPosition;
            if (BaseShiftPos != null)
            {
               logPosToUse = m_logPos.AddPositionUpperLeft(BaseShiftPos);
            }
            return logPosToUse;
         }
      }
      /// <summary>
      /// A position that represents some base offset position.  For example, imagine you have the same widget 
      /// (consisting of numerous UIElements) in various places; you could give each one its own offset.  
      /// If this is null, it is ignored; otherwise it is added to the logical position before the logical position is used in
      /// calculations.
      /// </summary>
      public Position BaseShiftPos { get; set; }
      /// <summary>
      /// The display position.  Ignores the camera position.
      /// </summary>
      public Position DisplayPosition
      {
         get
         {
            return m_model.Camera.MapLogicalToDisplay(LogicalPositionShifted, true);
         }
      }
      public Boolean IsMousedOver { get { return m_isMousedOver; } }

      #endregion

      #region Public Methods

      public void CheckMouseOver()
      {
        // m_isMousedOver = m_model.IsPositionMousedOver(LogicalPositionShifted);
      }

      #endregion
   }
}
