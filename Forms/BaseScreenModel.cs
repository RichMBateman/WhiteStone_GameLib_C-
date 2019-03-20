using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Spatial;
using System;
using System.Collections.Generic;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.Forms
{
   /// <summary>
   /// Represents a set of UI elements that will appear on a screen.  Can check for whether elements are moused over.
   /// </summary>
   public class BaseScreenModel
   {
      #region Private Members

      #region Model

      protected readonly BaseGameModel m_model;

      #endregion

      #region UI Elements

      protected readonly List<UIElement> m_allUIElements = new List<UIElement>();

      #endregion

      #endregion

      #region Constructors

      public BaseScreenModel(BaseGameModel model)
      {
         m_model = model;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// All UI Elements on this screen.
      /// </summary>
      public List<UIElement> AllUIElements { get { return m_allUIElements; } }

      #endregion

      #region Public Methods

      /// <summary>
      /// A method that is called to indicate that one game frame has completed.
      /// The base behavior is to check all the UIElements for mouse over.
      /// </summary>
      public virtual void Tick()
      {
         foreach (UIElement e in m_allUIElements)
         {
            e.CheckMouseOver();
         }
      }

      #endregion

      #region Protected Methods

      protected UIElement CreateUIElement(Double bX, Double bY, Double w, Double h, Position baseShift = null)
      {
         UIElement uie = new UIElement(m_model, bX, bY, w, h);
         uie.BaseShiftPos = baseShift;
         m_allUIElements.Add(uie);
         return uie;
      }

      #endregion

      #region Private Methods

      #endregion
   }
}
