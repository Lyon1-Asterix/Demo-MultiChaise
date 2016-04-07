using UnityEngine;
using System.Collections;
using System;
using System.Text;

[Serializable]
public struct Joystick
{
    public bool[] buttons;
    public uint[] axes;

    public Joystick(byte[] b)
    {
        uint nbButtons = b[0];
        buttons = new bool[nbButtons];
        for (int i = 0; i < nbButtons; ++i)
        {
            buttons[i] = Convert.ToBoolean(b[i + 1]);
        }

        uint nbAxes = b[nbButtons + 1];
        axes = new uint[nbAxes];
        for (int i = 0; i < nbAxes;++i)
        {
            axes[i] = Convert.ToUInt16(b[2*i + nbButtons + 2] * 256 + b[2*i + nbButtons + 3]);
        }
    }

    public void Print()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("B : [");
        for (int i = 0; i < buttons.Length - 1; ++i)
        {
            sb.Append(buttons[i]);
            sb.Append(',');
        }
        sb.Append(buttons[buttons.Length - 1]);
        sb.Append("]\tA : [");
        for (int i = 0; i < axes.Length - 1; ++i)
        {
            sb.Append(axes[i]);
            sb.Append(',');
        }
        sb.Append(']');
    }
}

public class JoystickBehaviour : MonoBehaviour
{
    static byte[] testArray = { 3, 0, 1, 1, 2, 0, 14, 10, 10 };

    [SerializeField]
    private Joystick joystick = new Joystick(testArray);
}