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

    private void Start()
    {
        updateInputs();

        CarnivorePopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeCarnivorePopulation);
        CropPopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeCropPopulation);
        HerbivorePopulationInput.onValueChanged.AddListener(SimulationSettings.Instance.OnChangeHerbivorePopulation);
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
    }
}
