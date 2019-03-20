using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;

/* Links:
 * http://www.switchonthecode.com/tutorials/winforms-accessing-mouse-and-keyboard-state
 * http://msdn.microsoft.com/en-us/library/windows/desktop/ms646301%28v=vs.85%29.aspx
 */

namespace WhiteStone.GameLib.Forms
{
   #region Delegate

   public delegate Point LocalizeScreenMousePositionHandler(Point globalPoint);
   public delegate Boolean MainFormHasFocus();

   #endregion

   /// <summary>
   /// Class used for easy polling of keyboard/mouse state.
   /// (As opposed to using events to detect when the user enters input)
   /// </summary>
   public class UserInput
   {
      #region Constants

      /// <summary>
      /// Used internally to help identify combinations of keys that are held down.
      /// </summary>
      private const String KeyComboSeparator = "|";
      /// <summary>
      /// The number of milliseconds to wait until we consider a key combination is down.  (prevents us from accepting rapid input)
      /// </summary>
      private const Int32 KeyDelayDurationMs = 150;

      #endregion

      #region Private Members

      private LocalizeScreenMousePositionHandler m_localizeScreenMousePositionHandler;
      private MainFormHasFocus m_mainFormHasFocusCb;

      /// <summary>
      /// For each combination of keys, the last time they were pressed / processed.
      /// </summary>
      private readonly Dictionary<String, DateTime> m_keyComboLastProcessedTime = new Dictionary<String, DateTime>();
      /// <summary>
      /// A temporary holding cell that stores the last time keys were processed.  Used to updated m_keyComboLastProcessedTime.
      /// </summary>
      private readonly Dictionary<String, DateTime> m_keyComboLastProcessedTimeBin = new Dictionary<String, DateTime>();

      private Boolean m_freeTextModeEnabled;
      private String m_freeText = String.Empty;
      private readonly Dictionary<Keys, Char> m_freeTextAllowedKeys = new Dictionary<Keys, Char>();
      private readonly List<Keys> m_freeTextForbiddenKeys = new List<Keys>();

      #endregion

      #region Constructors

