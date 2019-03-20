using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Extension
{
   public static class ExtendList
   {
      private static Random rng = new Random();

      /// <summary>
      /// Extension method to shuffle a list of items.
      /// From: http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
      /// </summary>
      public static void Shuffle<T>(this IList<T> list)
      {
         int n = list.Count;
         while (n > 1)
         {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
         }
      }
   }
}
