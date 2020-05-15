using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace XploriaAR.Network
{
    public class NetworkClient : MonoBehaviour
    {
        #region Private fields

        private TcpClient socketConnection;
        private Thread clientReceiveThread;

        #endregion

        #region Inspector fields

        [SerializeField]
        private int tcpPort = 8052;

        [SerializeField]
        private string connectionIp = "127.0.0.1";

        [SerializeField]
        private StringEvent onDataReceived;

        #endregion

        #region Editor methods

        private void OnValidate()
        {
            tcpPort = Mathf.Max(tcpPort, 0);
        }

        #endregion

        #region Init methods

        private void Start()
        {
            ConnectToTcpServer();
        }

        private void OnDestroy()
        {
            DisconnectFromTcpServer();
        }

        #endregion

        /// <summary> 	
        /// Setup socket connection. 	
        /// </summary> 	
        private void ConnectToTcpServer()
        {
            try
            {
                clientReceiveThread = new Thread(new ThreadStart(ListenForData))
                {
                    IsBackground = true
                };
                clientReceiveThread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError("On client connect exception " + e);
            }
        }

        /// <summary>
        /// Disconnects from current connected server(if possible).
        /// </summary>
        private void DisconnectFromTcpServer()
        {
            clientReceiveThread?.Abort();
            socketConnection?.Dispose();
        }

        /// <summary> 	
        /// Runs in background clientReceiveThread; Listens for incomming data. 	
        /// </summary>     
        private void ListenForData()
        {
            ListenForData(connectionIp);
        }

        /// <summary> 	
        /// Runs in background clientReceiveThread; Listens for incomming data. 	
        /// </summary>  
        private void ListenForData(string ip)
        {
            try
            {
                socketConnection = new TcpClient(ip, tcpPort);
                var bytes = new Byte[1024];
                while (true)
                {
                    using (var stream = socketConnection.GetStream())
                    {
                        var length = 0;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            var serverMessage = Encoding.ASCII.GetString(incommingData);
                            onDataReceived?.Invoke(serverMessage);
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("Socket exception: " + socketException);
            }
        }

        /// <summary> 	
        /// Send message to server using socket connection. 	
        /// </summary> 	
        private void SendData(string data)
        {
            if (socketConnection == null)
            {
                return;
            }

            try
            {		
                var stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {               
                    var clientMessageAsByteArray = Encoding.ASCII.GetBytes(data);               
                    stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                    Debug.Log("Client sent his message - should be received by server");
                }
            }
            catch (SocketException socketException)
            {
#if UNITY_EDITOR
                Debug.LogError("Socket exception: " + socketException);
#endif
                Android.ShowAndroidToastMessage(socketException.ToString(), 20000);
            }
        }

        #region Properties

        public string ConnectionIp
        {
            get { return connectionIp; }
            set
            {
                DisconnectFromTcpServer();
                connectionIp = value;
                ConnectToTcpServer();
            }
        }

        public StringEvent OnDataReceived
        {
            get
            {
                if (onDataReceived == null)
                {
                    onDataReceived = new StringEvent();
                }
                return onDataReceived;
            }
        }

        #endregion
    }
}