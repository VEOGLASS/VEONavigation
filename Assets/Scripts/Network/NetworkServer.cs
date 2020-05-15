using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace XploriaAR.Network
{
    public interface IExternalDataReceiver
    {
        StringEvent OnDataReceived { get; }
    }

    [RequireComponent(typeof(ConnectionManager))]
    public class NetworkServer : MonoBehaviour, IExternalDataReceiver
    {
        #region Private fields

        /// <summary> 	
        /// TCPListener to listen for incomming TCP connection requests. 	
        /// </summary> 	
        private TcpListener tcpListener;
        /// <summary> 	
        /// Create handle to connected tcp client. 	
        /// </summary> 	
        private TcpClient tcpClient;

        /// <summary> 
        /// Background thread for TcpServer workload. 	
        /// </summary> 	
        private Thread tcpListenerThread;

        /// <summary>
        /// Base for all network methods.
        /// </summary>
        private ConnectionManager connection;

        #endregion

        #region Inspector fields

        [SerializeField]
        private int tcpPort = 8052;

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

        private void Awake()
        {
            #region Self-Injection

            connection = GetComponent<ConnectionManager>();

            #endregion
        }

        private void Start()
        {
            StartCoroutine(Initialize());
        }

        private void OnDestroy()
        {
            DisposeTcpServer();
        }

        #endregion

        private IEnumerator Initialize()
        {
            //TODO:
            yield return new WaitUntil(() => connection.IsConnected);
            yield return new WaitWhile(() => string.IsNullOrEmpty(connection.HostIp));
            CreateTcpServer();
        }

        /// <summary>
        /// Creates server from scratch.
        /// </summary>
        private void CreateTcpServer()
        {
            tcpListenerThread = new Thread(new ThreadStart(ListenForData))
            {
                IsBackground = true
            };
            tcpListenerThread.Start();
        }

        /// <summary>
        /// Destroys server if possible(if currently connected).
        /// </summary>
        private void DisposeTcpServer()
        {
            tcpListenerThread?.Abort();
            tcpListener?.Stop();
            tcpClient?.Dispose();
        }

        /// <summary> 	
        /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
        /// </summary> 	
        private void ListenForData()
        {
            try
            {			
                tcpListener = new TcpListener(IPAddress.Parse(connection.HostIp), tcpPort);
                tcpListener.Start();

                var  bytes = new Byte[1024];
                while (true)
                {
                    using (tcpClient = tcpListener.AcceptTcpClient())
                    {					
                        using (var stream = tcpClient.GetStream())
                        {
                            var length = 0;						
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);							
                                var clientMessage = Encoding.ASCII.GetString(incommingData);
                                onDataReceived?.Invoke(clientMessage);
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("SocketException " + socketException.ToString());
            }
            catch (ThreadAbortException)
            {
                Debug.Log("Server thread ended.");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary> 	
        /// Send message to client using socket connection. 	
        /// </summary> 	
        public void SendData(string data)
        {
            if (tcpClient == null) return;
            
            try
            {			
                var stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {               
                    var serverMessageBytes = Encoding.ASCII.GetBytes(data);          
                    stream.Write(serverMessageBytes, 0, serverMessageBytes.Length);
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("Socket exception: " + socketException);
            }
        }

        #region Properties

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