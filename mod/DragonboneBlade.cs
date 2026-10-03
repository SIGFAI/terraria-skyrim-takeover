using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// A greatsword of dragon bone. Every swing also lets out the Thu'um: FUS RO DAH.
public class DragonboneBlade : ModItem
{
    int swings;

    public override void SetDefaults()
    {
        Item.width = 40; Item.height = 40; Item.scale = 1.4f;
        Item.damage = 55; Item.DamageType = DamageClass.Melee; Item.knockBack = 7f;
        Item.useTime = 26; Item.useAnimation = 26; Item.useStyle = ItemUseStyleID.Swing; Item.autoReuse = true;
        Item.UseSound = SoundID.Item1; Item.rare = ItemRarityID.Pink; Item.value = Item.sellPrice(gold: 5);
        Item.shoot = ModContent.ProjectileType<ShoutWave>(); Item.shootSpeed = 13f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        string[] words = { "FUS!", "FUS RO!", "FUS RO DAH!" };
        Mix.Popup(player.Top + new Vector2(0, -16), words[swings++ % 3], Color.Cyan);
        Mix.Sound("shout", player.Center);
        Mix.Burst(player.Center + new Vector2(player.direction * 30, -4), DustID.MagicMirror, 10, 10, 4);
        return true;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        if (Main.rand.NextBool(3)) {
            Dust d = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.IceTorch, 0, 0, 100, default, 1.1f);
            d.noGravity = true;
        }
    }
}
