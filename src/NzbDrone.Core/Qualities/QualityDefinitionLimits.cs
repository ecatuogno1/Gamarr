namespace NzbDrone.Core.Qualities;

// Quality size limits are absolute sizes in GB (not per minute like Radarr's)
public static class QualityDefinitionLimits
{
    public const int Min = 0;
    public const int Max = 1000;
}
