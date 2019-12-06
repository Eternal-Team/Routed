using LayerLibrary;
using Routed.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class RoutedLayer : ModLayer<Duct>
	{
		public override int TileSize => 1;

		public override void Load(List<TagCompound> list)
		{
			base.Load(list);

			foreach (Duct duct in data.Values) duct.Merge();
		}

		public bool PlaceComponent<T>(BaseComponentItem<T> item) where T : BaseComponent, new()
		{
			if (TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct) && duct.Component == null)
			{
				duct.Component = new T { Parent = duct };
				return true;
			}

			return false;
		}

		public void RemoveComponent()
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (TryGetValue(posX, posY, out Duct duct) && duct.Component != null)
			{
				//Item.NewItem(posX * 16, posY * 16, 16, 16, duct.Component.DropItem);
				duct.Component = null;
			}
		}

		public override void Remove()
		{
			RemoveComponent();
			base.Remove();
		}
	}
}