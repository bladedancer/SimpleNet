using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ChangeWeightsEvent : UnityEvent<double[]>
{
}

[System.Serializable]
public class DistanceSensor
{
    public string[] layers;
    public float maxDistance;
    public float angle;
    public Vector3 extents = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 origin = new Vector3(0, 0, 0);

    [System.NonSerialized]
    public int layerMask;

    [System.NonSerialized]
    public Vector3 direction;
}

[Serializable]
public class Threat {
    public string tag;
    public double level;
}

public class SensorNet : MonoBehaviour {
    public List<Threat> threats = new List<Threat>();
    private Dictionary<string, double> ThreatDictionary = new Dictionary<string, double>();

    public ChangeWeightsEvent OnChangeWeights;
    public int sensorDataLength { get; private set; }
    public DistanceSensor[] distanceSensors = new DistanceSensor[0];
    public double[] data;
    private Stats stats;

    void Awake () {
        if (OnChangeWeights == null)
        {
            OnChangeWeights = new ChangeWeightsEvent();
        }

        stats = GetComponent<Stats>();
        sensorDataLength = (distanceSensors.Length * 3); // (Distance, tag, nutrition)
        data = new double[sensorDataLength];

        threats.ForEach(threat =>
        {
            ThreatDictionary.Add(threat.tag, threat.level);
        });

        for (int i = 0; i < distanceSensors.Length; ++i)
        {
            DistanceSensor sensor = distanceSensors[i];
            sensor.layerMask = LayerMask.GetMask(sensor.layers);
            Quaternion rot = Quaternion.AngleAxis(sensor.angle, Vector3.up);
            sensor.direction = (rot * Vector3.forward).normalized;
        }
    }

    void FixedUpdate()
    {
        // Distance Sensors
        for (int i = 0; i < distanceSensors.Length; ++i)
        {
            DistanceSensor sensor = distanceSensors[i];
            Vector3 dir = transform.TransformDirection(new Vector3(sensor.direction.x, 0, sensor.direction.z)) * sensor.maxDistance;
            RaycastHit hit;
            Vector3 raySrc = transform.position + (transform.right * sensor.origin.x) + (transform.forward * sensor.origin.z) + (transform.up * sensor.origin.y);

            if (Physics.BoxCast(raySrc, sensor.extents, dir, out hit, Quaternion.identity, sensor.maxDistance, sensor.layerMask))
            {
                data[i * 2] = 1 - (hit.distance / sensor.maxDistance);
                data[(i * 2) + 1] = ThreatDictionary.ContainsKey(hit.transform.tag) ? ThreatDictionary[hit.transform.tag] : 0;

                Stats hitStats = hit.transform.GetComponent<Stats>();
                if (hitStats != null && Array.IndexOf(stats.Menu, hit.transform.tag) > -1)
                {
                    data[(i * 2) + 2] = Mathf.Clamp(hitStats.Nutrition / stats.MaxHealth, 0, 1);
                }
                else
                {
                    data[(i * 2) + 2] = 0;
                }

                Color col = Color.HSVToRGB((float) data[(i * 2) + 1] * 0.75f, 1f, 0.5f);
                Debug.DrawRay(raySrc, hit.point - raySrc, col);
            }
            else
            {
                data[i * 2] = 0;
                data[(i * 2) + 1] = 0;
                data[(i * 2) + 2] = 0;
            }
        }

        OnChangeWeights.Invoke(data);
    }
}
