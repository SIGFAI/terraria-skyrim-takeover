using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class SigfMod : ModSystem
{
    static long questDoneAt = -99999;

    static int Count<T>() where T : ModNPC => Mix.NpcsNear(Mix.Host.Center, 80, n => n.type == ModContent.NPCType<T>()).Count;

    // A spot on the town street, dx tiles from the world spawn (feet position).
    public static Vector2 Street(float dx) => new Vector2((Main.spawnTileX + dx) * 16 + 8, Whiterun.GroundY * 16);

    public static void CallDragon()
    {
        if (Count<Dragon>() > 0) return;
        Player p = Mix.Host;
        Mix.Spawn<Dragon>(p.Center + new Vector2(p.direction * 420, -300));
        Mix.Title("A DRAGON ATTACKS!", "Fire rains on Whiterun", 3.5);
        Mix.Sound("roar", p.Center);
        Mix.Shake(10, 1.0);
        SkyrimAtmosphere.Quest = "QUEST: Dragon Rising";
        SkyrimAtmosphere.QuestLine = "Slay the dragon attacking Whiterun";
    }

    public static void Horde()
    {
        Mix.Title("THE DRAUGR RISE!", "Defend Whiterun", 3);
        Mix.Sound("boom", Mix.Host.Center);
        Mix.Shake(6, 0.6);
        for (int i = 0; i < 4; i++) { Mix.Spawn<Draugr>(Street(-12 - i * 3)); Mix.Spawn<Draugr>(Street(13 + i * 3)); }
    }

    public static void QuestDone()
    {
        questDoneAt = Mix.Tick;
        SkyrimAtmosphere.Quest = "QUEST COMPLETE: Dragon Rising";
        SkyrimAtmosphere.QuestLine = "Dragon soul absorbed";
        Mix.After(1.5, () => Mix.Sound("chime", Mix.Host.Center));
    }

    public override void OnWorldLoad()
    {
        Mix.WanderTiles = 15;
        SkyrimAtmosphere.WhiterunBuilt = false;
        SkyrimAtmosphere.Quest = "QUEST: Defend Whiterun";
        SkyrimAtmosphere.Slain = 0;
        Mix.After(0.2, Whiterun.Build);
        Mix.After(0.5, Mix.Arm<DragonboneBlade>);

        // always on: undead keep coming, guards keep fighting, a dragon now and then
        Mix.Every(6, () => {
            if (Whiterun.GroundY > 0 && Count<Draugr>() < 5) Mix.Spawn<Draugr>(Street(Main.rand.NextBool() ? -26 : 27));
        });
        Mix.Every(5, () => {
            if (Whiterun.GroundY > 0 && Count<WhiterunGuard>() < 2) Mix.Spawn<WhiterunGuard>(Street(Main.rand.Next(-8, 8)));
        });
        Mix.Every(1, () => {
            if (Mix.Tick - questDoneAt > 60 * 8 && Count<Dragon>() == 0) { SkyrimAtmosphere.Quest = "QUEST: Defend Whiterun"; }
            if (Count<Dragon>() == 0 && Mix.Tick - questDoneAt > 60 * 8) SkyrimAtmosphere.QuestLine = "Draugr slain: " + SkyrimAtmosphere.Slain;
        });
        if (!Mix.IsDemo) { Mix.Every(45, CallDragon); Mix.Every(70, Horde); }

        Mix.Demo(1, () => { Mix.Arm<DragonboneBlade>(); Mix.Title("SKYRIM TAKEOVER", "Hey, you. You're finally awake.", 4); });
        Mix.Demo(2, () => { Mix.Spawn<WhiterunGuard>(Street(-4)); Mix.Spawn<WhiterunGuard>(Street(4)); });
        Mix.Demo(3, () => { for (int i = 0; i < 3; i++) { Mix.Spawn<Draugr>(Street(-10 - i * 3)); Mix.Spawn<Draugr>(Street(12 + i * 3)); } });
        Mix.Demo(9, CallDragon);
        Mix.Demo(30, Horde);
        Mix.Demo(44, CallDragon);
        Mix.Demo(62, Horde);
    }
}
