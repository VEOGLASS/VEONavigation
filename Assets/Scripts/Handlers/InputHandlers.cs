using UnityEngine;

namespace XploriaAR
{
    public interface IHandleInput
    {
        bool GetKey(KeyCode code);
        bool GetKeyUp(KeyCode code);
        bool GetKeyDown(KeyCode code);

        bool GetMouseButtonDown(int button);

        #region Properties

        bool AnyKey { get; }

        string InputString { get; }

        Compass Compass { get; }
        Vector3 MousePos { get; }
        Vector3 Acceleration { get; }
        Gyroscope Gyroscope { get; }
        LocationService LocationService { get; }

        DeviceOrientation DeviceOrientation { get; }

        #endregion
    }

    public class UnityInputHandler : IHandleInput
    {
        #region Public methods

        public bool GetKey(KeyCode code)
        {
            return Input.GetKey(code);
        }

        public bool GetKeyUp(KeyCode code)
        {
            return Input.GetKeyUp(code);
        }

        public bool GetKeyDown(KeyCode code)
        {
            return Input.GetKeyDown(code);
        }

        public bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        #endregion

        #region Properties

        public bool AnyKey
        {
            get { return Input.anyKey; }
        }

        public string InputString
        {
            get { return Input.inputString; }
        }

        public Compass Compass
        {
            get { return Input.compass; }
        }

        public Vector3 MousePos
        {
            get { return Input.mousePosition; }
        }

        public Vector3 Acceleration
        {
            get { return Input.acceleration; }
        }

        public Gyroscope Gyroscope
        {
            get { return Input.gyro; }
        }

        public LocationService LocationService
        {
            get { return Input.location; }
        }

        public DeviceOrientation DeviceOrientation
        {
            get { return Input.deviceOrientation; }
        }

        #endregion
    }

    public class TestInputHandler : IHandleInput
    {
        public bool GetKey(KeyCode code)
        {
            return true;
        }

        public bool GetKeyUp(KeyCode code)
        {
            return false;
        }

        public bool GetKeyDown(KeyCode code)
        {
            return false;
        }

        public bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        public bool AnyKey
        {
            get;
            private set;
        }

        public string InputString
        {
            get { return Input.inputString; }
        }

        public Compass Compass
        {
            get { return Input.compass; }
        }

        public Vector3 MousePos
        {
            get;
            private set;
        }

        public Vector3 Acceleration
        {
            get { return Vector3.zero; }
        }

        public Gyroscope Gyroscope
        {
            get { return Input.gyro; }
        }

        public LocationService LocationService
        {
            get { return new LocationService(); }//return Input.location; }
        }

        public DeviceOrientation DeviceOrientation
        {
            get { return DeviceOrientation.Portrait; }
        }
    }
}