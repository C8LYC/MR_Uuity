using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = true;
    private Action<string> callback; // Callback to process received data

    public UDPReceiver(int port, Action<string> callback)
    {
        this.callback = callback;
        udpClient = new UdpClient(port);

        // Start a new thread for receiving data
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref endPoint); // Blocking call
                string message = Encoding.UTF8.GetString(data);

                // Invoke the callback to handle the message
                callback?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"UDP Receive Error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        isRunning = false;
        udpClient.Close();
        if (receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }
}
