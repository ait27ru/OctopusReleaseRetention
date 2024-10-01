using System.Diagnostics;
using OctopusReleaseRetention.Core;

namespace OctopusReleaseRetention.Entities;

[DebuggerDisplay("{ToString()}")]
public class Project
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public Priority Priority { get; set; }
    public override string ToString()
    {
        return $"{Id}, {Name}";
    }
}
