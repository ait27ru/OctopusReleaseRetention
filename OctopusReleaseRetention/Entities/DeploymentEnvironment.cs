using System.Diagnostics;

namespace OctopusReleaseRetention.Entities;

[DebuggerDisplay("{ToString()}")]
public class DeploymentEnvironment
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public override string ToString()
    {
        return $"{Id}, {Name}";
    }
}