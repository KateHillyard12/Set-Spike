
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

public class ArduinoReceiver : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
    // Start is called before the first frame update
    void Start()
    {
        sp.Open();
        /*
        Set the read timeout low so unity doesn't freeze,
        and catch the exception below in update that unity will throw
        when the port isn't open and unity tries to check it
        */
        sp.ReadTimeout = 1;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (sp.IsOpen)
        {
            try
            {
                int tempInt = int.Parse(sp.ReadLine());
                ButtonPressed(tempInt);
            }
            catch (System.Exception){}
        }
    }

    private void ButtonPressed(int button)
    {
        switch (button)
        {
            case 9: Debug.Log("Centered");
            break;
            case 8: Debug.Log("Jumped");
            break;
        }
    }
}
