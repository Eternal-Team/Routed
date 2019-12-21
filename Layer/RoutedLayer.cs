using LayerLibrary;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class RoutedLayer : ModLayer<Duct>
	{
		public override int TileSize => 1;

		public override bool Interact()
		{
			if (Main.LocalPlayer.HeldItem.modItem is BasicDuct) return false;
			if (Main.LocalPlayer.HeldItem.modItem is BaseModuleItem) return false;

			return TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct) && duct.Interact();
		}

		public Dictionary<Point16, Duct> a => data;

		public override List<TagCompound> Save() => RoutedNetwork.Networks.Select(network => network.Save()).ToList();

		public override void Load(List<TagCompound> list)
		{
			data.Clear();
			RoutedNetwork.Networks.Clear();

			foreach (TagCompound compound in list)
			{
				RoutedNetwork network = new RoutedNetwork();
				network.Load(compound);
			}

			foreach (Duct duct in data.Values) duct.UpdateFrame();
		}

		public override bool Place(BaseLayerItem item)
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (!ContainsKey(posX, posY))
			{
				Duct element = new Duct
				{
					Position = new Point16(posX, posY),
					Frame = Point16.Zero,
					Layer = this
				};
				data.Add(new Point16(posX, posY), element);
				element.OnPlace();

				element.UpdateFrame();
				foreach (Duct neighbor in element.GetVisualNeighbors()) neighbor.UpdateFrame();

				return true;
			}

			return false;
		}

		public bool PlaceModule<T>(BaseModuleItem<T> item) where T : BaseModule, new()
		{
			if (TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct) && duct.Module == null)
			{
				duct.Module = new T { Parent = duct };
				return true;
			}

			return false;
		}

		public override void Remove()
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (TryGetValue(posX, posY, out Duct element))
			{
				RemoveModule();
				element.OnRemove();
				data.Remove(new Point16(posX, posY));

				foreach (Duct neighbor in element.GetVisualNeighbors()) neighbor.UpdateFrame();

				Item.NewItem(posX * 16, posY * 16, TileSize * 16, TileSize * 16, element.DropItem);
			}
		}

		public void RemoveModule()
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (TryGetValue(posX, posY, out Duct duct) && duct.Module != null)
			{
				Item.NewItem(posX * 16, posY * 16, 16, 16, duct.Module.DropItem);
				duct.Module = null;
			}
		}

		public override void Update()
		{
			base.Update();

			foreach (RoutedNetwork network in RoutedNetwork.Networks)
			{
				network.Update();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			foreach (RoutedNetwork network in RoutedNetwork.Networks) network.Draw(spriteBatch);
		}
	}
}