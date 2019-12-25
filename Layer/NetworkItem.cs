using BaseLibrary;
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
		public const int speed = 20;
		public Point16 CurrentPosition;
		public Item item;

		public Stack<Point16> path;
		public Point16 PreviousPosition;
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

		public TagCompound Save() => new TagCompound
		{
			["Item"] = item,
			["Position"] = CurrentPosition,
			["Destination"] = path.Count == 0 ? CurrentPosition : path.Last()
		};

		public void Update()
		{
			if (++timer >= speed)
			{
				PreviousPosition = CurrentPosition;

				if (path.Count == 0)
				{
					if (ModContent.GetInstance<Routed>().RoutedLayer.TryGetValue(CurrentPosition, out Duct duct) && duct.Module != null)
					{
						BaseModule module = duct.Module;
						ItemHandler handler = module.GetHandler();
						handler?.InsertItem(ref item);
					}

					if (!item.IsAir)
					{
						Utility.NewItem(CurrentPosition.X * 16, CurrentPosition.Y * 16, 16, 16, item);
						item.TurnToAir();
					}
				}
				else
				{
					CurrentPosition = path.Pop();

					if (!ModContent.GetInstance<Routed>().RoutedLayer.ContainsKey(CurrentPosition))
					{
						Utility.NewItem(CurrentPosition.X * 16, CurrentPosition.Y * 16, 16, 16, item);
						item.TurnToAir();
					}
				}

				timer = 0;
			}
		}
	}
}