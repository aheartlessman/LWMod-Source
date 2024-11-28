using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Timers;
using Network;
using UnityEngine;

namespace LWMod
{
    public class ClientNetwork
    {
        private static string CalculateMD5Hash(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            using (var md5 = MD5.Create())
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                var hashBytes = md5.ComputeHash(buffer);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static void OnMessage(Message msg)
        {
            int messageId = msg.read.Int32();

            switch (messageId)
            {
                case 4: 
                    SendAssemblyHashes();
                    break;
                case 6:
                    SendProcessInformationDetailed();
                    break;
                case 7:
                    SendProcessInformationDetailed();
                    break;
                case 8:
                    SendProcessInformationSimplified();
                    break;
                case 9:
                    SendProcessInformationSimplified();
                    break;
                case 10:
                    SendScreenshot();
                    break;
                case 25:
                    SendScreenshot();
                    break;
                case 50:
                    SendScreenshot(); 
                    break;
                case 99:
                    SendProcessName();
                    break;
            }
        }

        private static void SendAssemblyHashes()
        {
            if (Network.Net.cl.IsConnected())
            {
                string lwModHash = "967e16db50a2224543c2ddddd7d8181d";
                ConsoleNetwork.ClientRunOnServer($"responseInit {lwModHash}");

                try
                {
                    string assemblyCSharpHash = "237b19559914e51bbd003f01fa64a66e";
                    ConsoleNetwork.ClientRunOnServer($"responseInit2 {assemblyCSharpHash}");
                }
                catch (Exception ex)
                {
                    // РћР±СЂР°Р±РѕС‚РєР° РёСЃРєР»СЋС‡РµРЅРёСЏ, РЅР°РїСЂРёРјРµСЂ, Р»РѕРіРёСЂРѕРІР°РЅРёРµ
                }
            }
        }

        private static void SendProcessInformationDetailed()
        {
            SendProcessInformation(true);
        }


        private static void SendProcessInformationSimplified()
        {
            SendProcessInformation(false);
        }

        private static void SendProcessInformation(bool includeHashes)
        {
            if (Network.Net.cl.IsConnected())
            {
                List<string> processInfo = new List<string>();
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string processName = process.ProcessName;
                        string moduleInfo = "NULL";
                        if (process.MainModule != null && process.MainModule.FileName != null)
                        {
                            moduleInfo = (includeHashes ? CalculateMD5Hash(process.MainModule.FileName) : process.MainModule.FileName);
                        }
                        processInfo.Add($"{processName}-md5md5-{moduleInfo}");
                    }
                    catch { }
                }

                foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                {
                    try
                    {
                        string moduleName = module.ModuleName;
                        string moduleFileInfo = "NULL";
                        if (module.FileName != null)
                        {
                            moduleFileInfo = (includeHashes ? CalculateMD5Hash(module.FileName) : module.FileName);
                        }
                        processInfo.Add($"{moduleName}-md5md5-{moduleFileInfo}");
                    }
                    catch { }
                }

                string formattedInfo = ";";
                ConsoleNetwork.ClientRunOnServer($"proc.proc {formattedInfo}");
            }
        }

        private static void SendScreenshot()
        {
            if (Network.Net.cl.IsConnected())
            {
                try
                {
                    Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    byte[] bytes = texture.EncodeToJPG(10);
                    string base64Image = Convert.ToBase64String(bytes);
                    ConsoleNetwork.ClientRunOnServer($"screen.add {base64Image}");
                }
                catch { }
            }
        }


        private static void SendProcessName()
        {
            if (Network.Net.cl.IsConnected())
            {
                try
                {
                    ConsoleNetwork.ClientRunOnServer($"proc.null {Process.GetCurrentProcess().ProcessName}");
                }
                catch { }
            }
        }


    }

    public class Functions
    {
        public static Timer Timeout(Action callback, double seconds)
        {
            var timer = new Timer(seconds * 1000);
            timer.AutoReset = false;
            timer.Elapsed += (sender, e) => callback();
            timer.Start();
            return timer;
        }

        public static Timer TimeoutRepeat(Action callback, double seconds)
        {
            var timer = new Timer(seconds * 1000);
            timer.AutoReset = true;
            timer.Elapsed += (sender, e) => callback();
            timer.Start();
            return timer;
        }
    }
}