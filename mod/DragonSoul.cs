using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// A wisp of dragon soul flowing from the dead dragon into the hero.
public class DragonSoul : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 20; Projectile.height = 20; Projectile.tileCollide = false; Projectile.timeLeft = 140;
        Projectile.friendly = false; Projectile.hostile = false; Projectile.penetrate = -1; Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Player p = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
        Vector2 to = p.Center - Projectile.Center;
        Projectile.ai[0]++;
        Vector2 want = to.SafeNormalize(Vector2.UnitY) * (6f + Projectile.ai[0] * 0.18f);
        // swirl first, then home in
        Vector2 swirl = new Vector2(-to.Y, to.X).SafeNormalize(Vector2.Zero) * (Projectile.ai[1] == 0 ? 2.5f : -2.5f) * MathHelper.Clamp(1f - Projectile.ai[0] / 60f, 0f, 1f);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, want + swirl, 0.1f);
        Projectile.rotation += 0.2f;
        Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 1f);
        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, Vector2.Zero, 100, default, 1.3f);
        d.noGravity = true;
        if (to.Length() < 24f) {
            p.AddBuff(ModContent.BuffType<Dragonborn>(), 60 * 40);
            Mix.Burst(p.Center, DustID.MagicMirror, 8, 14, 3);
            Projectile.Kill();
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
