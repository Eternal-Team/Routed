using ContainerLibrary;
using LayerLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class NetworkItem
	{
		public Item item;

		public Stack<Point16> path;
		public Point16 CurrentPosition;
		public Point16 PreviousPosition;

		public const int speed = 5;
		public int timer = speed;

		public NetworkItem(Item item, Stack<Point16> path)
		{
			this.item = item;
			this.path = path;
			CurrentPosition = PreviousPosition = path.Pop();
		}

		public NetworkItem(TagCompound tag, RoutedNetwork network)
		{
			item = tag.Get<Item>("Item");
			CurrentPosition = PreviousPosition = tag.Get<Point16>("Position");
			path = Pathfinding.FindPath(network.Tiles, CurrentPosition, tag.Get<Point16>("Destination"));
		}

		public void Update()
		{
			if (++timer >= speed)
			{
				PreviousPosition = CurrentPosition;

				if (path.Count == 0)
				{
					if (ModContent.GetInstance<Routed>().RoutedLayer.TryGetValue(CurrentPosition, out Duct duct))
					{
						BaseModule module = duct.Module;
						ItemHandler handler = module.GetHandler();
						handler?.InsertItem(ref item);
					}

					if (!item.IsAir)
					{
						int i = Item.NewItem(CurrentPosition.X * 16, CurrentPosition.Y * 16, 16, 16, item.type, item.stack, pfix: item.prefix);
						Main.item[i] = Main.item[i].CloneWithModdedDataFrom(item);

						item.TurnToAir();
					}
				}
				else
				{
					CurrentPosition = path.Pop();
					// todo: check if current position exists in network
				}

				timer = 0;
			}
		}

		public TagCompound Save() => new TagCompound
		{
			["Item"] = item,
			["Position"] = CurrentPosition,
			["Destination"] = path.Count == 0 ? CurrentPosition : path.Last()
		};
	}
}