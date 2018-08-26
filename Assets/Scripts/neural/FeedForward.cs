using System;
using System.Collections.Generic;

namespace NeuralNet
{
    public interface Net
    {
        // Unique Net Id
        string id { get; }

        /// <summary>
        /// Evaluate the neural network with the given inputs.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns>The outputs from the neural network.</returns>
        double[] eval(double[] inputs);

        /// <summary>
        /// The weights of the neural network;
        /// </summary>
        double[] weights { get; }

        /// <summary>
        /// Create a copy of the net.
        /// </summary>
        /// <returns></returns>
        Net Clone();
    }

    /// <summary>
    /// Activation functions.
    /// </summary>
    public class Activation
    {
        public delegate double func(double i);

        /// <summary>
        /// The sigmoid activation function with values from 0 to 1.
        /// </summary>
        /// <param name="v">The value to convert.</param>
        /// <returns>The resulting value.</returns>
        public static double sigmoid(double v)
        {
            return (1 / (1 + Math.Pow(Math.E, -v)));
        }

        /// <summary>
        /// The tanh activation function with values from -1 to 1.
        /// </summary>
        /// <param name="v">The value to convert.</param>
        /// <returns>The resulting value.</returns>
        public static double tanh(double v)
        {
            return Math.Tanh(v);
        }
    }

    /// <summary>
    /// This is a feed forward fully connected neural network.
    /// </summary>
    [Serializable]
    public class FeedForward : Net, Fitness
    {
        public string id { get; private set; }

        /// <summary>
        /// The weights of the neural network;
        /// </summary>
        public double[] weights { get; private set; }

        /// <summary>
        /// The activation function of the network. Defaults to Activcation.sigmoid.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Activation.func activationFunc { get; set; }

        private FitnessFunc func { get; set; }

        /// <summary>
        /// The array of layer sizes. The first layer is the input and the last layer is the output.
        /// </summary>
        public int[] layerSizes { get; private set; }
        
        /// <summary>
        /// The fitness evaluator delegate property.
        /// </summary>
        public FitnessFunc FitnessEvaluator { set; private get; }

        /// <summary>
        /// Return the fitness for this net
        /// </summary>
        public double Fitness
        {
            get
            {
                return FitnessEvaluator != null ? FitnessEvaluator(this) : 0;
            }
        }

        /// <summary>
        /// The bias value.
        /// </summary>
        private float bias = 1;

        /// <summary>
        /// Clone the net.
        /// </summary>
        /// <returns></returns>
        public Net Clone()
        {
            FeedForward clone = (FeedForward)this.MemberwiseClone();
            clone.weights = new double[this.weights.Length];
            Array.Copy(this.weights, clone.weights, this.weights.Length);
            return clone;
        }

        /// <summary>
        /// For serialization
        /// </summary>
        public FeedForward()
        {
            this.activationFunc = Activation.sigmoid;
        }

        /// <summary>
        /// Create a network with the specified number of layers.
        /// </summary>
        /// <param name="layerSizes">The layer setup.</param>
        /// /// <param name="weigths">The weights to use for the network.</param>
        public FeedForward(int[] layerSizes, double[] initWeights = null)
        {
            this.id = Guid.NewGuid().ToString();
            this.layerSizes = layerSizes;
            this.activationFunc = Activation.sigmoid;

            int totalWeights = 0;
            for (int i = 1; i < layerSizes.Length; ++i)
            {
                totalWeights += layerSizes[i] * (layerSizes[i - 1] + 1 /*bias node*/);
            }

            this.weights = new double[totalWeights];

            if (initWeights != null)
            {
                if (initWeights.Length != totalWeights)
                {
                    throw new Exception("Expected " + totalWeights + " weights. Got " + initWeights.Length);
                }
                for (int i = 0; i < totalWeights; ++i)
                {
                    this.weights[i] = initWeights[i];
                }
            }
        }

        /// <summary>
        /// Evaluate the neural network with the given inputs.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns>The outputs from the neural network.</returns>
        public double[] eval(double[] inputs)
        {
            if (inputs.Length != layerSizes[0])
            {
                throw new Exception("Invalid input count. Expected " + layerSizes[0] + " but got " + inputs.Length);
            }

            List<double> layerVals = new List<double>();
            layerVals.AddRange(inputs);

            // TODO GET A BETTER WAY OF DOING THIS, LOOK AT THE MATRIX
            int w = 0;
            for (int layer = 1; layer < layerSizes.Length; ++layer)
            {
                layerVals.Add(bias);
                double[] nextVals = new double[layerSizes[layer]];

                // All the weights by all the previous layer weights plus bias.
                for (int node = 0; node < layerVals.Count; ++node)
                {
                    for (int resultNode = 0; resultNode < layerSizes[layer]; ++resultNode)
                    {
                        nextVals[resultNode] = nextVals[resultNode] + (layerVals[node] * weights[w++]);
                    }
                }

                for (int i = 0; i < nextVals.Length; ++i)
                {
                    nextVals[i] = activationFunc(nextVals[i]);
                }

                // Update the inputs
                layerVals.Clear();
                layerVals.AddRange(nextVals);

            }

            return layerVals.ToArray();
        }

        public void SetFitnessFunc(FitnessFunc evaluator)
        {
            throw new NotImplementedException();
        }
    }
}