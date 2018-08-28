using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ValueText
{
    public float value;
    public string text;
}

public class SliderTextController : MonoBehaviour {

    public List<ValueText> values;
    public Text text;

    public void UpdateText(float value)
    {
        string txt = values.Count > 0 ? values[0].text : "";
        for (int i = 0; i < values.Count; ++i)
        {
            if (values[i].value > value)
            {
                break;
            }
            txt = values[i].text;
        }
        text.text = txt;
    }
}
