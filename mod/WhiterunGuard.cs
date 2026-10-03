using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// A Whiterun guard: follows the hero, shoots arrows at undead, and tells everyone about his knee.
public class WhiterunGuard : ModNPC
{
    int stuck, shootTimer, jokeTimer;
    static readonly string[] Lines = {
        "I used to be an adventurer like you. Then I took an arrow to the knee.",
        "Let me guess, someone stole your sweetroll.",
        "Stop right there, criminal scum!",
        "No lollygaggin'!",
        "I'd be a lot more comfortable if I weren't on fire.",
    };

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

    public override void SetDefaults()
    {
        NPC.width = 28; NPC.height = 68;
        NPC.lifeMax = 220; NPC.damage = 0; NPC.defense = 10; NPC.knockBackResist = 0.4f; NPC.friendly = true;
        NPC.aiStyle = -1; NPC.HitSound = SoundID.NPCHit4; NPC.DeathSound = SoundID.NPCDeath6;
        NPC.value = 0; NPC.noGravity = false;
        DrawOffsetY = -2;
        jokeTimer = Main.rand.Next(400, 900);
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.velocity.Y != 0 || System.Math.Abs(NPC.velocity.X) < 0.1f) { NPC.frame.Y = 0; return; }
        NPC.frameCounter += 0.4 + System.Math.Abs(NPC.velocity.X);
        NPC.frame.Y = (int)(NPC.frameCounter / 7 % 4) * frameHeight;
    }

    public override void AI()
    {
        Player p = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
        NPC enemy = Mix.Nearest(NPC.Center, 24, Mix.IsHostile);
        int want = 0;
        if (enemy != null) {
            float dx = enemy.Center.X - NPC.Center.X;
            NPC.direction = NPC.spriteDirection = dx < 0 ? -1 : 1;
            float ad = System.Math.Abs(dx);
            want = ad > 16 * 11 ? NPC.direction : ad < 16 * 5 ? -NPC.direction : 0;
            if (++shootTimer >= 70 && ad < 16 * 22 && System.Math.Abs(enemy.Center.Y - NPC.Center.Y) < 16 * 12) {
                shootTimer = 0;
                Vector2 from = NPC.Center + new Vector2(NPC.direction * 12, -8);
                Vector2 aim = enemy.Center + enemy.velocity * 6 + new Vector2(0, -System.Math.Abs(dx) * 0.05f);
                Mix.Shoot(ProjectileID.WoodenArrowFriendly, from, Mix.Aim(from, aim, 12f), 24, 3f);
                Mix.Sound("twang", NPC.Center, 0.7f);
                Mix.Burst(from, DustID.Smoke, 4, 6, 1.5f);
            }
        } else if (p.active) {
            float dx = p.Center.X - NPC.Center.X;
            want = System.Math.Abs(dx) > 16 * 7 ? System.Math.Sign(dx) : 0;
            if (want != 0) NPC.direction = NPC.spriteDirection = want;
        }

        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, want * 2.0f, 0.15f);
        // Hop over ledges.
        stuck = want != 0 && System.Math.Abs(NPC.velocity.X) < 0.4f && NPC.velocity.Y == 0 ? stuck + 1 : 0;
        if (stuck > 8) { NPC.velocity.Y = -7.5f; stuck = 0; }
        if (NPC.velocity.Y == 0 && Collision.SolidCollision(NPC.position + new Vector2(want * 14, 0), NPC.width, NPC.height - 8))
            NPC.velocity.Y = -7.5f;

        if (--jokeTimer <= 0) {
            jokeTimer = Main.rand.Next(900, 1500);
            string line = Lines[Main.rand.Next(Lines.Length)];
            CombatText.NewText(NPC.getRect(), new Color(150, 200, 255), line.Length > 34 ? "Arrow to the knee..." : line, true);
            Mix.Say("Whiterun Guard: " + line, new Color(150, 200, 255));
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < (NPC.life > 0 ? 5 : 20); i++)
            Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 2f, -2f);
    }
}
