using System.Collections.Generic;
using Terraria;

namespace Routed.Layer
{
	public class RoutedNetwork
	{
		public static List<RoutedNetwork> Networks = new List<RoutedNetwork>();

		public List<Cable> Tiles { get; }

		public RoutedNetwork(Cable tube)
		{
			Networks.Add(this);

			Tiles = new List<Cable> { tube };
		}

		public void AddTile(Cable tile)
		{
			if (!Tiles.Contains(tile))
			{
				Networks.Remove(tile.Network);
				tile.Network = this;
				Tiles.Add(tile);
			}
		}

		public void RemoveTile(Cable tile)
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
		}

		public void Reform()
		{
			Networks.Remove(this);

			for (int i = 0; i < Tiles.Count; i++) Tiles[i].Network = new RoutedNetwork(Tiles[i]);

			for (int i = 0; i < Tiles.Count; i++) Tiles[i].Merge();
		}
	}
}