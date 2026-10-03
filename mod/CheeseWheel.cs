using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

// Heals a bit, like every good Nord meal.
public class CheeseWheel : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()
    {
        Item.width = 28; Item.height = 24; Item.maxStack = 99;
        Item.useStyle = ItemUseStyleID.EatFood; Item.useTime = 15; Item.useAnimation = 15;
        Item.useTurn = true; Item.consumable = true; Item.UseSound = SoundID.Item2;
        Item.healLife = 60; Item.potion = true; Item.rare = ItemRarityID.Green; Item.value = 500;
    }
}
