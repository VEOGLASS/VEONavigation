using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    [Flags]
    public enum XploriaMode
    {
        SailMode = 1,
        LandMode = 2,
    }

    public class ModeManager : MonoBehaviour
    {
        [SerializeField]
        private ViewManager viewManager;

        [SerializeField, ReadOnly, HideLabel]
        private XploriaMode appMode = XploriaMode.SailMode;

        [SerializeField, ReorderableList(ListStyle.Boxed, "View")]
        private List<ViewRenderer> sailViews;

        [OrderedSpace]

        [SerializeField, ReorderableList(ListStyle.Boxed, "View")]
        private List<ViewRenderer> landViews;


        private void Awake()
        {
            if (viewManager) return;

            viewManager = FindObjectOfType<ViewManager>();
        }

        private void Start()
        {
            SetAppMode(appMode);
        }

        private List<ViewRenderer> GetViewsInMode(XploriaMode mode)
        {
            switch (appMode)
            {
                case XploriaMode.SailMode:
                    return sailViews;
                case XploriaMode.LandMode:
                    return landViews;
                default:
                    return null;
            }
        }


        public void AddViewInMode(XploriaMode mode, ViewRenderer view)
        {
            switch (mode)
            {
                case XploriaMode.SailMode:
                    if (sailViews == null) sailViews = new List<ViewRenderer>();
                    sailViews.Add(view);
                    break;
                case XploriaMode.LandMode:
                    if (landViews == null) landViews = new List<ViewRenderer>();
                    landViews.Add(view);
                    break;
            }
        }

        public void RemoveViewInMode(XploriaMode mode, ViewRenderer view)
        {
            switch (mode)
            {
                case XploriaMode.SailMode:
                    sailViews.Remove(view);
                    break;
                case XploriaMode.LandMode:
                    landViews.Remove(view);
                    break;
            }
        }

        public void SetAppMode(XploriaMode mode)
        {
            appMode = mode;
            viewManager?.ResetAllViews(GetViewsInMode(mode));
        }

        public void SetNextMode()
        {
            SetAppMode(appMode.Next());
        }

        public XploriaMode GetAppMode()
        {
            return appMode;
        }


        public ViewManager ViewManager
        {
            get => viewManager;
            set => viewManager = value;
        }
    }
}