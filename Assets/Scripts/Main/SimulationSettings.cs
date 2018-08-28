using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSettings : MonoBehaviour {
    public static SimulationSettings Instance { get; private set; }

    public int CropPopulation;
    public int HerbivorePopulation;
    public int CarnivorePopulation;
    public int WorldSize;
    public int[] HerbivoreLayers = new int[0];
    public int[] CarnivoreLayers = new int[0];

    public void OnChangeCropPopulation(string pop)
    {
        if (!int.TryParse(pop, out CropPopulation))
        {
            CropPopulation = 0;
        }
        Debug.Log("crop " + CropPopulation);
    }

    public void OnChangeHerbivorePopulation(string pop)
    {
        if (!int.TryParse(pop, out HerbivorePopulation))
        {
            HerbivorePopulation = 0;
        }
        Debug.Log("herb " + HerbivorePopulation);
    }

    public void OnChangeHerbivoreLayers(string layers)
    {
        try
        {
            HerbivoreLayers = new List<int>(Array.ConvertAll(layers.Split(','), int.Parse)).ToArray();
            Debug.Log("herbLayers " + String.Join(",", HerbivoreLayers));
        }
        catch (FormatException ex)
        {
            HerbivoreLayers = new int[0];
        }
    }

    public void OnChangeCarnivorePopulation(string pop)
    {
        if (!int.TryParse(pop, out CarnivorePopulation))
        {
            CarnivorePopulation = 0;
        }
        Debug.Log("carn " + CarnivorePopulation);
    }

    public void OnChangeCarnivoreLayers(string layers)
    {
        try {
            CarnivoreLayers = new List<int>(Array.ConvertAll(layers.Split(','), int.Parse)).ToArray();
            Debug.Log("carnLayers " + String.Join(",", CarnivoreLayers));
        } catch (FormatException ex) {
            CarnivoreLayers = new int[0];
        }
    }

    public void OnChangeWorldSize(float size)
    {
        WorldSize = (int) size;
        Debug.Log("world " + WorldSize);
    }

    private void Awake()
    {
        if (!SimulationSettings.Instance)
        {
            DontDestroyOnLoad(this.gameObject);
            Instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }
}
