using System.Text.Json.Serialization;
using becore.api.Scheme;
using File = becore.api.Scheme.System.File;

namespace becore.api.Models;

public class FileModel
{
    /// <summary>
    /// Database entity
    /// </summary>
    public required File Entity { get; set; }
    /// <summary>
    /// Physical data
    /// </summary>
    [JsonIgnore]
    public required Stream Data { get; set; }

    public override string ToString() => $"{Entity as DbEntity}";
}
