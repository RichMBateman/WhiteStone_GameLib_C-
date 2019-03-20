using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Numbers
{
   public static class Lists
   {
      /// <summary>
      /// Assumes this list is unsorted and not empty.  Gets the index of the largest value.
      /// </summary>
      public static int GetIndexOfLargestValue(List<Double> list)
      {
         int strongestIndex = 0;
         double highestValue = list[0];

         for (int i = 1; i < list.Count; i++)
         {
            if (list[i] > highestValue)
            {
               strongestIndex = i;
               highestValue = list[i];
            }
         }

         return strongestIndex;
      }
   }
}
