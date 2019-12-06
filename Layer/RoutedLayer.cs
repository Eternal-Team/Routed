using LayerLibrary;
using Routed.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class RoutedLayer : ModLayer<Cable>
	{
		public override int TileSize => 1;

		public override void Load(List<TagCompound> list)
		{
			base.Load(list);

			foreach (Cable tube in data.Values) tube.Merge();
		}

		public bool PlaceComponent<T>(BaseComponentItem<T> item) where T : BaseComponent, new()
		{
			if (TryGetValue(Player.tileTargetX, Player.tileTargetY, out Cable cable) && cable.Component == null)
			{
				cable.Component = new T { Parent = cable };
				return true;
			}

			return false;
		}

		public void RemoveComponent()
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (TryGetValue(posX, posY, out Cable cable) && cable.Component != null)
			{
				//Item.NewItem(posX * 16, posY * 16, 16, 16, cable.Component.DropItem);
				cable.Component = null;
			}
		}

		public override void Remove()
		{
			RemoveComponent();
			base.Remove();
		}
	}
}