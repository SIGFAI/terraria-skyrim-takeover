using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// Nordic undead warrior: shambles toward the player, chills on touch, burns easily.
public class Draugr : ModNPC
{
    public bool Launched;   // thrown by a shout: tumbles through the air
    int airTime;

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

    public override void SetDefaults()
    {
        NPC.width = 30; NPC.height = 76;
        NPC.lifeMax = 210; NPC.damage = 18; NPC.defense = 4; NPC.knockBackResist = 0.55f; NPC.value = 120;
        NPC.aiStyle = NPCAIStyleID.Fighter; AIType = NPCID.Zombie;
        AnimationType = -1;
        NPC.HitSound = SoundID.NPCHit2; NPC.DeathSound = SoundID.NPCDeath2;
        DrawOffsetY = -2;
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.velocity.Y != 0) { NPC.frame.Y = 0; return; }
        NPC.frameCounter += 0.4 + System.Math.Abs(NPC.velocity.X);
        NPC.frame.Y = (int)(NPC.frameCounter / 7 % 4) * frameHeight;
    }

    public override void AI()
    {
        if (Launched) {
            NPC.rotation += NPC.velocity.X > 0 ? 0.4f : -0.4f;
            airTime++;
            if (airTime > 6 && NPC.velocity.Y == 0) {   // landed
                Launched = false; airTime = 0; NPC.rotation = 0;
                Mix.Burst(NPC.Bottom, DustID.Smoke, 12, 16, 3);
                Mix.Sound(SoundID.Item53, NPC.Bottom);
            }
        }
        Lighting.AddLight(NPC.Top + new Vector2(0, 10), 0.1f, 0.3f, 0.5f);   // cold glowing eyes
        if (Main.rand.NextBool(18)) {
            Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.IceTorch, 0, -1f, 150, default, 0.8f);
            d.noGravity = true;
        }
        // Draugr also cut down guards that stand in the way.
        if (NPC.ai[3]++ % 45 == 0)
            foreach (NPC g in Mix.NpcsNear(NPC.Center, 2.5, n => n.type == ModContent.NPCType<WhiterunGuard>()))
                g.SimpleStrikeNPC(14, NPC.direction, false, 4f, DamageClass.Default, true, 0f, true);
    }

    public override void OnKill() => SkyrimAtmosphere.Slain++;

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) => target.AddBuff(BuffID.Chilled, 180);

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < (NPC.life > 0 ? 6 : 24); i++) {
            Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, i % 3 == 0 ? DustID.IceTorch : DustID.Bone, hit.HitDirection * 3f, -2f);
            d.noGravity = i % 3 == 0;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CheeseWheel>(), 5));
        npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 3, 1, 2));
    }
}
