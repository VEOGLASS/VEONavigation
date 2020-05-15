#define ENABLE_UPDATE_FUNCTION_CALLBACK
#define ENABLE_LATEUPDATE_FUNCTION_CALLBACK
#define ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Main application thread.
/// </summary>
[DisallowMultipleComponent]
public sealed class ThreadManager : MonoBehaviour
{
    #region Static fields

    private static ThreadManager instance = null;

    #endregion

    #region Update fields 

    //holds actions received from another Thread.
    //Will be coped to actionCopiedQueueUpdateFunc then executed from there
    private static List<Action> actionQueuesUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesUpdateFunc to be executed
    List<Action> actionCopiedQueueUpdateFunc = new List<Action>();

    //used to know if whe have new Action function to execute.
    //This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteUpdateFunc = true;

    #endregion

    #region LateUpdate fields 

    //holds actions received from another Thread. Will be coped to 
    //actionCopiedQueueLateUpdateFunc then executed from there
    private static List<System.Action> actionQueuesLateUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesLateUpdateFunc to be executed
    List<Action> actionCopiedQueueLateUpdateFunc = new List<Action>();

    //used to know if whe have new Action function to execute. 
    //This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteLateUpdateFunc = true;

    #endregion

    #region FixedUpdate fields

    //holds actions received from another Thread. Will be coped to 
    //actionCopiedQueueFixedUpdateFunc then executed from there
    private static List<Action> actionQueuesFixedUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesFixedUpdateFunc to be executed
    List<Action> actionCopiedQueueFixedUpdateFunc = new List<Action>();

    //used to know if whe have new Action function to execute. 
    //This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteFixedUpdateFunc = true;

    #endregion

    #region Init and deinit methods

    /// <summary>
    /// Used to initialize UnityThread. 
    /// Call once before any function here.
    /// </summary>
    /// <param name="visible"></param>
    public static void InitUnityThread(bool visible = false)
    {
        if (instance) return;
        
        if (Application.isPlaying)
        {
            //add an invisible game object to the scene
            GameObject obj = new GameObject("MainThreadExecuter");
            if (!visible)
            {
                obj.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<ThreadManager>();
        }
    }

    public void OnDisable()
    {
        if (instance == this) instance = null;      
    }

    #endregion

    #region Update methods

#if ENABLE_UPDATE_FUNCTION_CALLBACK

    private void Update()
    {
        if (noActionQueueToExecuteUpdateFunc)
        {
            return;
        }

        //clear the old actions from the actionCopiedQueueUpdateFunc queue
        actionCopiedQueueUpdateFunc.Clear();
        lock (actionQueuesUpdateFunc)
        {
            //copy actionQueuesUpdateFunc to the actionCopiedQueueUpdateFunc variable
            actionCopiedQueueUpdateFunc.AddRange(actionQueuesUpdateFunc);
            //now clear the actionQueuesUpdateFunc since we've done copying it
            actionQueuesUpdateFunc.Clear();
            noActionQueueToExecuteUpdateFunc = true;
        }

        //loop and execute the functions from the actionCopiedQueueUpdateFunc
        for (int i = 0; i < actionCopiedQueueUpdateFunc.Count; i++)
        {
            actionCopiedQueueUpdateFunc[i].Invoke();
        }
    }

    /// <summary>
    /// Executes co-routine in update function.
    /// </summary>
    /// <param name="action">Action to extecute.</param>
    public static void ExecuteCoroutine(IEnumerator action)
    {
        if (instance)
        {
            ExecuteInUpdate(() => instance.StartCoroutine(action));
        }
    }

    /// <summary>
    /// Executes action in update function.
    /// </summary>
    /// <param name="action">Action to extecute.</param>
    public static void ExecuteInUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesUpdateFunc)
        {
            actionQueuesUpdateFunc.Add(action);
            noActionQueueToExecuteUpdateFunc = false;
        }
    }

#endif

    #endregion

    #region LateUpdate methods

#if ENABLE_LATEUPDATE_FUNCTION_CALLBACK

    public void LateUpdate()
    {
        if (noActionQueueToExecuteLateUpdateFunc)
        {
            return;
        }

        //clear the old actions from the actionCopiedQueueLateUpdateFunc queue
        actionCopiedQueueLateUpdateFunc.Clear();
        lock (actionQueuesLateUpdateFunc)
        {
            //copy actionQueuesLateUpdateFunc to the actionCopiedQueueLateUpdateFunc variable
            actionCopiedQueueLateUpdateFunc.AddRange(actionQueuesLateUpdateFunc);
            //now clear the actionQueuesLateUpdateFunc since we've done copying it
            actionQueuesLateUpdateFunc.Clear();
            noActionQueueToExecuteLateUpdateFunc = true;
        }

        //loop and execute the functions from the actionCopiedQueueLateUpdateFunc
        for (int i = 0; i < actionCopiedQueueLateUpdateFunc.Count; i++)
        {
            actionCopiedQueueLateUpdateFunc[i].Invoke();
        }
    }

    /// <summary>
    /// Executes action in late update function.
    /// </summary>
    /// <param name="action">Action to extecute.</param>
    public static void ExecuteInLateUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesLateUpdateFunc)
        {
            actionQueuesLateUpdateFunc.Add(action);
            noActionQueueToExecuteLateUpdateFunc = false;
        }
    }

#endif

    #endregion

    #region FixedUpdate methods

#if ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK

    public void FixedUpdate()
    {
        if (noActionQueueToExecuteFixedUpdateFunc)
        {
            return;
        }

        //clear the old actions from the actionCopiedQueueFixedUpdateFunc queue
        actionCopiedQueueFixedUpdateFunc.Clear();
        lock (actionQueuesFixedUpdateFunc)
        {
            //copy actionQueuesFixedUpdateFunc to the actionCopiedQueueFixedUpdateFunc variable
            actionCopiedQueueFixedUpdateFunc.AddRange(actionQueuesFixedUpdateFunc);
            //now clear the actionQueuesFixedUpdateFunc since we've done copying it
            actionQueuesFixedUpdateFunc.Clear();
            noActionQueueToExecuteFixedUpdateFunc = true;
        }

        //loop and execute the functions from the actionCopiedQueueFixedUpdateFunc
        for (int i = 0; i < actionCopiedQueueFixedUpdateFunc.Count; i++)
        {
            actionCopiedQueueFixedUpdateFunc[i].Invoke();
        }
    }

    /// <summary>
    /// Executes action in fixed update function.
    /// </summary>
    /// <param name="action">Action to extecute.</param>
    public static void ExecuteInFixedUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesFixedUpdateFunc)
        {
            actionQueuesFixedUpdateFunc.Add(action);
            noActionQueueToExecuteFixedUpdateFunc = false;
        }
    }

#endif

    #endregion
}