      public UserInput()
      {
         InitializeFreeTextAllowedKeys();
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// The list of keys that are not allowed for the current free text.
      /// </summary>
      public List<Keys> FreeTextForbiddenKeys { get; set; }

      #endregion

      #region Public API

      #region Callback Installations

      /// <summary>
      /// Installs a function that can be used to take the current mouse position and translate it to a coordinate relative to the screen.
      /// </summary>
      public void InstallLocalizeScreenMousePositionHandler(LocalizeScreenMousePositionHandler handler)
      {
         m_localizeScreenMousePositionHandler = handler;
      }

      /// <summary>
      /// Installs a function that determines whether the main game form has focus.
      /// </summary>
      public void InstallMainFormHasFocusCb(MainFormHasFocus handler)
      {
         m_mainFormHasFocusCb = handler;
      }

      #endregion

      #region Free Text Mode

      public void CheckForFreeTextEntry()
      {
         if (m_freeTextModeEnabled)
         {
            if (IsKeyDownWithDelay(Keys.Back))
            {
               if (m_freeText.Length > 0)
               {
                  m_freeText = m_freeText.Substring(0, m_freeText.Length - 1);
               }
               return;
            }
            Boolean isShiftDown = IsShiftKeyDown();
            foreach (KeyValuePair<Keys, Char> kvp in m_freeTextAllowedKeys)
            {
               if (!m_freeTextForbiddenKeys.Contains(kvp.Key)) // ignore the key if it's forbidden
               {
                  if (IsKeyDownWithDelay(kvp.Key))
                  {
                     Char toAppend = kvp.Value;
                     if (isShiftDown)
                     {
                        toAppend = Char.ToUpper(toAppend);
                     }
                     m_freeText = String.Concat(m_freeText, toAppend);
                  }
               }
            }
         }
      }

      public void ClearFreeText()
      {
         m_freeText = String.Empty;
      }

      public String GetFreeText()
      {
         return m_freeText;
      }

      public void SetFreeText(String text)
      {
         m_freeText = text;
      }

      public void EnableFreeTextMode()
      {
         m_freeTextModeEnabled = true;
      }

      public void DisableFreeTextMode()
      {
         m_freeTextModeEnabled = false;
         ClearFreeText();
      }

      #endregion

      #region Keyboard Queries

      /// <summary>
      /// Iterates through the collection of all successful key combo presses, and updates the last time of processing.
      /// This is necessary so that, when you ask whether a key combination is down, if it was just pressed, it won't be reported as being pressed again.
      /// This scheme allows a user to call "AreKeyDowns()" as often as they'd like; previously, as soon as you called that function once, subsequent
      /// calls would report the keys were NOT down.
      /// </summary>
      public void MarkAllKeyDelays()
      {
         foreach (KeyValuePair<String, DateTime> kvp in m_keyComboLastProcessedTimeBin)
         {
            m_keyComboLastProcessedTime[kvp.Key] = kvp.Value;
         }
         m_keyComboLastProcessedTimeBin.Clear();
      }

      /// <summary>
      /// Whether a set of keys are down, keeping a delay in mind.  We can ask this question as often as we want... it will only return true periodically.
      /// This allows a user to hold down a key and not have it be treated as a huge number of separate inputs.
      /// You must call MarkAllKeyDelays() in order for the delay counter to reset after a successful key press.
      /// </summary>
      public Boolean AreKeysDownWithDelay(params Keys[] list)
      {
         Boolean allKeysDown = true; // Assume all the keys are in fact down.
         String keyComboName = String.Empty;
         foreach (Keys k in list)
         {
            Boolean isKeyDown = IsKeyDown(k);
            if (!isKeyDown)
            {
               allKeysDown = false;
               break;
            }
            keyComboName = k + KeyComboSeparator;
         }
         // If all keys are INDEED down, we don't want to report that unless the delay has passed
         if (allKeysDown)
         {
            DateTime rightNow = DateTime.UtcNow;
            if (m_keyComboLastProcessedTime.ContainsKey(keyComboName))
            {
               // This key combination has been pressed before.
               DateTime lastProcessedTime = m_keyComboLastProcessedTime[keyComboName];
               TimeSpan difference = rightNow - lastProcessedTime;
               if (difference.TotalMilliseconds >= KeyDelayDurationMs)
               {
                  // Enough time has passed.  We can report all keys are down, but we need to update
                  // the last time the key was pressed.
                  // We used to update the main dictionary, but this had the side effect of causing immediate calls to AreKeysDown
                  // to return false.
                  //m_keyComboLastProcessedTime[keyComboName] = rightNow;
                  m_keyComboLastProcessedTimeBin[keyComboName] = rightNow;
               }
               else
               {
                  allKeysDown = false; // NOPE!  Too soon to say this combination of keys are down.
               }
            }
            else
            {
               // we have never encountered this key combination before.
               // We will says the keys are all down.
               // Again, we will store this information to be moved to the main dictionary LATER.
               //m_keyComboLastProcessedTime[keyComboName] = rightNow;
               m_keyComboLastProcessedTimeBin[keyComboName] = rightNow;
            }
         }
         return allKeysDown;
      }

      /// <summary>
      /// Whether a key is down, keeping a delay in mind.
      /// </summary>
      public Boolean IsKeyDownWithDelay(Keys key)
      {
         Boolean isDown = AreKeysDownWithDelay(key);
         return isDown;
      }

      /// <summary>
      /// Whether a key is down.  Calling this function does not change the state of the input module.
      /// </summary>
      public Boolean IsKeyDown(Keys key)
      {
         Boolean isKeyDown = false;
         Boolean formFocused = m_mainFormHasFocusCb();
         if (formFocused)
         {
            isKeyDown = (KeyStates.Down == (GetKeyState(key) & KeyStates.Down));
         }
         return isKeyDown;
      }

      /// <summary>
      /// Whether a key is toggled.
      /// </summary>
      public Boolean IsKeyToggled(Keys key)
      {
         Boolean isKeyToggled = false;
         Boolean formFocused = m_mainFormHasFocusCb();
         if (formFocused)
         {
            isKeyToggled = (KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled));
         }
         return isKeyToggled;
      }

      #endregion

      #region Mouse Queries

      /// <summary>
      /// Gets the current mouse position relative to the game screen.
      /// </summary>
      public Point GetMousePosition()
      {
         Point position = System.Windows.Forms.Control.MousePosition; // Control.MousePosition is relative to the entire display, not to any particular form.
         if (m_localizeScreenMousePositionHandler != null)
         {
            position = m_localizeScreenMousePositionHandler(position);
         }
         return position;
      }

      #endregion

      #endregion

      #region Private Enumerations

      [Flags]
      private enum KeyStates
      {
         None = 0,
         Down = 1,
         Toggled = 2
      }

      #endregion

      #region Private Methods

      #region Free Text

