using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    [DisallowMultipleComponent]
    public sealed class ViewManager : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private ViewRenderer current;
        [SerializeField, ReorderableList(elementLabel: "View")]
        private List<ViewRenderer> views;


        private void Awake()
        { }

        private void Start()
        {
            //ActivateFirst();
            //ValidateActivity();
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            ValidateActivity();
            yield return null;
            ActivateFirst();
        }

        private void ValidateActivity()
        {
            views?.ForEach(view =>
            {
                if (view.IsActive && view != current)
                {
                    view.SetActive(false);
                }
            });
        }

        private void ActivateView(ViewRenderer view)
        {
            current?.SetActive(false);
            current = view;
            current?.SetActive(true);
        }

        #region Public methods

        public void AddView(ViewRenderer view)
        {
            if (views == null) views = new List<ViewRenderer>();
            views.Add(view);
        }

        public void RemoveView(ViewRenderer view)
        {
            views?.Remove(view);
        }

        public void ResetAllViews(List<ViewRenderer> newViews)
        {
            ClearAllViews();
            if (views == null)
            {
                views = new List<ViewRenderer>();
            }
            views.AddRange(newViews);
            Start();
        }

        public void ClearAllViews()
        {
            current = null;
            ValidateActivity();
            views.Clear();
        }

        public void ActivateNextView()
        {
            if (views == null || views.Count <= 1) return;
            var index = current ? views.IndexOf(current) + 1 : 1;
            if (index == views.Count)
            { 
                index = 0;
            }

            ActivateAt(index);
        }

        public void ActivateAt(int index)
        {
            ActivateView(views[index]);
        }

        public void ActivateFirst()
        {
            var first = views.First();
            if (first) ActivateView(first);
        }

        #endregion

        public float ViewsCount => views.Count;
    }
}