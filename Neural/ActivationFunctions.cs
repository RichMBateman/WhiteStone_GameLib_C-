using System;

namespace WhiteStone.GameLib.Neural
{
   /// <summary>
   /// Contains activations functions that are used to compute the output of a neuron (and limit it to some range) given a set of input)
   /// </summary>
   /// <remarks>
   /// See:
   /// >https://en.wikibooks.org/wiki/Artificial_Neural_Networks/Activation_Functions
   /// >http://www.cscjournals.org/manuscript/Journals/IJAE/volume1/Issue4/IJAE-26.pdf
   /// >http://dontveter.com/bpr/activate.html (I like this one)
   /// >https://www.desmos.com/calculator (Nice Graph Calculator)
   /// </remarks>
   public static class ActivationFunctions
   {
      #region Public Functions
      /// <summary>
      /// A sigmoidal whose output is bounded to [0..1].  Unipolar Sigmoid (Logistic) function.
      /// </summary>
      /// <param name="x">The total input into this function</param>
      /// <returns>The output, bounded between 0 and 1.</returns>
      /// <remarks>
      /// The derivative of this function = f(x) * (1 - f(x))
      /// </remarks>
      public static Double Sigmoidal_0_1(Double x)
      {
         return (1.0 / (1.0 + Math.Pow(Math.E, -x)));
      }
      /// <summary>
      /// A sigmoidal whose output is bounded to [-1...1].  Bipolar Sigmoid Function.
      /// </summary>
      /// <param name="x">The total input into this function</param>
      /// <returns>Output bounded between -1 and +1.</returns>
      /// <remarks>
      /// The derivative of this function = 0.5 * (1 + f(x)) * (1-f(x))
      /// </remarks>
      public static Double Sigmoidal_1_1(Double x)
      {
         return 2 * Sigmoidal_0_1(x) - 1;
         //return (1 - Math.Pow(Math.E, -x)) / (1 + Math.Pow(Math.E, -x));
      }
      /// <summary>
      /// The output of tanh ranges from -1 to +1.  
      /// It is equivalent to (2 / (1 + e^(-2x))) - 1
      /// Its deriviative is simply 1 - x^2
      /// </summary>
      /// <param name="x">The total input to the function</param>
      /// <returns>The output, bounded between -1 and +1.</returns>
      /// <remarks>
      /// Approximate outputs (flipped for positive values):
      ///     -5   >> ~ -1
      ///     -2   >> ~ -0.96
      ///     -1   >> ~ -0.76
      ///     -0.5 >> ~ -0.46
      ///     0    >> 0
      /// </remarks>
      public static Double Tanh(Double x)
      {
         return Math.Tanh(x);
      }
      /// <summary>
      /// The derivative to Tanh.
      /// </summary>
      /// <param name="x">Input to function.</param>
      /// <returns>The derivative.</returns>
      public static Double TanhDerivative(Double x)
      {
         return (1 - Math.Pow(x, 2));
      }
      #endregion
   }
}
