using UnityEngine;
using UnityEngine.Events;

namespace XploriaAR
{
    [DisallowMultipleComponent, SelectionBase]
    [RequireComponent(typeof(Animator))]
    public class ViewRenderer : MonoBehaviour
    {
        /// <summary>
        /// Associated and required <see cref="Animator"/> component.
        /// </summary>
        private Animator animator;

        [SerializeField, ReadOnly]
        private bool isActive = true;

        [Space]

        [SerializeField]
        private BoolEvent onIsActiveChange;

        [Space]

        [SerializeField]
        private string upStateName = "ShowView";
        [SerializeField]
        private string downStateName = "HideView";

        private void Awake()
        {
            /*
             * Self-Injection
             */
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            SetActive(IsActive);
        }

        public void SetActive(bool value)
        {
            IsActive = value;
        }

        public void AddOnActivityChangeListener(UnityAction<bool> listener)
        {
            if (onIsActiveChange == null) onIsActiveChange = new BoolEvent();
            onIsActiveChange.AddListener(listener);
        }

        public void RemoveOnActivityChangeListener(UnityAction<bool> listener)
        {
            onIsActiveChange?.RemoveListener(listener);
        }

        public bool IsActive
        {
            get { return isActive; }
            private set
            {
                isActive = value;
                onIsActiveChange?.Invoke(value);
                animator.Play(value ? upStateName : downStateName);
            }
        }

        public string UpStateName
        {
            get => upStateName;
            set => upStateName = value;
        }

        public string DownStateName
        {
            get => downStateName;
            set => downStateName = value;
        }
    }
}