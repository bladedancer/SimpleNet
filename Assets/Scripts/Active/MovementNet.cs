using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet;
using System;
using System.Linq;

public class MovementNet : MonoBehaviour {
    private int NetOutputCount = 2;  // motion/rotation
    private int NetOutputHistoryDepth = 2;
    private double[] NetOutputHistory;

    public int[] hiddenLayers
    {
        get
        {
            return _hiddenLayers;
        }
        set
        {
            _hiddenLayers = value;
            createNet();
        }
    }

    public int[] _hiddenLayers = new int[] { 8, 5 };
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
        NetOutputHistory = new double[NetOutputCount * NetOutputHistoryDepth];
        Array.Clear(NetOutputHistory, 0, NetOutputHistory.Length);
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
        int[] layers = new int[_hiddenLayers.Length + 2]; // input/output
        layers[0] = sensors.data.Length + (NetOutputHistory.Length);
        _hiddenLayers.CopyTo(layers, 1);
        layers[layers.Length - 1] = NetOutputCount;
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
            double[] values = Net.eval(sensors.data.Concat(NetOutputHistory).ToArray());
            pushHistory(values);
            moveCtrl.SetMovement(
                (float) values[0], // Forward only // (2f * (float)values[0]) - 1,
                (2f * (float)values[1]) - 1);
        }
	}

    void pushHistory(double[] values)
    {
        NetOutputHistory = values.Concat(NetOutputHistory.Take(NetOutputHistory.Length - values.Length)).ToArray();
    }
}
