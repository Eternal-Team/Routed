using BaseLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Modules;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class RoutedNetwork
	{
		public static List<RoutedNetwork> Networks = new List<RoutedNetwork>();

		internal Color debugColor;

		public List<NetworkItem> NetworkItems = new List<NetworkItem>();

		public RoutedNetwork()
		{
			Networks.Add(this);

			debugColor = Utility.RandomColor();
		}

		public List<Duct> Tiles { set; get; }

		public List<ProviderModule> ProviderModules => Tiles.Select(duct => duct.Module).OfType<ProviderModule>().ToList();

		public List<ConsumerModule> ConsumerModules => Tiles.Select(duct => duct.Module).OfType<ConsumerModule>().ToList();

		public List<MarkerModule> MarkerModules => Tiles.Select(duct => duct.Module).OfType<MarkerModule>().ToList();

		public List<ExtractorModule> ExtractorModules => Tiles.Select(duct => duct.Module).OfType<ExtractorModule>().ToList();

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (NetworkItem item in NetworkItems)
			{
				Vector2 previous = item.PreviousPosition.ToScreenCoordinates(false) + new Vector2(8);
				Vector2 current = item.CurrentPosition.ToScreenCoordinates(false) + new Vector2(8);

				Vector2 position = Vector2.Lerp(previous, current, item.timer / (float)NetworkItem.speed);

				spriteBatch.DrawItemInWorld(item.item, position, new Vector2(14));
			}
		}

		public void Load(TagCompound tag)
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

			foreach (TagCompound compound in tag.GetList<TagCompound>("NetworkItems"))
			{
				NetworkItems.Add(new NetworkItem(compound));
			}
		}

		public TagCompound Save()
		{
			return new TagCompound
			{
				["Tiles"] = Tiles.Select(duct => new TagCompound
				{
					["Position"] = duct.Position,
					["Data"] = duct.Save()
				}).ToList(),
				["NetworkItems"] = NetworkItems.Select(item => item.Save()).ToList()
			};
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

		public void CheckPaths()
		{
			IEnumerable<Point16> tiles = Tiles.Select(duct => duct.Position).ToList();

			foreach (NetworkItem item in NetworkItems.Where(item => item.path.Any(position => !tiles.Contains(position))))
			{
				item.path = Pathfinding.FindPath(Routed.RoutedLayer[item.CurrentPosition], item.destination);
			}
		}
	}
}