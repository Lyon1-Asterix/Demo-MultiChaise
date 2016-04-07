using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

[Serializable]
public struct Joystick
{
    public bool[] buttons;
    public byte[] axes;


    public void SetValues(byte[] b)
    {

        uint offset = 0;
        uint nbButtons = b[0];
        offset++;

        buttons = new bool[nbButtons];

        //on lit le flux octet par octet, et chaque octet contient jusqu'à 8 boutons
        for (int i = 0; i < ((nbButtons + 7) / 8); ++i)
        {
            for (int j = 0; j < 8 && i * 8 + j < nbButtons; ++j)
            {
                buttons[i * 8 + j] = (b[i + 1] & (1 << j)) != 0;
            }

        }
        offset += (nbButtons + 7) / 8;
        uint nbAxes = b[offset];
        axes = new byte[nbAxes];
        offset++;
        for (int i = 0; i < nbAxes; ++i)
        {
            axes[i] = b[i + offset];
        }
        /*
        for (int i = 0; i < nbAxes;++i)
        {
            axes[i] = Convert.ToUInt16(b[2*i + nbButtons + 2] * 256 + b[2*i + nbButtons + 3]);
        }
        //*/
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
    public static byte[] testArray = { 11, 51, 1, 2, 255, 14 };

    [SerializeField]
    private Joystick joystick = new Joystick();
    private JoystickRetriever retriever;// = new JoystickRetriever();

    void Start()
    {
        retriever = new JoystickRetriever();
    }

    void OnDestroy()
    {
        retriever = null;
    }

    void Update()
    {
        joystick.SetValues(retriever.CurrentJoystick);
    }
}

public class JoystickRetriever //client
{
    const string IP_ADDRESS_STRING = "192.168.1.253";
    const int PORT_NUM = 5231;
    IPAddress ipaddr = IPAddress.Parse(IP_ADDRESS_STRING);

    private TcpListener listener;
    private TcpClient client;
    private Thread clientThread;
    private Thread debugServerThread;
    private byte[] currentJoystick;

    public JoystickRetriever()
    {
        LaunchDebugServer();
        client = new TcpClient(IP_ADDRESS_STRING, PORT_NUM);

        clientThread = new Thread(() => RetrieveInput(client, out currentJoystick));
        clientThread.Start();
    }

    public void LaunchDebugServer()
    {
        listener = new TcpListener(ipaddr, PORT_NUM);
        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        listener.Start();

        debugServerThread = new Thread(() => SendInput(listener));
        debugServerThread.Start();
    }

    ~JoystickRetriever()
    {
        Debug.Log("Quitting peacefully...");

        client.Close();
        listener.Stop();

        Debug.Log("Waiting for threads to terminate...");
        clientThread.Join();
        debugServerThread.Join();
        Debug.Log("Quitted peacefully, thank you :)");
    }

    public static void RetrieveInput(TcpClient client, out byte[] currentJoystick)
    {
        NetworkStream clientStream = client.GetStream();
        {
            int bytesToRead = clientStream.ReadByte();
            byte[] byteArray = new byte[bytesToRead];

            int bytesRead = 0;
            while (bytesRead < bytesToRead)
            {
                bytesRead += clientStream.Read(byteArray, bytesRead, bytesToRead - bytesRead);
            }
            currentJoystick = byteArray;
        }
        clientStream.Close();
    }

    public static void SendInput(TcpListener listener)
    {
        try
        {
            Debug.Log("Server listening for a client");
            Socket s = listener.AcceptSocket();
            Debug.Log("Client connected !");

            byte[] toSend = { 11, 31, 3, 2, 0, 48 };

            s.Send(new byte[] { (byte)toSend.Length });
            s.Send(toSend);
            Thread.Sleep(1000);
            s.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Woopsy daisies, problème dans sendinput");
        }
    }

    public byte[] CurrentJoystick
    {
        get
        {
            return currentJoystick;
        }
    }

}
