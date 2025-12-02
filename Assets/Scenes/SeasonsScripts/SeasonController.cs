using UnityEngine;
using Seasons;
using System;

public class SeasonController : MonoBehaviour
{
    [Header("References")]
    public OverlayFeed overlayFeed;
    public SimulationController simulationController;

    [Header("Season Asset Roots")]
    public GameObject winterRoot;
    public GameObject springRoot;
    public GameObject summerRoot;
    public GameObject fallRoot;

    [Header("Year Settings")]
    public double siderealYearDays = 365.256363;

    private SeasonHelper.SeasonType currentSeason;
    private bool hasSeasonData = false;

    void Start()
    {
        if (simulationController == null && overlayFeed != null && overlayFeed.masterControl != null)
        {
            simulationController = overlayFeed.masterControl.GetComponent<SimulationController>();
        }

        UpdateSeason(true);
    }

    void Update()
    {
        UpdateSeason(false);
    }

    private void UpdateSeason(bool force)
    {
        if (overlayFeed == null || simulationController == null)
            return;

        hasSeasonData =
            overlayFeed.winter_len > 0 &&
            overlayFeed.spring_len > 0 &&
            overlayFeed.summer_len > 0 &&
            overlayFeed.fall_len > 0;

        if (!hasSeasonData)
            return;

 
        double timeInYear = ComputeDayOfYear();

        var newSeason = SeasonHelper.GetSeasonForTime(
            timeInYear,
            siderealYearDays,
            overlayFeed.winter_len,
            overlayFeed.spring_len,
            overlayFeed.summer_len,
            overlayFeed.fall_len);

        if (!force && newSeason == currentSeason)
            return;

        currentSeason = newSeason;
        ApplySeasonAssets(newSeason);
    }

    private double ComputeDayOfYear()
    {
        string dateStr = simulationController.dateRead();

        DateTime date;

  
        if (DateTime.TryParse(dateStr, out date))
        {
            return date.DayOfYear;
        }

        // If parsing fails, default to 0 (start of year)
        Debug.LogWarning("SeasonController: Could not parse date string from SimulationController: " + dateStr);
        return 0;
    }

    private void ApplySeasonAssets(SeasonHelper.SeasonType season)
    {
        if (winterRoot != null) winterRoot.SetActive(season == SeasonHelper.SeasonType.Winter);
        if (springRoot != null) springRoot.SetActive(season == SeasonHelper.SeasonType.Spring);
        if (summerRoot != null) summerRoot.SetActive(season == SeasonHelper.SeasonType.Summer);
        if (fallRoot != null) fallRoot.SetActive(season == SeasonHelper.SeasonType.Fall);
    }
}
