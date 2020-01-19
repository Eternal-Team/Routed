using BaseLibrary.Items;
using Terraria.ID;

namespace Routed.Items
{
	public class PotatoKnish : BaseItem
	{
		public override string Texture => "Routed/Textures/Items/PotatoKnish";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Potato Knish");
			Tooltip.SetDefault("https://www.youtube.com/watch?v=IFfLCuHSZ-U");
		}

		public override void SetDefaults()
		{
			item.width = 24;
			item.height = 16;
			item.maxStack = 999;
			item.rare = ItemRarityID.Expert;
		}
	}
}