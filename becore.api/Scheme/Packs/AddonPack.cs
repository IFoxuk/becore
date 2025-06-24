using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.Packs;

[Table("AddonPack")]
public class AddonPack : Pack
{
    public AddonPack()
    {
        PackType = PackType.Addition;
    }
}