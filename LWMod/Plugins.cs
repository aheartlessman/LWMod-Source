using System.Reflection;
using System;
using UnityEngine;
using System.Timers;
using Network;
using Object = UnityEngine.Object;

namespace LWMod.Plugins
{
    public class AfterMapClear : MonoBehaviour
    {
        private bool mapCleared = false;
        private readonly string[] objectsToRemove = { "fields-", "beach-", "forest-", "shore" };
        private void Update(BasePlayer player)
        {
            if (player.IsLocalPlayer() != null)
            {
                if (!mapCleared)
                {
                    mapCleared = true;
                    ClearMap();
                }
            }
            else
            {
                mapCleared = false;
            }
        }

        private void ClearMap()
        {
            GameObject[] gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.name == "assets/bundled/prefabs/world/decor.prefab")
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
                else if (World.Seed >= 1000000U)
                {
                    foreach (string objectNamePart in objectsToRemove)
                    {
                        if (gameObject.name.Contains(objectNamePart))
                        {
                            UnityEngine.Object.Destroy(gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }

    public class FileCheck : MonoBehaviour
    {
        private Timer checkResolutionTimer;

        private void Awake()
        {
            checkResolutionTimer = Functions.TimeoutRepeat(CheckResolution, 5000);
        }

        private void CheckResolution()
        {
            if (Net.cl.IsConnected() && Screen.width / (float)Screen.height > 2)
            {
                ConsoleNetwork.ClientRunOnServer("razmerokna");
            }
        }


       

        private void OnDestroy()
        {
            checkResolutionTimer?.Stop();
            checkResolutionTimer?.Dispose();
        }

    }
    public static class GameStarter 
    {
        public static GameObject go = new GameObject();

        public static void OnGameStarting()
        {
            go.AddComponent<FileCheck>();
            Object.DontDestroyOnLoad(go);
        }
    }
}