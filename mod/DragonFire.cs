using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// Dragon breath: burns the wooden houses, sets undead on fire and hurts guards. A shout sends it back.
public class DragonFire : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 22; Projectile.height = 22; Projectile.timeLeft = 150;
        Projectile.friendly = false; Projectile.hostile = true; Projectile.penetrate = 1;
        Projectile.DamageType = DamageClass.Default; Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
        Projectile.velocity.Y += 0.08f;
        Lighting.AddLight(Projectile.Center, 1f, 0.55f, 0.15f);
        for (int i = 0; i < 2; i++) {
            Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), Main.rand.NextBool(3) ? DustID.Smoke : DustID.Torch,
                -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1, 1), 100, default, 1.7f);
            d.noGravity = true;
        }

        if (Projectile.hostile) {
            foreach (NPC n in Mix.NpcsNear(Projectile.Center, 2)) {
                if (n.type == ModContent.NPCType<WhiterunGuard>()) {
                    n.SimpleStrikeNPC(22, Projectile.velocity.X > 0 ? 1 : -1, false, 5f, DamageClass.Default, true, 0f, true);
                    n.AddBuff(BuffID.OnFire, 180);
                    Projectile.Kill();
                    return;
                }
                if (n.type == ModContent.NPCType<Draugr>()) n.AddBuff(BuffID.OnFire, 360);   // undead burn nicely
            }
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire, 240);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire, 360);

    public override void OnKill(int timeLeft)
    {
        Mix.Sound(SoundID.Item14, Projectile.Center);
        for (int i = 0; i < 26; i++) {
            Dust d = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.Smoke : DustID.Torch, Main.rand.NextVector2Circular(6, 6), 100, default, 2f);
            d.noGravity = true;
        }
        // Wooden blocks catch fire and fall apart.
        int cx = (int)(Projectile.Center.X / 16f), cy = (int)(Projectile.Center.Y / 16f);
        for (int x = cx - 2; x <= cx + 2; x++)
            for (int y = cy - 2; y <= cy + 2; y++) {
                if (!WorldGen.InWorld(x, y, 10)) continue;
                Tile t = Main.tile[x, y];
                if (t.HasTile && (t.TileType == TileID.WoodBlock || t.TileType == TileID.Platforms || t.TileType == TileID.WoodenBeam) && Main.rand.NextBool(2)) {
                    WorldGen.KillTile(x, y, false, false, true);
                    Mix.Burst(new Vector2(x * 16 + 8, y * 16 + 8), DustID.Torch, 8, 8, 3);
                }
            }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, 1f,
            Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        return false;
    }
}
