using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSettings : MonoBehaviour {
    public static SimulationSettings Instance { get; private set; }

    public int CropPopulation;
    public int HerbivorePopulation;
    public int CarnivorePopulation;

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

    public void OnChangeCarnivorePopulation(string pop)
    {
        if (!int.TryParse(pop, out CarnivorePopulation))
        {
            CarnivorePopulation = 0;
        }
        Debug.Log("carn " + CarnivorePopulation);
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
