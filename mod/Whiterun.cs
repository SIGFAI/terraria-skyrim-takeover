using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// Builds a small Nordic town out of vanilla blocks next to the spawn: a stone watchtower and a wooden longhouse.
public static class Whiterun
{
    public static int GateX, GroundY;   // tile coordinates of the town

    static void Put(int x, int y, int type)
    {
        if (!WorldGen.InWorld(x, y, 10)) return;
        Tile t = Main.tile[x, y];
        t.ClearTile();
        WorldGen.PlaceTile(x, y, type, true, true);
    }

    static void Clear(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 10)) return;
        WorldGen.KillTile(x, y, false, false, true);
        Tile t = Main.tile[x, y];
        if (t.HasTile) t.ClearTile();
        t.WallType = 0;
        t.LiquidAmount = 0;
    }

    static void Wall(int x, int y, int wall)
    {
        if (!WorldGen.InWorld(x, y, 10)) return;
        WorldGen.PlaceWall(x, y, wall, true);
    }

    public static void Build()
    {
        int sx = Main.spawnTileX;
        int x0 = sx + 17;   // tower
        int gy = Main.spawnTileY;
        int mid = sx;
        for (int y = Main.spawnTileY - 40; y < Main.spawnTileY + 40; y++)
            if (WorldGen.SolidTile(mid, y)) { gy = y; break; }
        GroundY = gy;

        // flatten the site
        for (int x = sx - 18; x < sx + 27; x++) {
            for (int y = gy - 30; y < gy; y++) Clear(x, y);
            for (int y = gy; y < gy + 4; y++) Put(x, y, y == gy ? TileID.GrayBrick : TileID.Dirt);
        }

        // watchtower: x0 .. x0+7, 18 high
        int tw = 8, th = 18;
        for (int y = gy - th; y < gy; y++)
            for (int x = x0; x < x0 + tw; x++) {
                bool edge = x == x0 || x == x0 + tw - 1;
                if (edge || y == gy - th) Put(x, y, TileID.GrayBrick);
                else Wall(x, y, WallID.GrayBrick);
            }
        for (int x = x0 - 1; x <= x0 + tw; x += 2) Put(x, gy - th - 1, TileID.GrayBrick);   // battlements
        for (int y = gy - 3; y < gy; y++) Clear(x0, y);                                   // door
        for (int f = 1; f < 4; f++) for (int x = x0 + 1; x < x0 + tw - 1; x++) if (x < x0 + 5) Put(x, gy - f * 5, TileID.Platforms);
        for (int k = 1; k < 4; k++) { WorldGen.PlaceTile(x0 + 1 + (k % 2) * 4, gy - k * 5 - 1, TileID.Torches, true, true); }

        // longhouse: x0+10 .. x0+31
        int hx = sx - 12, hw = 22, hh = 8;
        for (int y = gy - hh; y < gy; y++)
            for (int x = hx; x < hx + hw; x++) {
                bool edge = x == hx || x == hx + hw - 1;
                if (edge) Put(x, y, TileID.WoodBlock);
                else Wall(x, y, WallID.Wood);
            }
        for (int y = gy - 3; y < gy; y++) { Clear(hx, y); Clear(hx + hw - 1, y); Wall(hx, y, 0); }   // doors
        for (int r = 0; r < 7; r++)                                                      // gable roof
            for (int x = hx - 1 + r; x <= hx + hw - r; x++) {
                Put(x, gy - hh - r, TileID.WoodBlock);
                if (r > 0 && x > hx - 1 + r && x < hx + hw - r) Wall(x, gy - hh - r, WallID.Wood);
            }
        // stone foundation line and a stone ledge by the tower
        for (int x = hx - 1; x <= hx + hw; x++) Put(x, gy - hh, x > hx && x < hx + hw - 1 ? TileID.WoodBlock : TileID.WoodBlock);

        // furniture and light
        int fy = gy - 1;
        WorldGen.PlaceObject(hx + 5, fy, TileID.WorkBenches, true, 0);
        WorldGen.PlaceObject(hx + 11, fy, TileID.Campfire, true, 0);
        WorldGen.PlaceObject(hx + 16, fy, TileID.Tables, true, 0);
        WorldGen.PlaceTile(hx + 3, gy - 6, TileID.Torches, true, true);
        WorldGen.PlaceTile(hx + hw - 4, gy - 6, TileID.Torches, true, true);
        WorldGen.PlaceTile(hx + 9, gy - 6, TileID.Torches, true, true);
        WorldGen.PlaceTile(hx + 14, gy - 6, TileID.Torches, true, true);
        int ci = WorldGen.PlaceChest(hx + 18, fy, TileID.Containers, false, 0);
        if (ci >= 0) {
            Chest c = Main.chest[ci];
            c.item[0].SetDefaults(ModContent.ItemType<CheeseWheel>()); c.item[0].stack = 12;
            c.item[1].SetDefaults(ItemID.HealingPotion); c.item[1].stack = 5;
            c.item[2].SetDefaults(ItemID.GoldCoin); c.item[2].stack = 30;
        }
        for (int x = sx - 18; x < sx + 27; x++) for (int y = gy - 30; y < gy + 4; y++) WorldGen.SquareTileFrame(x, y, true);

        foreach (Item it in Main.item)   // tree stumps and acorns left by the clearing
            if (it.active && (it.type == ItemID.Wood || it.type == ItemID.Acorn || it.type == ItemID.DirtBlock || it.type == ItemID.StoneBlock)) it.active = false;
        GateX = hx + hw / 2;
        SkyrimAtmosphere.WhiterunPos = new Vector2(GateX * 16 + 8, (gy - hh - 15) * 16);
        SkyrimAtmosphere.WhiterunBuilt = true;
    }
}
