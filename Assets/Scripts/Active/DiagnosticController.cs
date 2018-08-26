using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiagnosticController : MonoBehaviour {
    private static DiagnosticController _instance;
    public static DiagnosticController Instance { get { return _instance; } }

    public Text FitnessText;
    public Slider gaugePrefab;
    private SensorNet sensors;
    private NeuralNet.Fitness fitness;
    private List<Slider> gauges;

    public Material debug;
    private Material original;

    public void Awake()
    {
        FitnessText.text = "";
        _instance = this;
        gauges = new List<Slider>();
    }
    
    public void SetSensorNet (SensorNet sensors) {
        if (this.sensors != null)
        {
            this.sensors.GetComponent<MeshRenderer>().material = original;
            this.sensors.OnChangeWeights.RemoveListener(this.OnChangeWeights);
            if (gauges.Count > 0)
            {
                gauges.ForEach(s =>
                {
                    Destroy(s.gameObject);
                });
            }
            gauges.Clear();
        }

        this.sensors = sensors;

        if (this.sensors != null)
        {
            // Create the sliders
            int rows = 30;
            for (int i = 0; i < this.sensors.sensorDataLength; ++i)
            {
                Slider gauge = Instantiate<Slider>(gaugePrefab, this.transform);
                RectTransform gaugeRect = gauge.GetComponent<RectTransform>();
                gaugeRect.localPosition = new Vector3(
                    gaugeRect.localPosition.x + (40 * Mathf.Floor(i / rows)),
                    gaugeRect.localPosition.y - (10 * (i % rows)),
                    gaugeRect.localPosition.z);

                gauges.Add(gauge);
            }
            sensors.OnChangeWeights.AddListener(this.OnChangeWeights);
            fitness = sensors.GetComponent<MovementNet>() ? sensors.GetComponent<MovementNet>().Net : null;
            original = sensors.GetComponent<MeshRenderer>().material;
            sensors.GetComponent<MeshRenderer>().material = debug;
        }
    }
	
	void OnChangeWeights(double[] weights) {
        for (int i = 0; i < weights.Length; ++i)
        {
            gauges[i].value = (float) weights[i];
        }

        if (fitness != null)
        {
            FitnessText.text = fitness.Fitness.ToString();
        }
	}
}
