using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsPanelController : MonoBehaviour {

    public TMP_InputField CarnivorePopulationInput;
    public TMP_InputField CropPopulationInput;
    public TMP_InputField HerbivorePopulationInput;
    public Slider WorldSizeSlider;

    public TMP_InputField CarnivoreLayersInput;
    public TMP_InputField HerbivoreLayersInput;

    private void Start()
    {
        updateInputs();

        CarnivorePopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeCarnivorePopulation);
        CropPopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeCropPopulation);
        HerbivorePopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeHerbivorePopulation);
        WorldSizeSlider.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeWorldSize);
        CarnivoreLayersInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeCarnivoreLayers);
        HerbivoreLayersInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeHerbivoreLayers);
    }

    private void OnEnable()
    {
        if (SimulationSettings.Instance)
        {
            updateInputs();
        }
    }

    private void updateInputs()
    {
        CarnivorePopulationInput.text = SimulationSettings.Instance.CarnivorePopulation.ToString();
        CropPopulationInput.text = SimulationSettings.Instance.CropPopulation.ToString();
        HerbivorePopulationInput.text = SimulationSettings.Instance.HerbivorePopulation.ToString();
        WorldSizeSlider.value = SimulationSettings.Instance.WorldSize;
        WorldSizeSlider.onValueChanged.Invoke(WorldSizeSlider.value);

        Debug.Log(SimulationSettings.Instance.CarnivoreLayers);
        Debug.Log(String.Join(",", SimulationSettings.Instance.CarnivoreLayers));
        CarnivoreLayersInput.text = String.Join(",", SimulationSettings.Instance.CarnivoreLayers);
        HerbivoreLayersInput.text = String.Join(",", SimulationSettings.Instance.HerbivoreLayers);
    }
}
