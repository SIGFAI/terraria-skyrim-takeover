using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Sigf.Content;

// Skyrim look and feel on screen: cold tint, falling snow, a compass bar with enemy markers, and the Whiterun sign.
public class SkyrimAtmosphere : ModSystem
{
    public static Vector2 WhiterunPos;   // world position of the town gate, set when the town is built
    public static bool WhiterunBuilt;
    public static string Quest = "", QuestLine = "";
    public static int Slain;

    public override void PostUpdateEverything()
    {
        if (Main.gameMenu || Main.dedServ || Mix.ReadyTick < 0) return;
        Main.GameZoomTarget = 1.5f;
        Main.mapStyle = 0;
        // Snow drifting across the whole view.
        Vector2 half = new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom.X;
        for (int i = 0; i < 4; i++) {
            Vector2 pos = Main.screenPosition + new Vector2(Main.rand.NextFloat(-60, half.X + 60), Main.rand.NextFloat(-30, half.Y * 0.9f));
            Dust d = Dust.NewDustPerfect(pos, DustID.Snow, new Vector2(Main.rand.NextFloat(-1.5f, 0.2f), Main.rand.NextFloat(1.2f, 2.4f)), 0, Color.White, Main.rand.NextFloat(1.2f, 1.9f));
            d.noGravity = true; d.fadeIn = 0.4f;
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // Cold blue-grey grade over the world, below the interface.
        layers.Insert(0, new LegacyGameInterfaceLayer("Sigf: Nord grade", () => {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(60, 90, 140) * 0.16f);
            return true;
        }, InterfaceScaleType.None));

        int i = layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text"));
        if (i < 0) i = layers.Count;
        layers.Insert(i, new LegacyGameInterfaceLayer("Sigf: Compass", DrawCompass, InterfaceScaleType.None));
        layers.Insert(i, new LegacyGameInterfaceLayer("Sigf: Signs", DrawSigns, InterfaceScaleType.Game));
    }

    static bool DrawSigns()
    {
        if (!WhiterunBuilt) return true;
        Vector2 p = WhiterunPos - Main.screenPosition;
        Utils.DrawBorderStringBig(Main.spriteBatch, "WHITERUN", p, new Color(235, 220, 160), 0.8f, 0.5f, 0.5f);
        return true;
    }

    static bool DrawCompass()
    {
        Player pl = Main.LocalPlayer;
        if (pl == null || !pl.active) return true;
        float s = Main.UIScale;   // drawn in raw pixels (layer scale None), so every size is multiplied by s
        float w = 460 * s, cx = Main.screenWidth / 2f, y = 22 * s, h = 20 * s;
        var bar = new Rectangle((int)(cx - w / 2), (int)y, (int)w, (int)h);
        var px = TextureAssets.MagicPixel.Value;
        Main.spriteBatch.Draw(px, bar, new Color(10, 14, 22) * 0.6f);
        Main.spriteBatch.Draw(px, new Rectangle(bar.X, bar.Y, bar.Width, (int)(2 * s)), new Color(210, 200, 170) * 0.8f);
        Main.spriteBatch.Draw(px, new Rectangle(bar.X, bar.Bottom - (int)(2 * s), bar.Width, (int)(2 * s)), new Color(210, 200, 170) * 0.8f);
        Main.spriteBatch.Draw(px, new Rectangle((int)cx - (int)s, (int)(bar.Y - 5 * s), (int)(3 * s), (int)(30 * s)), Color.White * 0.9f);
        // tick marks that scroll with the hero
        for (int t = -6; t <= 6; t++) {
            float x = cx + (t * 40 - (pl.Center.X % 160) / 4f) * s;
            if (x < bar.X + 4 * s || x > bar.Right - 4 * s) continue;
            Main.spriteBatch.Draw(px, new Rectangle((int)x, (int)(bar.Y + 6 * s), (int)(2 * s), (int)(8 * s)), new Color(210, 200, 170) * 0.35f);
        }
        if (Quest.Length > 0) {
            Utils.DrawBorderString(Main.spriteBatch, Quest, new Vector2(cx, y + 36 * s), new Color(255, 225, 130), 0.95f * s, 0.5f, 0.5f);
            Utils.DrawBorderString(Main.spriteBatch, QuestLine, new Vector2(cx, y + 58 * s), Color.White, 0.75f * s, 0.5f, 0.5f);
        }
        const float range = 2400f;
        foreach (NPC n in Main.npc) {
            if (!n.active || n.friendly || !n.CanBeChasedBy()) continue;
            float dx = (n.Center.X - pl.Center.X) / range;
            if (System.Math.Abs(dx) > 1) continue;
            bool boss = n.boss;
            Main.spriteBatch.Draw(px, new Vector2(cx + dx * w / 2, bar.Y + 10 * s), null, boss ? Color.Gold : new Color(230, 70, 60), MathHelper.PiOver4,
                px.Size() / 2f, new Vector2((boss ? 11f : 7f) * s) / px.Size(), SpriteEffects.None, 0);
        }
        if (WhiterunBuilt) {
            float dx = (WhiterunPos.X - pl.Center.X) / range;
            float tx = cx + MathHelper.Clamp(dx, -1, 1) * w / 2;
            Utils.DrawBorderString(Main.spriteBatch, "Whiterun", new Vector2(tx, bar.Y + 10 * s), new Color(150, 210, 255), 0.7f * s, 0.5f, 0.5f);
        }
        return true;
    }
}
