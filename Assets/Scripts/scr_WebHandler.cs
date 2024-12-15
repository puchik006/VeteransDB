using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class scr_WebHandler
{
    public IEnumerator IE_Get(string url, Action<string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("_Successfully fetched JSON: " + www.downloadHandler.text);
                //existingJson = www.downloadHandler.text;  // Store the existing data
                callback(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Failed to fetch JSON from server: {www.error}");
                Debug.LogError("Full response: " + www.downloadHandler.text);
                yield break; // Exit if fetch fails
            }
        }
    }



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

    public IEnumerator PostData(string url, string jsonData, Action onSuccess, Action<string> onError)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending JSON to server: " + url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully posted JSON to server.");
                onSuccess?.Invoke();
            }
            else
            {
                string error = $"Error {www.responseCode}: {www.error}";
                Debug.LogError(error);
                onError?.Invoke(error);
            }
        }
    }

    public IEnumerator PostFormData(string url, string jsonData, System.Action onSuccess, System.Action<string> onError)
    {
        // Prepare the form for the HTTP POST request
        WWWForm form = new WWWForm();
        form.AddField("JS", jsonData);  // Add the JSON as a form field

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            Debug.Log("Uploading form data to server: " + url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Form data uploaded successfully.");
                onSuccess?.Invoke();
            }
            else
            {
                string error = $"Error {www.responseCode}: {www.error}";
                Debug.LogError(error);
                onError?.Invoke(error);
            }
        }
    }
}
