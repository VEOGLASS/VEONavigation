using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace XploriaAR.Network
{
    [DisallowMultipleComponent]
    public class ConnectionManager : MonoBehaviour
    {
        private const int maxConnections = 255;

        private Coroutine scanRoutine;      //scan process routine

        private int pingsCount;             //sended pings count          
        private int pongsCount;             //received pongs count

        private int hostNr;                 //ipBase + hostNr == hostIp

        private string ipBase;              //host ip without last numbers

        #region Inspector fields

        [SerializeField, ReadOnly]
        private bool isConnected;           //if connected to network
        [SerializeField, ReadOnly]
        private bool isScanning;            //if currently scanning

        [Space]

        [SerializeField, Tooltip("Timeout in ms.")]
        private int pingTimeout = 8000;     //for UnityEngine.Ping should be > 10000

        [Space]

        [SerializeField, ReadOnly, Tooltip("Last known host IP.")]
        private string hostIp;              //device IP

        [Separator, Space]
                                            //TODO: read only lists
        [SerializeField, ReadOnly, Tooltip("All known devices.")]
        private List<string> availableIps = new List<string>();

        #endregion


        /// <summary>
        /// Inspector reset method.
        /// </summary>
        private void Reset()
        {
            pingTimeout = 8000;
        }

        /// <summary>
        /// Inspector data validation.
        /// </summary>
        private void OnValidate()
        {
            pingTimeout = Mathf.Max(pingTimeout, 1000);
        }

        /// <summary>
        /// Connection init.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            yield return TestInternet();
            yield return InitHost();
            yield return null;

            ScanNetwork();
        }


        #region Private methods

        private IEnumerator TestInternet()
        {
            yield return TestInternet("http://www.google.com");
        }

        private IEnumerator TestInternet(string sampleUrl)
        {
            //creating request to ping
            var internet = UnityWebRequest.Get(sampleUrl);

            yield return internet.SendWebRequest();
            //checking for connection error
            isConnected = internet.error == null;

            if (Application.platform != RuntimePlatform.Android) yield break;

            Android.ShowAndroidToastMessage(isConnected ? "Device connected" : "Device not connected(check Wi-Fi connection)");
        }

        private IEnumerator InitHost()
        {
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);
        
            var ipGroups = new string[0];

            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIp = ip.ToString();
                    ipGroups = hostIp.Split('.');
                    ipBase = string.Join(".", ipGroups, 0, 3) + ".";
                    hostNr = int.Parse(ipGroups[ipGroups.Length - 1]);
                    break;
                }

                yield return null;
            }
        }

        #region UnityEngine.Ping

        /// <summary>
        /// Scan process.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UnityScanNetwork()
        {
            isScanning = true;

            //check connection and own IP
            yield return TestInternet();
            yield return InitHost();

            //stop scan if not connected
            if (!isConnected)
            {
                isScanning = false;
                yield break;
            }

            //sending pings to all potential devices
            for (int i = 0; i < maxConnections; i++)
            {
                if (i != hostNr) SendUnityPing(ipBase + i);
            }

            //waits until all pongs are available
            yield return new WaitWhile(() => pingsCount == 0);
            yield return new WaitWhile(() => pingsCount != pongsCount);

            //reset data after scan
            pingsCount = 0;
            pongsCount = 0;

            isScanning = false;
        }

        /// <summary>
        /// Pings specific IP using <see cref="UnityEngine.Ping"/>.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private IEnumerator UnityPing(string ip)
        {
            pingsCount++;

            var ping = new UnityEngine.Ping(ip);
            var time = 0.0f;

            //wait for response loop
            while (!ping.isDone)
            {
                time += Time.unscaledDeltaTime;
                //check timeout
                if (time > pingTimeout / 1000) break;
                yield return null;
            }

            pongsCount++;
            if (ping.time >= 0)
            {
                //saves correct IP
                availableIps.Add(ip);
                yield break;
            }
        }

        #endregion

        #region System.Net.NetworkInformation.Ping

        private async void ScanAsync()
        {
            isScanning = true;

            var tasksList = new List<Task>();
            for (int i = 0; i < maxConnections; i++)
            {
                tasksList.Add(Task.Factory.StartNew(() => SendPing(ipBase + i)));
                //tasksList.Add(Task.Factory.StartNew(() => SendPingAsync(ipBase + i)));
            }

            try
            {
                await Task.WhenAll(tasksList.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            isScanning = false;
        }

        private void SendPing(string ip)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            var reply = ping.Send(ip, pingTimeout);
           
            if (reply == null || reply.Status != IPStatus.Success) return;
            availableIps.Add(reply.Address.ToString());       
        }

        private void SendPingAsync(string ip)
        {
            var ping = new System.Net.NetworkInformation.Ping();

            ping.PingCompleted += OnPingCompleted;
            ping.SendAsync(ip, pingTimeout, ip);
        }

        private void OnPingCompleted(object sender, PingCompletedEventArgs e)
        {
#if UNITY_EDITOR
            Debug.Log("Received " + e?.Reply.Status);
            Debug.Log(e?.Reply.Status + " " + e?.Reply.Address);
#endif
            if (e.Reply == null || e.Reply.Status != IPStatus.Success) return;

            availableIps.Add(e.Reply.Address.ToString());
        }

        #endregion

        #endregion

        #region Public methods

        public void ScanNetwork()
        {
            if (isScanning) return;

#if UNITY_ANDROID
            scanRoutine = StartCoroutine(UnityScanNetwork());
#else
            //ScanAsync();
            scanRoutine = StartCoroutine(UnityScanNetwork());
#endif
        }

        public void StopScan()
        {
            if (isScanning)
            {
                isScanning = false;
                pingsCount = 0;
                pongsCount = 0;

                if (scanRoutine == null) return;
                StopCoroutine(scanRoutine);
            }
        }

        public void SendUnityPing(string ip)
        {
            StartCoroutine(UnityPing(ip));
        }

        public string GetHostName(string ipAddress)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(ipAddress);
                if (hostEntry != null)
                {
                    return hostEntry.HostName;
                }
            }
            catch (SocketException e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        #endregion


        public bool IsScanning => isScanning;
        public bool IsConnected => isConnected;

        public string HostIp => hostIp;

        public string[] AvailableIps => availableIps.ToArray();
    }
}