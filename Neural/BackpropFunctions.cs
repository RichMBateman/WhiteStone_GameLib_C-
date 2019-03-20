using System;

namespace WhiteStone.GameLib.Neural
{
   /// <summary>
   /// Functions related to backpropagation.
   /// </summary>
   public static class BackpropFunctions
   {
      #region Public Functions
      /// <summary>
      /// The squared error for the output of a neuron.  The reason the 0.5 is included is to cancel the exponent 
      /// when we differentiate this function later.
      /// </summary>
      /// <param name="targetOutput">The target or desired output for the training sample.</param>
      /// <param name="actualOutput">The actual output of the output neuron.</param>
      /// <returns>The squared error.</returns>
      public static Double SquaredErrorFunction(Double targetOutput, Double actualOutput)
      {
         return (0.5 * Math.Pow((targetOutput - actualOutput), 2));
      }
      #endregion
   }
}
