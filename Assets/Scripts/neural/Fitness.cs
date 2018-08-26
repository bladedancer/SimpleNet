using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet
{
    public delegate double FitnessFunc(Net net);

    interface Fitness
    {
        double Fitness { get; }
        FitnessFunc FitnessEvaluator { set; }
    }
}
