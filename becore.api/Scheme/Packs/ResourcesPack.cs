using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.Packs;

[Table("ResourcePack")]
public class ResourcesPack : Pack
{
    public ResourcesPack()
    {
        PackType = PackType.Resources;
    }
}