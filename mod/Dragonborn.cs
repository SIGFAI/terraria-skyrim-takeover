using Terraria;
using Terraria.ModLoader;

namespace Sigf.Content;

// Dragon soul absorbed: stronger shouts and swords, and a little life regeneration.
public class Dragonborn : ModBuff
{
    public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = false; }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage(DamageClass.Generic) += 0.2f;
        player.lifeRegen += 4;
        if (Main.rand.NextBool(6)) {
            var d = Dust.NewDustDirect(player.position, player.width, player.height, Terraria.ID.DustID.MagicMirror, 0, -2f, 100, default, 1f);
            d.noGravity = true;
        }
    }
}
