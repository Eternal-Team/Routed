using Routed.Modules;
using System.Collections.Generic;
using System.Linq;

namespace Routed.Layer
{
	public class RoutedNetwork
	{
		public static List<RoutedNetwork> Networks = new List<RoutedNetwork>();

		public List<Duct> Tiles { get; }

		public List<ProviderModule> ProviderModules => Tiles.Select(duct => duct.Module).OfType<ProviderModule>().ToList();

		public List<ConsumerModule> ConsumerModules => Tiles.Select(duct => duct.Module).OfType<ConsumerModule>().ToList();

		public List<MarkerModule> MarkerModules => Tiles.Select(duct => duct.Module).OfType<MarkerModule>().ToList();

		public List<ExtractorModule> ExtractorModules => Tiles.Select(duct => duct.Module).OfType<ExtractorModule>().ToList();

		public List<NetworkItem> NetworkItems = new List<NetworkItem>();

		public RoutedNetwork(Duct duct)
		{
			Networks.Add(this);

			Tiles = new List<Duct> { duct };
		}

		public void Update()
		{
			for (int i = 0; i < NetworkItems.Count; i++)
			{
				NetworkItem item = NetworkItems[i];
				item.Update();
				if (item.item == null || item.item.IsAir) NetworkItems.Remove(item);
			}
		}

		public void AddTile(Duct tile)
		{
			if (!Tiles.Contains(tile))
			{
				Networks.Remove(tile.Network);
				tile.Network = this;
				Tiles.Add(tile);
			}
		}

		public void RemoveTile(Duct tile)
		{
			if (Tiles.Contains(tile))
			{
				Tiles.Remove(tile);
				Reform();
			}
		}

		public void Merge(RoutedNetwork other)
		{
			for (int i = 0; i < other.Tiles.Count; i++) AddTile(other.Tiles[i]);

			NetworkItems.AddRange(other.NetworkItems);
		}

		public void Reform()
		{
			Networks.Remove(this);

			for (int i = 0; i < Tiles.Count; i++) Tiles[i].Network = new RoutedNetwork(Tiles[i]);

			for (int i = 0; i < Tiles.Count; i++) Tiles[i].Merge();
		}
	}
}