      private void InitializeFreeTextAllowedKeys()
      {
         m_freeTextAllowedKeys.Add(Keys.A, 'a');
         m_freeTextAllowedKeys.Add(Keys.B, 'b');
         m_freeTextAllowedKeys.Add(Keys.C, 'c');
         m_freeTextAllowedKeys.Add(Keys.D, 'd');
         m_freeTextAllowedKeys.Add(Keys.E, 'e');
         m_freeTextAllowedKeys.Add(Keys.F, 'f');
         m_freeTextAllowedKeys.Add(Keys.G, 'g');
         m_freeTextAllowedKeys.Add(Keys.H, 'h');
         m_freeTextAllowedKeys.Add(Keys.I, 'i');
         m_freeTextAllowedKeys.Add(Keys.J, 'j');
         m_freeTextAllowedKeys.Add(Keys.K, 'k');
         m_freeTextAllowedKeys.Add(Keys.L, 'l');
         m_freeTextAllowedKeys.Add(Keys.M, 'm');
         m_freeTextAllowedKeys.Add(Keys.N, 'n');
         m_freeTextAllowedKeys.Add(Keys.O, 'o');
         m_freeTextAllowedKeys.Add(Keys.P, 'p');
         m_freeTextAllowedKeys.Add(Keys.Q, 'q');
         m_freeTextAllowedKeys.Add(Keys.R, 'r');
         m_freeTextAllowedKeys.Add(Keys.S, 's');
         m_freeTextAllowedKeys.Add(Keys.T, 't');
         m_freeTextAllowedKeys.Add(Keys.U, 'u');
         m_freeTextAllowedKeys.Add(Keys.V, 'v');
         m_freeTextAllowedKeys.Add(Keys.W, 'w');
         m_freeTextAllowedKeys.Add(Keys.X, 'x');
         m_freeTextAllowedKeys.Add(Keys.Y, 'y');
         m_freeTextAllowedKeys.Add(Keys.Z, 'z');

         m_freeTextAllowedKeys.Add(Keys.D0, '0');
         m_freeTextAllowedKeys.Add(Keys.D1, '1');
         m_freeTextAllowedKeys.Add(Keys.D2, '2');
         m_freeTextAllowedKeys.Add(Keys.D3, '3');
         m_freeTextAllowedKeys.Add(Keys.D4, '4');
         m_freeTextAllowedKeys.Add(Keys.D5, '5');
         m_freeTextAllowedKeys.Add(Keys.D6, '6');
         m_freeTextAllowedKeys.Add(Keys.D7, '7');
         m_freeTextAllowedKeys.Add(Keys.D8, '8');
         m_freeTextAllowedKeys.Add(Keys.D9, '9');

         m_freeTextAllowedKeys.Add(Keys.NumPad0, '0');
         m_freeTextAllowedKeys.Add(Keys.NumPad1, '1');
         m_freeTextAllowedKeys.Add(Keys.NumPad2, '2');
         m_freeTextAllowedKeys.Add(Keys.NumPad3, '3');
         m_freeTextAllowedKeys.Add(Keys.NumPad4, '4');
         m_freeTextAllowedKeys.Add(Keys.NumPad5, '5');
         m_freeTextAllowedKeys.Add(Keys.NumPad6, '6');
         m_freeTextAllowedKeys.Add(Keys.NumPad7, '7');
         m_freeTextAllowedKeys.Add(Keys.NumPad8, '8');
         m_freeTextAllowedKeys.Add(Keys.NumPad9, '9');

         m_freeTextAllowedKeys.Add(Keys.Space, ' ');
         m_freeTextAllowedKeys.Add(Keys.OemPeriod, '.');
         m_freeTextAllowedKeys.Add(Keys.OemQuestion, '?');
      }

      #endregion

      #region Key-State Helpers

      [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
      private static extern short GetKeyState(int keyCode);

      private static KeyStates GetKeyState(Keys key)
      {
         KeyStates state = KeyStates.None;

         short retVal = GetKeyState((int)key);

         //If the high-order bit is 1, the key is down
         //otherwise, it is up.
         if ((retVal & 0x8000) == 0x8000)
            state |= KeyStates.Down;

         //If the low-order bit is 1, the key is toggled.
         if ((retVal & 1) == 1)
            state |= KeyStates.Toggled;

         return state;
      }

      #endregion

      #region Key Helpers

      private Boolean IsShiftKeyDown()
      {
         Boolean isShiftDown = IsKeyDown(Keys.Shift)
                               || IsKeyDown(Keys.LShiftKey)
                               || IsKeyDown(Keys.RShiftKey)
                               || IsKeyDown(Keys.ShiftKey);
         return isShiftDown;
      }

      #endregion

      #endregion

   }
}
