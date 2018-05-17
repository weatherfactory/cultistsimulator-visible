using UnityEngine;

public class FramerateMonitor : BugReportingMonitor
{
    #region Constructors

    public FramerateMonitor()
    {
        this.MaximumDurationInSeconds = 10;
        this.MinimumFramerate = 15;
    }

    #endregion

    #region Fields

    private float duration;

    public float MaximumDurationInSeconds;

    public float MinimumFramerate;

    #endregion

    #region Methods

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        float framerate = 1.0f / deltaTime;
        if (framerate < this.MinimumFramerate)
        {
            this.duration += deltaTime;
        }
        else
        {
            this.duration = 0;
        }
        if (this.duration > this.MaximumDurationInSeconds)
        {
            this.duration = 0;
            this.Trigger();
        }
    }

    #endregion
}