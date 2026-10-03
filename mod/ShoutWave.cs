using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// Unrelenting Force: a wave of sound that shoves everything away and sends dragon fire back where it came from.
public class ShoutWave : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 48; Projectile.height = 48; Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee; Projectile.penetrate = -1; Projectile.timeLeft = 55;
        Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.alpha = 40;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
        Projectile.scale = 1.2f + (55 - Projectile.timeLeft) / 55f * 1.6f;
        Projectile.alpha = 40 + (int)((55 - Projectile.timeLeft) / 55f * 200);
        Projectile.velocity *= 0.985f;
        Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 0.9f);
        Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(22, 22) * Projectile.scale, DustID.IceTorch, Projectile.velocity * 0.2f, 100, default, 1.2f);
        d.noGravity = true;

        // Reflect hostile projectiles (dragon fire) back at their sender.
        foreach (Projectile p in Main.projectile) {
            if (!p.active || !p.hostile || p.whoAmI == Projectile.whoAmI) continue;
            if (Vector2.Distance(p.Center, Projectile.Center) > 28 * Projectile.scale + p.width) continue;
            p.velocity = -p.velocity * 1.3f; p.hostile = false; p.friendly = true; p.damage = 45; p.timeLeft = 120;
            p.owner = Projectile.owner; p.penetrate = 2;
            Mix.Burst(p.Center, DustID.MagicMirror, 14, 12, 4);
            Mix.Popup(p.Top, "Reflected!", Color.Cyan);
        }
        // Items on the floor get blown away too.
        foreach (Item it in Main.item) {
            if (it.active && Vector2.Distance(it.Center, Projectile.Center) < 40) it.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Extra shove for anything light enough.
        if (!target.boss) {
            target.velocity.X = Projectile.velocity.SafeNormalize(Vector2.UnitX).X * 11f * target.knockBackResist * 1.8f;
            target.velocity.Y = -9f * (target.knockBackResist > 0 ? 1 : 0);
            if (target.ModNPC is Draugr d) d.Launched = true;
        }
        Mix.Burst(target.Center, DustID.MagicMirror, 14, 14, 4);
        Mix.Sound(SoundID.Item122, target.Center);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Glow: bright regardless of the surrounding light.
        var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (1f - Projectile.alpha / 255f),
            Projectile.rotation, tex.Size() / 2f, Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        return false;
    }
}
