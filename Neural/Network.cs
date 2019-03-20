using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Neural
{
   /// <summary>
   /// A multi-layer neural network, consisting on one input layer, one hidden layer, and one output layer.
   /// The input and hidden layers always have a bias, in addition to the number of neurons that user specifies.
   /// </summary>
   public class Network
   {
      #region Static Members

      /// <summary>
      /// Random number generator.
      /// </summary>
      private static Random R = new Random();

      #endregion

      #region Private Members
      /// <summary>
      /// The number of input neurons, EXCLUDING the bias neuron.
      /// </summary>
      private Int32 m_numInput;
      /// <summary>
      /// The number of hidden neurons, EXCLUDING the bias neuron.
      /// </summary>
      private Int32 m_numHidden;
      /// <summary>
      /// The number of output neurons.  There is no bias neuron for this layer.
      /// </summary>
      private Int32 m_numOutput;
      /// <summary>
      /// The number of Bias neurons.  Realistically, always one.
      /// In my networks, imagine that the bias neuron for the input and hidden layer is always the LAST neuron in that layer.
      /// The idea behind the bias neuron is to always have a non-zero value being fed to the next layer.  (If all zeroes were fed, you would
      /// never get any output beyond 0... but what if we want non-zero output for a zero input?)
      /// </summary>
      private const Int32 BiasNeuronCount = 1;

      private Double[,] m_ih; // connections from input to hidden
      private Double[,] m_ho; // connections from hidden to output

      private Double[] m_lastInput; // The last calculated input passed into the network, PLUS the constant input from the bias layer.
      private Double[] m_hiddenLayerOutput; // The last calculated output from the hidden layer
      private Double[] m_outputLayerOutput; // The last calculated output from the output layer

      // From: http://mattmazur.com/2015/03/17/a-step-by-step-backpropagation-example/
      // The goal with backpropagation is to update each of the weights in the network so that they
      // cause the actual output to be closer to the target output, thereby minimizing the error
      // of each output neuron and the network as a whole.

      // For a given weight, we want to know how much a change in the weight affects the total error.
      // We want the derivative of the total error with respect to the weight.
      // Also read as "The partial derivative of the total error with respect to the weight."
      // According to the chain rule... this is equal to:
      // Partial derivative of Total Error / Output Neuron * 
      // Partial derivative of Output Neuron / Net Input *
      // Partial derivative of Net Input / Weight

      // Output layer backpropagation
      private Double[] m_outputSquaredErrors; // The total error for each output neuron, with respect to the ideal output.
      private Double m_totalOutputError; // The total error overall of the entire network.  The sum of each output neuron's error.
      private Double[] m_outputPartialDerivTotalErrorOverOutputNeuron; // Partial Derivative of the total error with respect to the output neuron.
      private Double[] m_outputPartialDerivOutputNeuronOverTotalInput; // Partial Derivative of the output respecting net input
      private Double[,] m_outputPartialDerivTotalInputOverWeight; // Partial Derivative of the total net input respecting weight.
      private Double[,] m_hoWeightChanges; // Changes to weights for one backpropagation pass

      // Hidden layer backpropagation
      private Double[,] m_hiddenPartialDerivTotalErrorOverHiddenOutput;
      private Double[,] m_hiddenPartialDerivHiddenOutputOverNetInput;
      private Double[,] m_hiddenPartialDerivNetInputOverWeight;
      private Double[,] m_ihWeightChanges; // Changes to weights for one backpropagation pass

      #endregion

      #region Properties
      /// <summary>
      /// The number of input neurons in this network, excluding the Bias.
      /// </summary>
      public Int32 NumInput
      {
         get { return m_numInput; }
      }
      /// <summary>
      /// The number of hidden neurons in this network, excluding the Bias.
      /// </summary>
      public Int32 NumHidden
      {
         get { return m_numHidden; }
      }
      /// <summary>
      /// The number of output neurons.  There is no bias neuron in this layer.
      /// </summary>
      public Int32 NumOutput
      {
         get { return m_numOutput; }
      }
      /// <summary>
      /// The weights from the input neurons (plus bias) to the hidden layer.
      /// </summary>
      public Double[,] IH
      {
         get { return m_ih; }
      }
      /// <summary>
      /// The weights from the hidden neurons (plus bias) to the output layer).
      /// </summary>
      public Double[,] HO
      {
         get { return m_ho; }
      }
      /// <summary>
      /// The last calculated output.
      /// </summary>
      public Double[] CalculatedOutput
      {
         get { return m_outputLayerOutput; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Creates a new neural network with these neurons.  All weights are initially 0.
      /// </summary>
      public Network(Int32 numInput, Int32 numHidden, Int32 numOutput)
      {
         m_numInput = numInput;
         m_numHidden = numHidden;
         m_numOutput = numOutput;

         m_ih = new Double[m_numInput + BiasNeuronCount, m_numHidden];
         m_ho = new Double[m_numHidden + BiasNeuronCount, m_numOutput];

         m_ihWeightChanges = new Double[m_numInput + BiasNeuronCount, m_numHidden];
         m_hoWeightChanges = new Double[m_numHidden + BiasNeuronCount, m_numOutput];

         m_lastInput = new Double[m_numInput + BiasNeuronCount];
         m_lastInput[m_numInput] = 1; // Input from the bias neuron is ALWAYS one and NEVER CHANGES
         m_hiddenLayerOutput = new Double[m_numHidden + BiasNeuronCount];
         m_hiddenLayerOutput[m_numHidden] = 1; // Input from the bias neuron is ALWAYS one and NEVER changes.
         m_outputLayerOutput = new Double[m_numOutput];

         m_outputSquaredErrors = new Double[m_numOutput];
         m_outputPartialDerivTotalErrorOverOutputNeuron = new Double[m_numOutput];
         m_outputPartialDerivOutputNeuronOverTotalInput = new Double[m_numOutput];
         m_outputPartialDerivTotalInputOverWeight = new Double[m_numHidden + BiasNeuronCount, m_numOutput];

         m_hiddenPartialDerivTotalErrorOverHiddenOutput = new Double[m_numInput + BiasNeuronCount, m_numHidden];
         m_hiddenPartialDerivHiddenOutputOverNetInput = new Double[m_numInput + BiasNeuronCount, m_numHidden];
         m_hiddenPartialDerivNetInputOverWeight = new Double[m_numInput + BiasNeuronCount, m_numHidden];
      }
      #endregion

      #region Public Methods

      #region Initialization

      /// <summary>
      /// Randomizes weights from -1.0 to 1.0
      /// </summary>
      public void RandomizeNetwork()
      {
         RandomizeNetwork(-1.0, 1.0);
      }
      public void RandomizeNetwork(Double lowerBound, Double upperBound)
      {
         for (Int32 i = 0; i < m_numInput + BiasNeuronCount; i++)
         {
            for (Int32 j = 0; j < m_numHidden; j++)
            {
               m_ih[i, j] = ((upperBound - lowerBound) * R.NextDouble()) + lowerBound;
            }
         }
         for (Int32 i = 0; i < m_numHidden + BiasNeuronCount; i++)
         {
            for (Int32 j = 0; j < m_numOutput; j++)
            {
               m_ho[i, j] = ((upperBound - lowerBound) * R.NextDouble()) + lowerBound;
            }
         }
      }

      #endregion

      #region Feed-Forward Process

      /// <summary>
      /// Given the input (should be one entry for each input neuron, excluding the bias, whose input is always 1), calculate and store
      /// the output of the hidden and output layers.
      /// </summary>
      /// <param name="input">An array of values corresponding to the input for the input layer.</param>
      public void ComputeOutput(Double[] input)
      {
         if (input.Length != m_numInput)
            throw new Exception("The input needs to be " + this.m_numInput.ToString() + " in length.");

         for (Int32 i = 0; i < m_numInput; i++)
         {
            m_lastInput[i] = input[i];
         }


         // Step1: Compute output of Hidden Layer
         for (Int32 i = 0; i < m_numHidden; i++)
         {
            m_hiddenLayerOutput[i] = 0; // Initialize
            for (Int32 j = 0; j < m_numInput + BiasNeuronCount; j++)
            {
               if (j == m_numInput) // Bias
                  m_hiddenLayerOutput[i] += (m_ih[j, i] * 1.0);
               else
                  m_hiddenLayerOutput[i] += (m_ih[j, i] * input[j]);
            }
            // Now that you summed weights * inputs, squash
            m_hiddenLayerOutput[i] = ActivationFunctions.Tanh(m_hiddenLayerOutput[i]);
         }

         // Step2: Compute output of Output Layer
         for (Int32 i = 0; i < this.m_numOutput; i++)
         {
            m_outputLayerOutput[i] = 0; // Initialize
            for (Int32 j = 0; j < this.m_numHidden + BiasNeuronCount; j++)
            {
               if (j == NumHidden)
                  m_outputLayerOutput[i] += (m_ho[j, i] * 1.0);
               else
                  m_outputLayerOutput[i] += (m_ho[j, i] * m_hiddenLayerOutput[j]);
            }
            // Now that you summed weights * inputs, squash
            m_outputLayerOutput[i] = ActivationFunctions.Tanh(m_outputLayerOutput[i]);
         }
      }

      #endregion

      #region Backpropagation

      /// <summary>
      /// Performs backpropagation.  Assumes that the output was just computed.
      /// </summary>
      /// <remarks>
      /// >http://mattmazur.com/2015/03/17/a-step-by-step-backpropagation-example/ - Great example of backpropagation.
      /// </remarks>
      public void BackwardsPass(Double[] idealOutput, Double learningRate)
      {
         CalculateHiddenToOutputWeightChanges(idealOutput, learningRate);
         CalculateInputToHiddenWeightChanges(learningRate);
         UpdateOutputWeights();
         UpdateHiddenWeights();
      }

      #endregion

      #region Storage / Reading from Storage

      /// <summary>
      /// Outputs the contents of the neural network as a list of Doubles.
      /// </summary>
      /// <returns></returns>
      public List<Double> ToDoubleList()
      {
         List<Double> l = new List<Double>();
         for (Int32 i = 0; i < m_numInput + BiasNeuronCount; i++)
         {
            for (Int32 j = 0; j < m_numHidden; j++)
            {
               l.Add(m_ih[i, j]);
            }
         }
         for (Int32 j = 0; j < m_numHidden + BiasNeuronCount; j++)
         {
            for (Int32 k = 0; k < m_numOutput; k++)
            {
               l.Add(m_ho[j, k]);
            }
         }
         return l;
      }
      /// <summary>
      /// Takes a list of Double to initialize a network.  (Used in conjunction with "ToDoubleList")
      /// </summary>
      public void FromDoubleList(List<Double> l)
      {
         Int32 index = 0;
         for (Int32 i = 0; i < m_numInput + BiasNeuronCount; i++)
         {
            for (Int32 j = 0; j < m_numHidden; j++)
            {
               m_ih[i, j] = l[index];
               index++;
            }
         }
         for (Int32 j = 0; j < m_numHidden + BiasNeuronCount; j++)
         {
            for (Int32 k = 0; k < m_numOutput; k++)
            {
               m_ho[j, k] = l[index];
               index++;
            }
         }
      }

      #endregion

      #endregion

      #region Private Methods

      private void CalculateHiddenToOutputWeightChanges(Double[] idealOutput, Double learningRate)
      {
         m_totalOutputError = 0; // reset total error.
         for (Int32 outputNeuronIndex = 0; outputNeuronIndex < m_numOutput; outputNeuronIndex++)
         {
            // Total Squared Error for each output neuron.
            m_outputSquaredErrors[outputNeuronIndex] = BackpropFunctions.SquaredErrorFunction(idealOutput[outputNeuronIndex], m_outputLayerOutput[outputNeuronIndex]);
            // Total Error of network
            m_totalOutputError += m_outputSquaredErrors[outputNeuronIndex];
            // Partial Derivative for total error with respect to output.  (Derivative of total squared error results in this)
            m_outputPartialDerivTotalErrorOverOutputNeuron[outputNeuronIndex] = m_outputLayerOutput[outputNeuronIndex] - idealOutput[outputNeuronIndex];
            // Partial Derivative of output of a particular neuron respecting its total net input (derivative of activation function)
            m_outputPartialDerivOutputNeuronOverTotalInput[outputNeuronIndex] = ActivationFunctions.TanhDerivative(m_outputLayerOutput[outputNeuronIndex]);

            for (Int32 hiddenNeuronIndex = 0; hiddenNeuronIndex < m_numHidden + BiasNeuronCount; hiddenNeuronIndex++)
            {
               // Partial Derivative of total net input with respect to weight.  Basically, the output of the hidden neuron.
               m_outputPartialDerivTotalInputOverWeight[hiddenNeuronIndex, outputNeuronIndex] = m_hiddenLayerOutput[hiddenNeuronIndex];
               // Multiply the partial derivatives together
               Double deltaOutputNeuron = m_outputPartialDerivTotalErrorOverOutputNeuron[outputNeuronIndex] *
                                          m_outputPartialDerivOutputNeuronOverTotalInput[outputNeuronIndex] *
                                          m_outputPartialDerivTotalInputOverWeight[hiddenNeuronIndex, outputNeuronIndex];
               Double changeInWeight = deltaOutputNeuron * learningRate;
               m_hoWeightChanges[hiddenNeuronIndex, outputNeuronIndex] = changeInWeight;
            }
         }
      }

      private void CalculateInputToHiddenWeightChanges(Double learningRate)
      {
         // We need to figure out the partial derivative of the total error with respect to each weight.
         // This is the same as:
         // Partial Derivative of Total Error with respect to the output of the hidden neuron *
         // (This is equal to the sum of partial derivatives of each output neuron's squared error with respect to the hidden neuron's output)
         // Partial Derivative of the output of the hidden neuron with respect to its net input *
         // Partial Derivative of the Net Input with respect to the weight from a input neuron to the hidden neuron.
         for (Int32 hiddenNeuronIndex = 0; hiddenNeuronIndex < m_numHidden; hiddenNeuronIndex++)
         {
            for (Int32 inputNeuronIndex = 0; inputNeuronIndex < m_numInput + BiasNeuronCount; inputNeuronIndex++)
            {
               m_hiddenPartialDerivTotalErrorOverHiddenOutput[inputNeuronIndex, hiddenNeuronIndex] = 0.0;
               Double runningTotal = 0.0;
               for (Int32 outputNeuronIndex = 0; outputNeuronIndex < m_numOutput; outputNeuronIndex++)
               {
                  runningTotal += m_outputPartialDerivTotalErrorOverOutputNeuron[outputNeuronIndex] *
                                  m_outputPartialDerivOutputNeuronOverTotalInput[outputNeuronIndex] *
                                  m_ho[hiddenNeuronIndex, outputNeuronIndex];
               }
               m_hiddenPartialDerivTotalErrorOverHiddenOutput[inputNeuronIndex, hiddenNeuronIndex] = runningTotal;

               m_hiddenPartialDerivHiddenOutputOverNetInput[inputNeuronIndex, hiddenNeuronIndex] =
                  ActivationFunctions.TanhDerivative(m_hiddenLayerOutput[hiddenNeuronIndex]);

               m_hiddenPartialDerivNetInputOverWeight[inputNeuronIndex, hiddenNeuronIndex] = m_lastInput[inputNeuronIndex];

               m_ihWeightChanges[inputNeuronIndex, hiddenNeuronIndex] = learningRate *
                                                                        m_hiddenPartialDerivTotalErrorOverHiddenOutput[inputNeuronIndex, hiddenNeuronIndex] *
                                                                        m_hiddenPartialDerivHiddenOutputOverNetInput[inputNeuronIndex, hiddenNeuronIndex] *
                                                                        m_hiddenPartialDerivNetInputOverWeight[inputNeuronIndex, hiddenNeuronIndex];
            }
         }
      }

      private void UpdateOutputWeights()
      {
         for (Int32 outputNeuronIndex = 0; outputNeuronIndex < m_numOutput; outputNeuronIndex++)
         {
            for (Int32 hiddenNeuronIndex = 0; hiddenNeuronIndex < m_numHidden + BiasNeuronCount; hiddenNeuronIndex++)
            {
               m_ho[hiddenNeuronIndex, outputNeuronIndex] -= m_hoWeightChanges[hiddenNeuronIndex, outputNeuronIndex];
            }
         }
      }

      private void UpdateHiddenWeights()
      {
         for (Int32 hiddenNeuronIndex = 0; hiddenNeuronIndex < m_numHidden; hiddenNeuronIndex++)
         {
            for (Int32 inputNeuronIndex = 0; inputNeuronIndex < m_numInput + BiasNeuronCount; inputNeuronIndex++)
            {
               m_ih[inputNeuronIndex, hiddenNeuronIndex] -= m_ihWeightChanges[inputNeuronIndex, hiddenNeuronIndex];
            }
         }
      }

      #endregion
   }
}
