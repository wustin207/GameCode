using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PlayerStats
{
    public string playerKills;
    public string playerDeaths;
    public string totalTimer;
    public int heartRateAvg;
    public int heartRateMin;
    public int heartRateMax;
}


public class Stats : MonoBehaviour
{
    public string serverURL = "https://server-6stn.onrender.com/api/stats";
    public GraphTest graph;
    public GameManager manager;


    public void GetStats()
    {
        TextMeshProUGUI playerKillsTMP = manager.playerKillsText.GetComponent<TextMeshProUGUI>();
        string playerKillsText = playerKillsTMP.text;

        TextMeshProUGUI playerDeathsTMP = manager.playerDeathsText.GetComponent<TextMeshProUGUI>();
        string playerDeathsText = playerDeathsTMP.text;

        TextMeshProUGUI totalTimerTMP = manager.totalTimerText.GetComponent<TextMeshProUGUI>();
        string totalTimerText = totalTimerTMP.text;

        graph = GameObject.Find("Graph").GetComponent<GraphTest>();


        PlayerStats stats = new PlayerStats
        {
            playerKills = playerKillsText,
            playerDeaths = playerDeathsText,
            totalTimer = totalTimerText,
            heartRateAvg = graph.averageHeartRate,
            heartRateMin = graph.minHeartRate,
            heartRateMax = graph.maxHeartRate
        };

        string json = JsonUtility.ToJson(stats);
        StartCoroutine(SendStats(json));
    }

    public IEnumerator SendStats(string json)
    {
        UnityWebRequest request = new UnityWebRequest(serverURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");


        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Stats uploaded successfully");
        else
            Debug.LogError("Upload failed: " + request.error);
    }
}