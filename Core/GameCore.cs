using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using WhiteStone.GameLib.Interfaces;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.Forms;
using WhiteStone.GameLib.View;

namespace WhiteStone.GameLib.Core
{
   #region Delegates

   /// <summary>
   /// Represents a function capable of returning a Bitmap that represents the game buffer.
   /// </summary>
   public delegate Bitmap GetGameBufferHandler();
   /// <summary>
   /// Used for a function capable of getting total available drawing size.
   /// </summary>
   public delegate Size GetTotalDrawingSizeHandler();
   /// <summary>
   /// Describes functions that can accept a new bitmap (that will be shown on the form)
   /// </summary>
   public delegate void AcceptLatestBitmapFrameHandler(Bitmap buffer);
   /// <summary>
   /// Represents a function capable of returning the current game state
   /// </summary>
   public delegate String GetCurrentGameStateKeyHandler();
   /// <summary>
   /// For transitioning game state.
   /// </summary>
   public delegate void TransitionToStateByKey(String stateKey);

   #endregion

   /// <summary>
   /// A fundamental object that represents a game.  You install views (screens), controllers, models, and game states,
   /// and it manages updating the model, getting user input and applying it, and drawing to the screen.
   /// During a frame tick, it accepts user input, performs updates, and draws to the screen,
   /// using installed components.
   /// </summary>
   public class GameCore<T> : ITicker
      where T : BaseGameModel
   {
      #region Members

      private BaseGameForm m_gameForm;

      private Simulation m_simulation;
      private T m_model;
      private Painter<T> m_painter;
      private UserInput m_userInput;
      private Dictionary<String, GameState<T>> m_gameStates = new Dictionary<String, GameState<T>>();

      private GetGameBufferHandler m_getGameBufferHandler;
      private GetTotalDrawingSizeHandler m_getTotalDrawingSizeHandler;
      private AcceptLatestBitmapFrameHandler m_acceptLatestBitmapFrameHandler;

      private GameState<T> m_currentState;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new GameCore.  You must supply the GameForm you'll be using for your drawing.
      /// </summary>
      public GameCore(BaseGameForm gameForm)
      {
         m_gameForm = gameForm;

         m_simulation = new Simulation();
         m_simulation.InstallModule(this);

         m_painter = new Painter<T>();
         m_userInput = new UserInput();

         m_gameForm.ConfigureGameCoreWithFormCallbacks(this);
      }

      #endregion

      #region Public Api

      #region Callback Installation

      /// <summary>
      /// Installs a callback function that can retrieve the game buffer whenever it's needed.
      /// </summary>
      public void InstallGetGameBufferHandler(GetGameBufferHandler handler)
      {
         m_getGameBufferHandler = handler;
      }

      /// <summary>
      /// Installs a callback function for invalidating the surface of the screen.
      /// </summary>
      public void InstallInvalidateDrawingSurfaceHandler(InvalidateDrawingSurfaceHandler handler)
      {
         m_painter.InstallInvalidateDrawingSurfaceHandler(handler);
      }

      /// <summary>
      /// Installs a callback for translating the current mouse position to a coordinate relative to the screen.
      /// </summary>
      public void InstallLocalizeScreenMousePositionHandler(LocalizeScreenMousePositionHandler handler)
      {
         m_userInput.InstallLocalizeScreenMousePositionHandler(handler);
      }

      /// <summary>
      /// Installs a callback to determine whether the main form has focus.
      /// </summary>
      public void InstallMainFormHasFocusCb(MainFormHasFocus handler)
      {
         m_userInput.InstallMainFormHasFocusCb(handler);
      }

      /// <summary>
      /// Installs a callback function that can be used to determine how much size is available for drawing.
      /// </summary>
      public void InstallGetTotalDrawingSizeHandler(GetTotalDrawingSizeHandler handler)
      {
         m_getTotalDrawingSizeHandler = handler;
      }

      /// <summary>
      /// Installs a callback to accept a new bitmap when one is ready.
      /// </summary>
      public void InstallAcceptLatestBitmapFrameHandler(AcceptLatestBitmapFrameHandler handler)
      {
         m_acceptLatestBitmapFrameHandler = handler;
      }


      #endregion

      #region Game Element Registration

      /// <summary>
      /// Registers a new game state.  You may indicate which state is the initial one.
      /// </summary>
      public void RegisterGameState(GameState<T> gameState, Boolean isInitialState = false)
      {
         m_gameStates.Add(gameState.Key, gameState);
         if (isInitialState)
         {
            m_currentState = gameState;
         }
      }

      /// <summary>
      /// Registers the model with the game core.
      /// </summary>
      public void RegisterGameModel(T gameModel)
      {
         m_model = gameModel;
         gameModel.UserInput = m_userInput;
         gameModel.GetCurrentStateKeyCb = GetCurrentGameStateKey;
         gameModel.TransitionToStateByKeyCb = TransitionToGameStateByKey;
      }

      #endregion

      #region Fonts

      /// <summary>
      /// Adds a custom font to the application.
      /// </summary>
      public void AddCustomFont(String fontFilePath, String fontName)
      {
         m_painter.InstallFontFromFile(fontFilePath, fontName);
      }

      #endregion

      #region Initialization

      /// <summary>
      /// Starts the game.
      /// </summary>
      public void Start()
      {
         m_simulation.Start();
      }

      #endregion

      #region Game State

      /// <summary>
      /// The current game state.
      /// </summary>
      public GameState<T> CurrentState
      {
         get { return m_currentState; }
      }

      #endregion

      #region Clean Up

      /// <summary>
      /// Function to call when you're all finished.
      /// </summary>
      public void CleanUp()
      {
         m_simulation.Stop();
      }

      #endregion

      #endregion

      #region ITicker

      /// <summary>
      /// Called every time a frame tick passes.
      /// </summary>
      public void AcceptFrameTick()
      {
         //Debug.WriteLine(DateTime.Now.ToString("mm:ss.fff"));

         if (CurrentState != null)
         {
            CurrentState.Controller.ApplyInput(m_model, m_userInput);
            if (m_model.TickDelayCounter <= 0)
            {
               m_model.AcceptFrameTick();
               m_model.TickDelayCounter = m_model.TickDelay;
            }
            else
            {
               m_model.TickDelayCounter--;
            }
            if (m_getTotalDrawingSizeHandler != null)
            {
               Size clientSize = m_getTotalDrawingSizeHandler();
               if (clientSize.Width > 0 && clientSize.Height > 0)
               {
                  Boolean displayAreaChanged = m_model.Camera.UpdateDisplayInformation(clientSize.Width, clientSize.Height);
                  if (displayAreaChanged)
                  {
                     m_model.NotifyDisplayAreaChanged();
                  }

                  // Don't bother drawing if the client is minimized.
                  // The game core creates a new bitmap each cycle.  The form will destroy it when appropriate.
                  // No copies are made, and we use invoking to deal with cross-thread exceptions.
                  Bitmap currentBuffer = new Bitmap(clientSize.Width, clientSize.Height);
                  m_painter.Draw(m_model, CurrentState, currentBuffer);
                  if (m_acceptLatestBitmapFrameHandler != null)
                  {
                     try
                     {
                        m_gameForm.Invoke(new Action(() =>
                        {
                           m_acceptLatestBitmapFrameHandler(currentBuffer);
                        }));
                     }
                     catch
                     {
                        // Sometimes we may crash we are are shutting down, and there doesn't seem to be any way around that.
                     }
                  }
               }
            }
         }
      }

      #endregion

      #region Private Methods

      private String GetCurrentGameStateKey()
      {
         return CurrentState.Key;
      }

      private void TransitionToGameStateByKey(String key)
      {
         m_currentState = m_gameStates[key];
      }

      #endregion
   }
}
