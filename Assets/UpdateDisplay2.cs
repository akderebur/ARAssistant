using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI display;
    Scrollbar scrollbar;

    private void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    private void Update()
    {
        UpdateValue(scrollbar.value);
    }

    public void UpdateValue(float sliderVal)
    {
        display.text = Mathf.FloorToInt(sliderVal * 60f).ToString();
    }
}
