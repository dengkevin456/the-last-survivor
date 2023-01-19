using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LevelStatistics : MonoBehaviour
{
    public static Dictionary<int, string> bestTimeDict = new Dictionary<int, string>();
    public static Dictionary<int, bool> levelCompleteDict = new Dictionary<int, bool>();
    public static double CalculateBestTime(string bestTimeString)
    {
        DateTime t = DateTime.ParseExact(bestTimeString, "mm:ss:ff", CultureInfo.InvariantCulture);
        return t.TimeOfDay.TotalSeconds;
    }

    public static float BestTime(float bestTime)
    {
        return TimeSpan.FromSeconds(bestTime).Seconds;
    }
}
