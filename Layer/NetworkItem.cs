using BaseLibrary;
using ContainerLibrary;
using LayerLibrary;
using Routed.Modules;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class NetworkItem
	{
		public const int speed = 20;
		public Point16 CurrentPosition;
		public Duct destination;
		public Item item;

		public Duct origin;

		public Stack<Point16> path;
		public Point16 PreviousPosition;
		public int timer = speed;

		public NetworkItem(Item item, Duct origin, Duct destination)
		{
			this.item = item;
			this.origin = origin;
			this.destination = destination;

			path = Pathfinding.FindPath(origin, destination);
			CurrentPosition = PreviousPosition = path.Pop();
		}

		public NetworkItem(TagCompound tag)
		{
			item = tag.Get<Item>("Item");
			origin = Routed.RoutedLayer[tag.Get<Point16>("Position")];
			destination = Routed.RoutedLayer[tag.Get<Point16>("Destination")];

			path = Pathfinding.FindPath(origin, destination);
			CurrentPosition = PreviousPosition = path.Pop();
		}

		public TagCompound Save() => new TagCompound
		{
			["Item"] = item,
			["Position"] = CurrentPosition,
			["Destination"] = destination.Position
		};

		public void Update()
		{
			if (++timer >= speed)
			{
				PreviousPosition = CurrentPosition;

				if (path.Count == 0)
				{
					if (Routed.RoutedLayer.TryGetValue(CurrentPosition, out Duct duct) && duct.Module != null)
					{
						BaseModule module = duct.Module;
						ItemHandler handler = module.GetHandler();
						if (handler != null)
						{
							int count = item.stack;
							int id = item.netID;

							handler.InsertItem(ref item);

							if (duct.Network.HandlerCache.TryGetValue(handler, out var list) && list.Any(baseModule => baseModule is ProviderModule))
							{
								duct.Network.ItemCache[id] += count - item.stack;
							}

							duct.Network.UpdateUIs();
						}
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

					if (!Routed.RoutedLayer.ContainsKey(CurrentPosition))
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