using System.ComponentModel.DataAnnotations.Schema;

namespace becore.api.Scheme.Packs;

[Table("ScriptPack")]
public class ScriptPack : Pack
{
    public ScriptPack()
    {
        PackType = PackType.Script;
    }
}