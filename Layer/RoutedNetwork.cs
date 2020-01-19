using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Modules;
using Routed.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class AutoAddDictionary<T, K> : Dictionary<T, K>
	{
		public new K this[T key]
		{
			get
			{
				if (TryGetValue(key, out K value)) return value;

				Add(key, default);
				return base[key];
			}
			set
			{
				if (ContainsKey(key)) base[key] = value;
				else Add(key, value);
			}
		}
	}

	public class RoutedNetwork
	{
		public static List<RoutedNetwork> Networks = new List<RoutedNetwork>();

		public List<ProviderModule> ProviderModules => Tiles.Select(duct => duct.Module).OfType<ProviderModule>().ToList();

		public List<ConsumerModule> ConsumerModules => Tiles.Select(duct => duct.Module).OfType<ConsumerModule>().ToList();

		public List<MarkerModule> MarkerModules => Tiles.Select(duct => duct.Module).OfType<MarkerModule>().ToList();

		public List<ExtractorModule> ExtractorModules => Tiles.Select(duct => duct.Module).OfType<ExtractorModule>().ToList();

		public AutoAddDictionary<int, int> ItemCache = new AutoAddDictionary<int, int>();

		public List<NetworkItem> NetworkItems = new List<NetworkItem>();

		public List<Duct> Tiles = new List<Duct>();

		public Dictionary<ItemHandler, List<BaseModule>> HandlerCache = new Dictionary<ItemHandler, List<BaseModule>>();

		internal RoutedNetwork()
		{
			Networks.Add(this);
		}

		public bool PullItem(int type, int count, Duct destination)
		{
			foreach (ProviderModule module in ProviderModules)
			{
				ItemHandler handler = module.GetHandler();
				if (handler == null) continue;

				int available = module.GetAvailableItems(type);
				if (available <= 0) continue;

				for (int i = 0; i < handler.Slots; i++)
				{
					Item item = handler.Items[i];
					if (item.IsAir || item.type != type) continue;

					int extractedAmount = Utility.Min(count, item.stack, available);
					if (extractedAmount <= 0) continue;

					NetworkItem networkItem = new NetworkItem(handler.ExtractItem(i, extractedAmount), module.Parent, destination);
					NetworkItems.Add(networkItem);

					ItemCache[item.netID] -= extractedAmount;
					if (ItemCache[item.netID] <= 0) ItemCache.Remove(item.type);

					UpdateUIs();

					count -= extractedAmount;
					available -= extractedAmount;
					if (count <= 0) return true;
				}
			}

			return false;
		}

		public bool PushItem(Item item, Duct origin)
		{
			foreach (MarkerModule module in MarkerModules.Where(markerModule =>
			{
				var other = markerModule.GetHandler();
				if (other == null) return false;
				return other.HasSpace(item) && markerModule.IsItemValid(item);
			}).OrderByDescending(markerModule => markerModule.priority))
			{
				ItemHandler handler = module.GetHandler();

				int sum = 0;
				for (int i = 0; i < handler.Slots; i++)
				{
					Item slot = handler.Items[i];
					int slotLimit = handler.GetItemLimit(i) ?? item.maxStack;

					if (slot.IsAir) sum += slotLimit;
					else if (slot.type == item.type) sum += slotLimit - slot.stack;
				}

				Item sent = new Item();
				sent.SetDefaults(item.type);
				int extracted = Math.Min(sum, item.stack);
				sent.stack = extracted;
				item.stack -= extracted;

				NetworkItems.Add(new NetworkItem(sent, origin, module.Parent));

				if (item.stack <= 0) return true;
			}

			return false;
		}

		internal void CheckPaths()
		{
			IEnumerable<Point16> tiles = Tiles.Select(duct => duct.Position).ToList();

			foreach (NetworkItem item in NetworkItems.Where(item => item.path.Any(position => !tiles.Contains(position))))
			{
				item.path = Pathfinding.FindPath(Routed.RoutedLayer[item.CurrentPosition], item.destination);
			}
		}

		internal void Draw(SpriteBatch spriteBatch)
		{
			foreach (NetworkItem item in NetworkItems)
			{
				Vector2 previous = item.PreviousPosition.ToScreenCoordinates(false) + new Vector2(8);
				Vector2 current = item.CurrentPosition.ToScreenCoordinates(false) + new Vector2(8);

				Vector2 position = Vector2.Lerp(previous, current, item.timer / (float)NetworkItem.speed);

				spriteBatch.DrawItemInWorld(item.item, position, new Vector2(14));
			}
		}

		internal void Load(TagCompound tag)
		{
			RoutedLayer layer = Routed.RoutedLayer;
			Tiles = new List<Duct>();
			NetworkItems = new List<NetworkItem>();

			foreach (TagCompound compound in tag.GetList<TagCompound>("Tiles"))
			{
				Duct element = new Duct
				{
					Position = compound.Get<Point16>("Position"),
					Layer = layer,
					Network = this
				};
				element.Load(compound.GetCompound("Data"));

				layer[element.Position] = element;
				Tiles.Add(element);
			}

			foreach (TagCompound compound in tag.GetList<TagCompound>("NetworkItems")) NetworkItems.Add(new NetworkItem(compound));

			RegenerateCache();
		}

		internal void RegenerateCache()
		{
			ItemCache.Clear();

			foreach (ProviderModule module in ProviderModules)
			{
				ItemHandler handler = module.GetHandler();
				if (handler == null) continue;

				foreach (Item item in handler.Items)
				{
					if (item.IsAir) continue;

					ItemCache[item.netID] += item.stack;
				}
			}
		}

		internal TagCompound Save() => new TagCompound
		{
			["Tiles"] = Tiles.Select(duct => new TagCompound
			{
				["Position"] = duct.Position,
				["Data"] = duct.Save()
			}).ToList(),
			["NetworkItems"] = NetworkItems.Select(item => item.Save()).ToList()
		};

		internal void Update()
		{
			HandlerCache.Clear();

			for (int i = 0; i < Tiles.Count; i++)
			{
				Duct duct = Tiles[i];
				ItemHandler handler = duct.Module?.GetHandler();
				if (handler != null)
				{
					if (HandlerCache.ContainsKey(handler)) HandlerCache[handler].Add(duct.Module);
					else HandlerCache.Add(handler, new List<BaseModule> { duct.Module });
				}
			}

			Main.NewText("Tracking " + HandlerCache.Count + " handlers and " + HandlerCache.Sum(pair => pair.Value.Count) + " modules");

			for (int i = 0; i < NetworkItems.Count; i++)
			{
				NetworkItem item = NetworkItems[i];
				item.Update();
				if (item.item == null || item.item.IsAir) NetworkItems.Remove(item);
			}
		}

		internal void UpdateUIs()
		{
			foreach (BaseElement element in PanelUI.Instance.Children)
			{
				if (element is RequesterModulePanel panel) panel.UpdateGrid();
			}
		}
	}
}