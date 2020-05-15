using UnityEngine;

namespace XploriaAR
{
    public interface ISceneLoader
    {
        void LoadScene(int index);
        void LoadScene(string name);
        void LoadWithAuthentication(int index);
        void LoadWithAuthentication(string name);
        void Quit();
    }

    public class SceneManager : MonoBehaviour, ISceneLoader
    {
        private void Awake()
        { }

        private void Start()
        { }


        /// <summary>
        /// Loads scene by index.
        /// </summary>
        /// <param name="index"></param>
        public void LoadScene(int index)
        {
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(index);
            }
            catch
            {
#if UNITY_EDITOR
                Debug.LogError("Invalid scene index.", this);
#endif
                return;
            }
        }

        /// <summary>
        /// Loads scene by name.
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadScene(string sceneName)
        {
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            catch
            {
#if UNITY_EDITOR
                Debug.LogError("Invalid scene name.", this);
#endif
                return;
            }
        }

        /// <summary>
        /// Loads scene with (if permission needed) chosen
        /// authentication way.
        /// </summary>
        /// <param name="index"></param>
        public void LoadWithAuthentication(int index)
        {
            throw new System.NotImplementedException();

            //if (!permissionNeeded)
            //{
            //    LoadScene(index);
            //    return;
            //}

            //if (!Connection.CreateSyncLoginRequest(LoginHandler.Email, LoginHandler.Input))
            //{
            //    LoginHandler.IncorrectPopup.Enable();
            //    return;
            //}

            //LoadScene(index);      
        }

        /// <summary>
        /// Loads scene with (if permission needed) chosen
        /// authentication way.
        /// </summary>
        /// <param name="index"></param>
        public void LoadWithAuthentication(string sceneName)
        {
            throw new System.NotImplementedException();

            //if (!permissionNeeded)
            //{
            //    LoadScene(sceneName);
            //    return;
            //}

            //if (!Connection.CreateSyncLoginRequest(LoginHandler.Email, LoginHandler.Input))
            //{
            //    LoginHandler.IncorrectPopup.Enable();
            //    return;
            //}

            //LoadScene(sceneName);           
        }

        /// <summary>
        /// Quits appliaction.
        /// </summary>
        public void Quit()
        {
            //TODO: are you sure?
            Application.Quit();
        }


        /// <summary>
        /// Returns true if provided scene exists.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public static bool SceneExists(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;

            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/");
                var name = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

                if (string.Compare(name, sceneName, true) == 0) return true;
            }

            return false;
        }
    }
}