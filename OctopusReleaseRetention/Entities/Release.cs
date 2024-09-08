using System;
using System.Diagnostics;

namespace OctopusReleaseRetention.Entities;

[DebuggerDisplay("{ToString()}")]
public class Release
{
    public required string Id { get; set; }
    public required string ProjectId { get; set; }
    public required string Version { get; set; }
    public required DateTime Created { get; set; }

    public override string ToString()
    {
        return $"{Id}, {ProjectId}, {Version} Created={Created}";
    }
}
