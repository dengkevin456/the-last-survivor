using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Stopwatch : MonoBehaviour
{
    private bool stopwatchActive;
    
    [HideInInspector]
    public float currentTime;
    
    public TextMeshProUGUI currentTimeText;

    private void Awake()
    {
        currentTimeText = GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        stopwatchActive = true;
        currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (stopwatchActive)
            currentTime += Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currentTimeText.text = time.ToString(@"mm\:ss\:ff");
    }

    public void StopStopwatch()
    {
        stopwatchActive = false;
    }
}
