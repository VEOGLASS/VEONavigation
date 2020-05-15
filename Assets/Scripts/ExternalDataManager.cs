using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    public class ExternalDataManager : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private NavigationApiResponse lastResponse;

        [SerializeField]
        private NmeaResponseEvent onDataChange;
    
        [SerializeField]
        private LocationEvent onLocationChange;


        public void GetData(string data)
        {
            lastResponse = JsonUtility.FromJson<NavigationApiResponse>(data);
            onDataChange?.Invoke(lastResponse);
            onLocationChange?.Invoke(lastResponse.data.location);
        }

        public void AddOnDataChangeListener(UnityAction<NavigationApiResponse> listener)
        {
            if (onDataChange == null) onDataChange = new NmeaResponseEvent();
            onDataChange.AddListener(listener);
        }

        public void RemoveOnDataChangeListener(UnityAction<NavigationApiResponse> listener)
        {
            onDataChange?.RemoveListener(listener);
        }

        public void AddOnLocationChangeListener(UnityAction<Location> listener)
        {
            if (onLocationChange == null) onLocationChange = new LocationEvent();
            onLocationChange.AddListener(listener);
        }

        public void RemoveOnLocationChangeListener(UnityAction<Location> listener)
        {
            onLocationChange?.RemoveListener(listener);
        }
    }
}