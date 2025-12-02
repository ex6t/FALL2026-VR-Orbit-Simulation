using System;
using UnityEngine;
using Seasons;   // SeasonHelper + SeasonsCalc
using Bergers;  // BergerSol (if available)

public class SeasonSimulationManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Object that has SimulationController (your Master Controller).")]
    public GameObject masterController;

    [Tooltip("Transform of the Earth object that already has the orbit script.")]
    public Transform earthTransform;

    [Header("Season Tree Roots (can be anywhere in hierarchy)")]
    public Transform winterRoot;
    public Transform springRoot;
    public Transform summerRoot;
    public Transform fallRoot;

    [Header("Year / Orbit Settings")]
    [Tooltip("Sidereal year length in days. Should match the T used in your orbit/season math.")]
    public double T = 365.256363;

    [Tooltip("Orbital eccentricity used by SeasonsCalc.")]
    public double eccentricity = 0.01670236225492288;

    [Tooltip("Precession angle in radians (same value you used before).")]
    public double precession = Mathf.PI - 1.796256991128036f;

    private SimulationController sim;

    // season lengths
    private double winterLen, springLen, summerLen, fallLen;
    private int cachedYear = int.MinValue;

    private bool simulationStarted = false;
    private SeasonHelper.SeasonType currentSeason;

    void Start()
    {
        if (masterController == null)
        {
            Debug.LogError("SeasonSimulationManager: masterController is not assigned.");
            return;
        }

        sim = masterController.GetComponent<SimulationController>();
        if (sim == null)
        {
            Debug.LogError("SeasonSimulationManager: SimulationController not found on masterController.");
            return;
        }

        if (earthTransform == null)
        {
            Debug.LogError("SeasonSimulationManager: earthTransform is not assigned.");
            return;
        }

        // Make sure only one set is visible at start (optional)
        ApplySeasonObjects(SeasonHelper.SeasonType.Winter); // or disable all, your choice
    }

    void Update()
    {
        if (!simulationStarted || sim == null)
            return;

        int year = sim.getYear();
        if (year != cachedYear)
        {
            UpdateSeasonLengths(year);
        }

        UpdateSeason(false);
    }

    /// <summary>
    /// Called by the UI button to begin the season system and attach trees to Earth.
    /// </summary>
    public void StartSimulation()
    {
        if (sim == null || earthTransform == null)
        {
            Debug.LogError("SeasonSimulationManager: Cannot start simulation, missing references.");
            return;
        }

        // 1) Parent each tree root to Earth so they ride along with it
        //    worldPositionStays = true keeps them where you placed them.
        if (winterRoot != null) winterRoot.SetParent(earthTransform, true);
        if (springRoot != null) springRoot.SetParent(earthTransform, true);
        if (summerRoot != null) summerRoot.SetParent(earthTransform, true);
        if (fallRoot != null) fallRoot.SetParent(earthTransform, true);

        // 2) Compute season lengths for current year
        int year = sim.getYear();
        UpdateSeasonLengths(year);

        // 3) Turn system on and immediately apply correct season
        simulationStarted = true;
        UpdateSeason(true);
    }

    private void UpdateSeasonLengths(int year)
    {
        cachedYear = year;

        // Optional: get eccentricity from BergerSol if you’re using it
        try
        {
            double obliquity, longPeri;
            BergerSol.CalculateOrbitalParameters(year, out eccentricity, out obliquity, out longPeri);
        }
        catch
        {
            // ignore if BergerSol isn't set up
        }

        SeasonsCalc.CalculateSeasonLengths(
            T,
            eccentricity,
            precession,
            out winterLen,
            out springLen,
            out summerLen,
            out fallLen
        );
    }

    private void UpdateSeason(bool force)
    {
        if (winterLen <= 0 || springLen <= 0 || summerLen <= 0 || fallLen <= 0)
            return;

        double day = GetDayOfYearFromSimulation();

        var newSeason = SeasonHelper.GetSeasonForTime(
            day,
            T,
            winterLen,
            springLen,
            summerLen,
            fallLen
        );

        if (!force && newSeason == currentSeason)
            return;

        currentSeason = newSeason;
        ApplySeasonObjects(newSeason);
    }

    private double GetDayOfYearFromSimulation()
    {
        string dateStr = sim.dateRead();  // e.g. "03/21/2000"

        if (DateTime.TryParse(dateStr, out DateTime dt))
            return dt.DayOfYear;

        Debug.LogWarning("SeasonSimulationManager: Could not parse date from SimulationController: " + dateStr);
        return 0;
    }

    private void ApplySeasonObjects(SeasonHelper.SeasonType season)
    {
        if (winterRoot != null) winterRoot.gameObject.SetActive(season == SeasonHelper.SeasonType.Winter);
        if (springRoot != null) springRoot.gameObject.SetActive(season == SeasonHelper.SeasonType.Spring);
        if (summerRoot != null) summerRoot.gameObject.SetActive(season == SeasonHelper.SeasonType.Summer);
        if (fallRoot != null) fallRoot.gameObject.SetActive(season == SeasonHelper.SeasonType.Fall);
    }
}
