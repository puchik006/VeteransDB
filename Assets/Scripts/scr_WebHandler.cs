using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class scr_WebHandler
{
    public IEnumerator FetchAndProcessData<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null,bool useNoCors = false)
    {
        string url = endpoint + "?" + DateTime.Now.Ticks; // Cache-busting
        Debug.Log($"Fetching data from {url}.");

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            if (useNoCors)
            {
                www.SetRequestHeader("mode", "no-cors");
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    T data = JsonUtility.FromJson<T>(www.downloadHandler.text);
                    onSuccess?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing JSON: {ex.Message}");
                    onError?.Invoke("Error processing JSON.");
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch data: {www.error}");
                onError?.Invoke(www.error);
            }
        }
    }

}
