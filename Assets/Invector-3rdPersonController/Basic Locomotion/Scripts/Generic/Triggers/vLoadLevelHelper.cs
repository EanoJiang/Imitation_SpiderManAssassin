using Invector.vCharacterController;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invector.Utils
{
    public interface vISceneLoadListener
    {
        void OnStartLoadScene(string sceneName);
        void OnFinishLoadScene(string sceneName);
    }

    public static class LoadLevelHelper
    {
        public static vThirdPersonInput targetCharacter;
        public static string spawnPointName;
        public static string sceneName;
        public static bool isLoading;
        public static void LoadScene(string _sceneName, string _spawnPointName, vThirdPersonInput tpInput)
        {
            if (!tpInput) return;
            targetCharacter = tpInput;
            spawnPointName = _spawnPointName;
            sceneName = _sceneName;

            if (targetCharacter.tpCamera)
            {
                targetCharacter.tpCamera.transform.parent = targetCharacter.transform;
            }

            var listeners = targetCharacter.GetComponents<vISceneLoadListener>();
            foreach (var listener in listeners)
            {
                listener.OnStartLoadScene(_sceneName);
            }

            targetCharacter.StartCoroutine(LoadAsyncScene());
        }

        static IEnumerator LoadAsyncScene()
        {           
            // Ensure targetCharacter is still valid
            if (targetCharacter == null|| isLoading) yield break;

            isLoading = true;
            Scene currentScene = SceneManager.GetActiveScene();
            if (!currentScene.name.Equals(sceneName))
            {
                SceneManager.sceneUnloaded += OnSceneUnloaded;

                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                SceneManager.MoveGameObjectToScene(targetCharacter.gameObject, SceneManager.GetSceneByName(sceneName));
                SceneManager.UnloadSceneAsync(currentScene);
            }
            else
            {
                MoveCharacterToSpawnPoint();
            }
            isLoading = false;
        }

        static void OnSceneUnloaded(Scene unloadedScene)
        {
            var listeners = targetCharacter.GetComponents<vISceneLoadListener>();
            foreach (var listener in listeners)
            {
                listener.OnFinishLoadScene(unloadedScene.name);
            }
            MoveCharacterToSpawnPoint();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        static void MoveCharacterToSpawnPoint()
        {
            var spawnPoint = GameObject.Find(spawnPointName);

            if (spawnPoint && targetCharacter)
            {
                targetCharacter.lockCameraInput = true;

                if (targetCharacter.tpCamera)
                {
                    targetCharacter.tpCamera.FreezeCamera();
                }

                targetCharacter.transform.position = spawnPoint.transform.position;
                targetCharacter.transform.rotation = spawnPoint.transform.rotation;

                if (targetCharacter.tpCamera)
                {
                    targetCharacter.tpCamera.transform.parent = null;
                    targetCharacter.tpCamera.UnFreezeCamera();
                }

                targetCharacter.lockCameraInput = false;
            }
        }
    }
}
