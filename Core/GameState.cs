using System;
using System.Collections.Generic;
using WhiteStone.GameLib.Controller;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.View;

namespace WhiteStone.GameLib.Core
{
   /// <summary>
   /// Represents a mode that a game can be in.  The game state has a single Controller to accept input, and multiple Views (although you could just use one for simplicity).
   /// The GameState is based around a particular model.
   /// </summary>
   public class GameState<T>
      where T : BaseGameModel
   {
      #region Members

      private String m_key;
      private BaseController<T> m_controller;
      private List<BaseView<T>> m_views = new List<BaseView<T>>();

      #endregion

      #region Properties

      /// <summary>
      /// The unique key for this game state.
      /// </summary>
      public String Key
      {
         get { return m_key; }
      }

      /// <summary>
      /// The views associated with this game state.
      /// </summary>
      public List<BaseView<T>> Views
      {
         get { return m_views; }
      }

      /// <summary>
      /// The Controller managing this game state.
      /// </summary>
      public BaseController<T> Controller
      {
         get { return m_controller; }
      }

      #endregion

      #region Constructors

      public GameState(String key)
      {
         m_key = key;
      }

      #endregion

      #region Public Methods

      public void SetupController(BaseController<T> controller)
      {
         m_controller = controller;
      }

      public void AddView(BaseView<T> view)
      {
         m_views.Add(view);
      }

      #endregion
   }
}
