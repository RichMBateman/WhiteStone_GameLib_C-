using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WhiteStone.GameLib.Forms
{
   /// <summary>
   /// Helps map keyboard input to an index position.
   /// The assumption is that we will never need the user to select from more than a certain number of items at once, so the keys 0-9, a-z should be sufficient.
   /// </summary>
   public static class KeyToIndexHelper
   {
      #region Private Static Members

      private readonly static Dictionary<Keys, Int32> m_keyToIndexPosition = new Dictionary<Keys, Int32>();

      #endregion

      #region Static Constructor

      static KeyToIndexHelper()
      {
         InitializeKeyToIndexPositionMap();
      }

      #endregion

      #region Public Api

      /// <summary>
      /// Iterates over the set of keys that could map to an index position, and returns the first applicable one that is currently down (with delay).
      /// </summary>
      public static Boolean MapCurrentDownKeyToIndexPosition(UserInput userInput, out Int32 index)
      {
         index = 0;
         Boolean isApplicable = false;
         foreach (KeyValuePair<Keys, Int32> kvp in m_keyToIndexPosition)
         {
            if (userInput.IsKeyDownWithDelay(kvp.Key))
            {
               if (MapKeyToIndexPosition(kvp.Key, out index))
               {
                  isApplicable = true;
                  break;
               }
            }
         }
         return isApplicable;
      }

      /// <summary>
      /// Maps a key to an index position.  Returns false if this key is not applicable.
      /// </summary>
      public static Boolean MapKeyToIndexPosition(Keys k, out Int32 index)
      {
         index = 0;
         Boolean keyApplicable = m_keyToIndexPosition.ContainsKey(k);
         if (keyApplicable)
         {
            index = m_keyToIndexPosition[k];
         }
         return keyApplicable;
      }

      #endregion

      #region Private Methods

      private static void InitializeKeyToIndexPositionMap()
      {
         m_keyToIndexPosition.Add(Keys.D1, 0);
         m_keyToIndexPosition.Add(Keys.NumPad1, 0);
         m_keyToIndexPosition.Add(Keys.D2, 1);
         m_keyToIndexPosition.Add(Keys.NumPad2, 1);
         m_keyToIndexPosition.Add(Keys.D3, 2);
         m_keyToIndexPosition.Add(Keys.NumPad3, 2);
         m_keyToIndexPosition.Add(Keys.D4, 3);
         m_keyToIndexPosition.Add(Keys.NumPad4, 3);
         m_keyToIndexPosition.Add(Keys.D5, 4);
         m_keyToIndexPosition.Add(Keys.NumPad5, 4);
         m_keyToIndexPosition.Add(Keys.D6, 5);
         m_keyToIndexPosition.Add(Keys.NumPad6, 5);
         m_keyToIndexPosition.Add(Keys.D7, 6);
         m_keyToIndexPosition.Add(Keys.NumPad7, 6);
         m_keyToIndexPosition.Add(Keys.D8, 7);
         m_keyToIndexPosition.Add(Keys.NumPad8, 7);
         m_keyToIndexPosition.Add(Keys.D9, 8);
         m_keyToIndexPosition.Add(Keys.NumPad9, 8);

         m_keyToIndexPosition.Add(Keys.A, 9);
         m_keyToIndexPosition.Add(Keys.B, 10);
         m_keyToIndexPosition.Add(Keys.C, 11);
         m_keyToIndexPosition.Add(Keys.D, 12);
         m_keyToIndexPosition.Add(Keys.E, 13);
         m_keyToIndexPosition.Add(Keys.F, 14);
         m_keyToIndexPosition.Add(Keys.G, 15);
         m_keyToIndexPosition.Add(Keys.H, 16);
         m_keyToIndexPosition.Add(Keys.I, 17);
         m_keyToIndexPosition.Add(Keys.J, 18);
         m_keyToIndexPosition.Add(Keys.K, 19);
         m_keyToIndexPosition.Add(Keys.L, 20);
         m_keyToIndexPosition.Add(Keys.M, 21);
         m_keyToIndexPosition.Add(Keys.N, 22);
         m_keyToIndexPosition.Add(Keys.O, 23);
         m_keyToIndexPosition.Add(Keys.P, 24);
         m_keyToIndexPosition.Add(Keys.Q, 25);
         m_keyToIndexPosition.Add(Keys.R, 26);
         m_keyToIndexPosition.Add(Keys.S, 27);
         m_keyToIndexPosition.Add(Keys.T, 28);
         m_keyToIndexPosition.Add(Keys.U, 29);
         m_keyToIndexPosition.Add(Keys.V, 30);
         m_keyToIndexPosition.Add(Keys.W, 31);
         m_keyToIndexPosition.Add(Keys.X, 32);
         m_keyToIndexPosition.Add(Keys.Y, 33);
         m_keyToIndexPosition.Add(Keys.Z, 34);
      }

      #endregion
   }
}
