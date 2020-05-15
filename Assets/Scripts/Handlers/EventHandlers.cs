using UnityEngine.Events;

namespace XploriaAR
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }

    [System.Serializable]
    public class LocationEvent : UnityEvent<Location> { }

    [System.Serializable]
    public class NmeaResponseEvent : UnityEvent<NavigationApiResponse> { }
}