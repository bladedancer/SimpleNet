using System;
using System.Collections.Generic;

namespace NeuralNet
{
    public class Options : Dictionary<string, object> { };
    public delegate Net MutatorFunc(Net[] nets, Options options = null);

    public class Mutators
    {
        private static Random random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Interleaves the weights from the nets, round robins.
        /// No options
        /// </summary>
        public static MutatorFunc InterLeaved = (nets, options) =>
        {
            if (nets.Length == 0)
            {
                return null;
            }
            Net child = nets[0].Clone();

            if (nets.Length >= 2)
            {
                for (int i = 1; i < nets[0].weights.Length; i += nets.Length)
                {
                    for (int j = 0; j < nets.Length - 1; ++j)
                    {
                        child.weights[i + j] = nets[j + 1].weights[i + j];
                    }
                }
            }
            return child;
        };

        /// <summary>
        /// Takes alternate layers from the nets.
        /// No options
        /// </summary>
        public static MutatorFunc LayerCake = (nets, options) =>
        {
            if (nets.Length == 0)
            {
                return null;
            }
            Net child = nets[0].Clone();

            if (!(child is FeedForward))
            {
                throw new Exception("LayerCake only works for FeedForward");
            }

            FeedForward ffchild = child as FeedForward;

            if (nets.Length >= 2)
            {
                int offset = 0;
                int selected = 0;
                for (int i = 1; i < ffchild.layerSizes.Length; ++i)
                {
                    int weightCount = ffchild.layerSizes[i] * (ffchild.layerSizes[i - 1] + 1 /*bias node*/);
                    Array.Copy(nets[selected].weights, offset, child.weights, offset, weightCount);
                    selected = (selected + 1) % nets.Length;
                    offset += weightCount;
                }
            }
            return child;
        };

        /// <summary>
        /// Takes a single net and mutates it.
        /// Options:
        /// cloneNet: If true a copy is made and it is mutated.
        /// mutationProbability: 0-1
        /// mutationFactor: n (multiplier)
        /// mutationRange: +-n (mutation if weight is 0) 
        /// </summary>
        public static MutatorFunc SelfMutate = (nets, options) =>
        {
            double mutationProbability = 0;
            if (options.ContainsKey("mutationProbability"))
            {
                mutationProbability = Convert.ToDouble(options["mutationProbability"]);
            }

            double mutationFactor = 0;
            if (options.ContainsKey("mutationFactor"))
            {
                mutationFactor = Convert.ToDouble(options["mutationFactor"]);
            }

            double mutationRange = 0;
            if (options.ContainsKey("mutationRange"))
            {
                mutationRange = Convert.ToDouble(options["mutationRange"]);
            }

            bool clone = false;
            if (options.ContainsKey("clone"))
            {
                clone = Convert.ToBoolean(options["clone"]);
            }

            Net mutant = nets[0];

            if (clone)
            {
                mutant = mutant.Clone();
            }

            for (int i = 0; i < mutant.weights.Length; ++i)
            {
                if (random.NextDouble() < mutationProbability)
                {
                    if (Math.Abs(mutant.weights[i]) < 0.00000000001)
                    {
                        mutant.weights[i] = ((random.NextDouble() * 2) - 1) * mutationRange;
                    }
                    else
                    {
                        double multiplier = ((random.NextDouble() * 2) - 1) * mutationFactor;
                        mutant.weights[i] = mutant.weights[i] + (mutant.weights[i] * multiplier);
                    }
                }
            }
            return mutant;
        };

        /// <summary>
        /// Randomly mixes the nets.
        /// </summary>
        public static MutatorFunc RandomMix = (nets, options) =>
        {
            bool clone = false;
            if (options != null && options.ContainsKey("clone"))
            {
                clone = Convert.ToBoolean(options["clone"]);
            }

            Net mutant = nets[0];

            if (clone)
            {
                mutant = mutant.Clone();
            }

            if (nets.Length > 1)
            {
                for (int i = 0; i < mutant.weights.Length; ++i)
                {
                    mutant.weights[i] = nets[(int) Math.Floor(random.NextDouble() * nets.Length)].weights[i];
                }
            }
            return mutant;
        };
    }
}
