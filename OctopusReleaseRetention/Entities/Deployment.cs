using System.Diagnostics;

namespace OctopusReleaseRetention.Entities;

[DebuggerDisplay("{ToString()}")]
public class Deployment
{
    public required string Id { get; set; }
    public required string ReleaseId { get; set; }
    public required string EnvironmentId { get; set; }
    public required DateTime DeployedAt { get; set; }

    public override string ToString()
    {
        return $"{Id}, {ReleaseId}, {EnvironmentId}, DeployedAt={DeployedAt}";
    }
}
