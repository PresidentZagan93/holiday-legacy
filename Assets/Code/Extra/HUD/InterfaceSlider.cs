using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceSlider : InterfaceObject {

	InterfaceSelectable select;
	public Slider slider;
    [ConfigSetting]
	public string key = "";
    public float rate = 0f;
    public float amount = 0.1f;
    float nextRate;
    
    bool isInteger;
    bool isToggle = false;

    string configType;
	
	protected override void Awake()
	{
        Refresh();
	}

    private void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        slider.onValueChanged.AddListener(ValueChanged);

        if (!key.Contains("+"))
        {
            key = "+";
        }
        configType = key.Split('+')[0];

        if (!select) select = GetComponent<InterfaceSelectable>();

        if (key != "")
        {
            object value = ConfigManager.GetValue(configType, key.Split('+')[1]);
            isInteger = value is int;
            isToggle = value is bool;
            if (isToggle)
            {
                slider.value = (bool)value ? slider.maxValue : 0;
            }
            else
            {
                if (isInteger) slider.value = (int)value;
                else slider.value = (float)value;
            }
        }
        
        slider.wholeNumbers = isInteger || isToggle;
    }

    private void ValueChanged(float arg0)
    {
        if (key != "")
        {
            if (isToggle)
            {
                ConfigManager.SetValue(configType, key.Split('+')[1], (slider.value / slider.maxValue) > 0.5f);
            }
            else
            {
                if(isInteger)
                {
                    ConfigManager.SetValue(configType, key.Split('+')[1], (int)slider.value);
                }
                else
                {
                    ConfigManager.SetValue(configType, key.Split('+')[1], slider.value);
                }
            }
        }
    }

    void FixedUpdate()
	{
		if(select.selected && Time.time > nextRate)
		{
            nextRate = Time.time + rate;

	        bool simLeft = false;
	        bool simRight = false;
	        if (Input.GetAxisRaw("Horizontal2") < 0)
	        {
	        	simLeft = true;
	        }
	        if (Input.GetAxisRaw("Horizontal2") > 0)
	        {
	        	simRight = true;
	        }

            float inc = slider.wholeNumbers ? 1 : amount;
            float dir = 0;
	        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || simLeft)
	        {
	        	dir -= inc;
	        }
	        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || simRight)
	        {
	        	dir += inc;
	        }
	        
			if(isToggle)
			{
				if(dir > 0) slider.value = slider.maxValue;
				if(dir < 0) slider.value = 0f;
			}
			else
			{
				slider.value += dir;
			}
		}
	}
}
