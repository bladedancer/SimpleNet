using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StatCanvasController : MonoBehaviour {

    public Text statText;
    public Slider statSlider;

    public void ChangeText(string text)
    {
        statText.text = text;
    }

    public void ChangeValue(float value)
    {
        statSlider.value = value;
    }
}
