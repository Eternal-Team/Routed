using BaseLibrary.Items;
using Microsoft.Xna.Framework;
using Routed.Layer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Routed.Items
{
	public abstract class BaseModuleItem : BaseItem
	{
	}

	public abstract class BaseModuleItem<T> : BaseModuleItem where T : BaseModule, new()
	{
		public override void SetDefaults()
		{
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.rare = 0;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 10;
			item.consumable = true;
			item.useTurn = true;
			item.autoReuse = true;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool ConsumeItem(Player player)
		{
			if (player.altFunctionUse == 2) ModContent.GetInstance<Routed>().RoutedLayer.RemoveModule();
			else return ModContent.GetInstance<Routed>().RoutedLayer.PlaceModule(this);

			return false;
		}

		public override bool UseItem(Player player)
		{
			Rectangle rectangle = new Rectangle(
				(int)(player.position.X + player.width * 0.5f - Player.tileRangeX * 32),
				(int)(player.position.Y + player.height * 0.5f - Player.tileRangeY * 32),
				Player.tileRangeX * 64,
				Player.tileRangeY * 64);

			return rectangle.Contains(Player.tileTargetX * 16, Player.tileTargetY * 16);
		}
	}
}