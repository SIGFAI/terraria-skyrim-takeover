using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// The dragon: circles the hero, breathes fire, swoops, and when it dies its soul flows into the Dragonborn.
public class Dragon : ModNPC
{
    int side = 1;
    Vector2 diveTo;

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 2;

    public override void SetDefaults()
    {
        NPC.width = 112; NPC.height = 64;
        NPC.lifeMax = 900; NPC.damage = 34; NPC.defense = 8; NPC.knockBackResist = 0f; NPC.value = Item.buyPrice(gold: 5);
        NPC.aiStyle = -1; NPC.noGravity = true; NPC.noTileCollide = true; NPC.boss = true; NPC.npcSlots = 5f;
        NPC.HitSound = SoundID.NPCHit7; NPC.DeathSound = Mix.Style("roar", 1f, -0.1f);
        NPC.lavaImmune = true;
        DrawOffsetY = 46;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        // flap faster while diving
        NPC.frame.Y = (int)(NPC.frameCounter / (NPC.ai[0] == 2 ? 5 : 10) % 2) * frameHeight;
    }

    // ai[0] state: 0 circle, 1 breathe, 2 dive; ai[1] timer
    public override void AI()
    {
        NPC.TargetClosest(false);
        Player p = Main.player[NPC.target];
        NPC.ai[1]++;
        Vector2 mouth = NPC.Center + new Vector2(NPC.direction * 56, 4);

        if (NPC.ai[0] == 0 || NPC.ai[0] == 1) {
            Vector2 hover = p.Center + new Vector2(side * 190, -150 + (float)System.Math.Sin(NPC.ai[1] * 0.05f) * 25);
            Vector2 want = (hover - NPC.Center) * 0.04f;
            if (want.Length() > 9f) want = want.SafeNormalize(Vector2.Zero) * 9f;
            NPC.velocity = Vector2.Lerp(NPC.velocity, want, 0.07f);
            NPC.direction = NPC.spriteDirection = p.Center.X < NPC.Center.X ? -1 : 1;
            if (NPC.ai[0] == 0 && NPC.ai[1] > 75) { NPC.ai[0] = 1; NPC.ai[1] = 0; Mix.Sound("roar", NPC.Center, 0.8f, 0.1f); }
            if (NPC.ai[0] == 1) {
                if (NPC.ai[1] % 7 == 0) {
                    Vector2 aim = p.Center + new Vector2(Main.rand.Next(-40, 40), Main.rand.Next(-20, 30));
                    Mix.Shoot<DragonFire>(mouth, Mix.Aim(mouth, aim, 9f), 16, 2f, hostile: true);
                    Mix.Burst(mouth, DustID.Torch, 5, 8, 4, null, 1.8f);
                }
                if (NPC.ai[1] > 56) {
                    NPC.ai[1] = 0;
                    NPC.ai[2]++;
                    if (NPC.ai[2] % 2 == 0) { NPC.ai[0] = 2; diveTo = p.Center; Mix.Sound("flap", NPC.Center); }
                    else { NPC.ai[0] = 0; side = -side; }
                }
            }
        } else {
            // dive at the hero's position, then climb away to the other side
            Vector2 dir = (diveTo - NPC.Center).SafeNormalize(Vector2.UnitX);
            NPC.velocity = Vector2.Lerp(NPC.velocity, dir * 14f, 0.12f);
            NPC.direction = NPC.spriteDirection = NPC.velocity.X < 0 ? -1 : 1;
            if (NPC.ai[1] > 38 || Vector2.Distance(NPC.Center, diveTo) < 50) { NPC.ai[0] = 0; NPC.ai[1] = 0; side = -side; }
        }

        if (NPC.ai[1] % 55 == 0) Mix.Sound("flap", NPC.Center, 0.5f);
        Lighting.AddLight(mouth, 0.5f, 0.2f, 0.05f);
        NPC.rotation = MathHelper.Clamp(NPC.velocity.Y * 0.04f, -0.3f, 0.3f) * NPC.direction;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < (NPC.life > 0 ? 8 : 60); i++) {
            Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, i % 4 == 0 ? DustID.Torch : DustID.Blood, hit.HitDirection * 3f, -1f, 0, default, 1.4f);
            d.noGravity = i % 4 == 0;
        }
        if (NPC.life > 0 && Main.rand.NextBool(6)) Mix.Sound("roar", NPC.Center, 0.4f, 0.2f);
    }

    public override void OnKill()
    {
        Mix.Shake(14, 1.2);
        Mix.Sound("boom", NPC.Center);
        Mix.Title("DRAGON SOUL ABSORBED", "You are Dragonborn!", 4);
        SigfMod.QuestDone();
        Mix.Sound("soul", Mix.Host.Center);
        for (int i = 0; i < 16; i++) {
            Projectile wisp = Mix.Shoot<DragonSoul>(NPC.Center, Main.rand.NextVector2CircularEdge(7, 7), 0, 0, false);
            if (wisp != null) wisp.ai[1] = i % 2;
        }
        Mix.Drop(ItemID.GoldCoin, NPC.Center, 12);
        Mix.Drop<CheeseWheel>(NPC.Center, 4);
    }

    public override void PostDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
}
