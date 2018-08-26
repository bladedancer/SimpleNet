using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet;
using System;
using System.Linq;

public class MovementNet : MonoBehaviour {
    public int[] hiddenLayers = new int[] { 8, 5 };
    public FeedForward Net
    {
        get;
        private set;
    }
    private MovementController moveCtrl;
    private SensorNet sensors;
    private Stats stats;

    private void Awake()
    {
        moveCtrl = GetComponent<MovementController>();
        sensors = GetComponent<SensorNet>();
        stats = GetComponent<Stats>();
        createNet();
    }
    
    public void TransplantNet(NeuralNet.Net net)
    {
        this.Net = (FeedForward) net;
        // Net fitness is based on amount consumed.
        Net.FitnessEvaluator = n => (double)stats.Consumed;
    }

    private void createNet()
    {
        int[] layers = new int[hiddenLayers.Length + 2];
        layers[0] = sensors.data.Length;
        hiddenLayers.CopyTo(layers, 1);
        layers[layers.Length - 1] = 2; // motion/rotation
        Net = new FeedForward(layers);

        NeuralNet.Mutators.SelfMutate(new NeuralNet.Net[] { Net }, new NeuralNet.Options()
        {
            { "clone", false },
            { "mutationProbability", 1 },
            { "mutationFactor", 1 },
            { "mutationRange", 1000 },
        });

        // Net fitness is based on amount consumed.
        Net.FitnessEvaluator = n => (double)stats.Consumed;
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (Net != null)
        {
            double[] values = Net.eval(sensors.data);
            moveCtrl.SetMovement(
                (float) values[0], // Forward only // (2f * (float)values[0]) - 1,
                (2f * (float)values[1]) - 1);
        }
	}
}
