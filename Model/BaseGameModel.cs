using System;
using System.Collections.Generic;
using WhiteStone.GameLib.Interfaces;
using WhiteStone.GameLib.View;
using WhiteStone.GameLib.Sprites;
using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Time;
using WhiteStone.GameLib.Drawing;
using System.Drawing;
using WhiteStone.GameLib.Forms;
using WhiteStone.GameLib.Spatial;

namespace WhiteStone.GameLib.Model
{
   /// <summary>
   /// Within a game, you should have a Model that represents the logical elements of your game (completely separated from user input, and how
   /// they will be presented).  Use this base game model to get some useful elements, such as a TickDelay that can be used to for easily slowing down or speeding up the game.
   /// There is also a camera that you can use to represent what part of your world is currently visible.
   /// </summary>
   public abstract class BaseGameModel : ITicker
   {
      #region Private Members

      private const String TickWaveDependentColorWave = "ColorWave";

      private readonly Random m_random = new Random();
      private readonly Dictionary<String, TickWave> m_tickWaves = new Dictionary<String, TickWave>();
      private readonly Dictionary<String, Dictionary<String, ITicker>> m_tickWaveDependents = new Dictionary<string, Dictionary<string, ITicker>>();

      #endregion

      #region Protected Members

      protected Int32 m_tickDelay;
      protected Int32 m_tickDelayCounter;
      protected readonly Camera m_camera = new Camera();
      protected readonly SpriteFactory m_spriteFactory = new SpriteFactory();

      #endregion

      #region Constructors

      public BaseGameModel()
      {
         InitializeTickWaveDependentMaps();
      }

      #endregion

      #region Public Properties

      #region Graphics and Display

      /// <summary>
      /// Maps a window from the logical realm into the display screen.
      /// </summary>
      public Camera Camera
      {
         get { return m_camera; }
      }

      /// <summary>
      /// Manages sprites, sprite sheets, and animations.
      /// </summary>
      public SpriteFactory SpriteFactory
      {
         get { return m_spriteFactory; }
      }


      #endregion

      #region Simulation

      /// <summary>
      /// The total number of frame ticks that must pass before the model will be updated.  (Controllers and Views are always updated at a normal rate)
      /// </summary>
      public Int32 TickDelay
      {
         get { return m_tickDelay; }
         set
         {
            m_tickDelay = value;
            m_tickDelayCounter = m_tickDelay;
         }
      }

      /// <summary>
      /// The current ticks remaining before the model should be updated again.
      /// </summary>
      public Int32 TickDelayCounter
      {
         get { return m_tickDelayCounter; }
         set { m_tickDelayCounter = value; }
      }


      #endregion

      #region State

      /// <summary>
      /// Callback used to get the current game state key.
      /// </summary>
      internal GetCurrentGameStateKeyHandler GetCurrentStateKeyCb { get; set; }
      /// <summary>
      /// Callback for state transitioning.
      /// </summary>
      internal TransitionToStateByKey TransitionToStateByKeyCb { get; set; }

      /// <summary>
      /// The key for the current state of the model.
      /// </summary>
      public String CurrentStateKey
      {
         get
         {
            String stateKey = GetCurrentStateKeyCb();
            return stateKey;
         }
      }

      #endregion

      #region Random

      /// <summary>
      /// Random number generator.
      /// </summary>
      public Random RandomNumGenerator
      {
         get { return m_random; }
      }

      #endregion

      #region User Input

      /// <summary>
      /// The user input module that should have been installed by the game core.
      /// </summary>
      public UserInput UserInput { get; set; }

      /// <summary>
      /// Where the mouse currently is (in actual screen coordinates).
      /// </summary>
      public Point CurrentDisplayMousePosition { get; set; }

      #endregion

      #endregion

      #region Public Methods

      #region Graphics and Display

      /// <summary>
      /// A method that is called when the display area has changed from what was last recorded.  This is an opportunity to make any changes in your model
      /// (such as rebuilding cached PreparedPagedText).
      /// </summary>
      public virtual void NotifyDisplayAreaChanged()
      {
      }

      #endregion

      #region Simulation

      /// <summary>
      /// This function is called every frame tick by the simulation.
      /// </summary>
      public void AcceptFrameTick()
      {
         HandleFrameTick(); // Call into the specific implementation.
         UpdateAllTickWaves();
         UpdateAllTickWaveDependentObjects();
      }

      /// <summary>
      /// This function is called every frame tick.
      /// </summary>
      public abstract void HandleFrameTick();

      /// <summary>
      /// Transitions the game to the state for this key.
      /// </summary>
      public void TransitionToGameStateByKey(String key)
      {
         TransitionToStateByKeyCb(key);
      }

      #endregion

      #region TickWaves

      /// <summary>
      /// Creates a new TickWave and installs it into model.  Returns it.
      /// </summary>
      public TickWave InstallTickWave(String tickWaveKey)
      {
         TickWave tw = new TickWave(tickWaveKey);
         m_tickWaves.Add(tickWaveKey, tw);
         return tw;
      }

      /// <summary>
      /// Gets the tick wave specified by some key.
      /// </summary>
      public TickWave GetTickWave(String tickWaveKey)
      {
         TickWave tw = m_tickWaves[tickWaveKey];
         return tw;
      }

      #endregion

      #region ColorWaves

      /// <summary>
      /// Creates a new ColorWave and installs it into model.  Returns it.
      /// </summary>
      public ColorWave InstallColorWave(String colorWaveKey, Color boundary1, Color boundary2, String tickWaveKey)
      {
         TickWave tickWave = m_tickWaves[tickWaveKey];
         ColorWave cw = new ColorWave(colorWaveKey, boundary1, boundary2, tickWave);
         Dictionary<String, ITicker> colorWaves = m_tickWaveDependents[TickWaveDependentColorWave];
         colorWaves.Add(cw.Key, cw);
         return cw;
      }

      /// <summary>
      /// Gets the tick wave specified by some key.
      /// </summary>
      public ColorWave GetColorWave(String colorWaveKey)
      {
         Dictionary<String, ITicker> colorWaves = m_tickWaveDependents[TickWaveDependentColorWave];
         ColorWave cw = colorWaves[colorWaveKey] as ColorWave;
         return cw;
      }

      #endregion

      #endregion

      #region Private Methods

      #region Tick Waves

      private void UpdateAllTickWaves()
      {
         foreach (TickWave tw in m_tickWaves.Values)
         {
            tw.AcceptFrameTick();
         }
      }

      #endregion

      #region Tick Wave Dependents

      private void InitializeTickWaveDependentMaps()
      {
         Dictionary<String, ITicker> colorWaveMap = new Dictionary<string, ITicker>();

         m_tickWaveDependents.Add(TickWaveDependentColorWave, colorWaveMap);
      }

      private void UpdateAllTickWaveDependentObjects()
      {
         foreach (Dictionary<String, ITicker> tickWaveMapType in m_tickWaveDependents.Values)
         {
            foreach (ITicker ticker in tickWaveMapType.Values)
            {
               ticker.AcceptFrameTick();
            }
         }
      }

      #endregion

      #endregion

   }
}
