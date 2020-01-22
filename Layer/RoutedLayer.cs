﻿using BaseLibrary;
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

		public override void Draw(SpriteBatch spriteBatch)
		{
			SpriteBatchState state = Utility.End(spriteBatch);
			Main.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

			base.Draw(spriteBatch);

			foreach (RoutedNetwork network in RoutedNetwork.Networks) network.Draw(spriteBatch);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(state);
		}

		public override bool Interact()
		{
			if (Main.LocalPlayer.HeldItem.modItem is BasicDuct) return false;
			if (Main.LocalPlayer.HeldItem.modItem is BaseModuleItem) return false;

			return TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct) && duct.Interact();
		}

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

				element.UpdateFrame();
				foreach (Duct neighbor in element.GetVisualNeighbors()) neighbor.UpdateFrame();

				element.OnPlace(item);
				return true;
			}

			return false;
		}

		public bool PlaceModule<T>(BaseModuleItem<T> item) where T : BaseModule, new()
		{
			if (TryGetValue(Player.tileTargetX, Player.tileTargetY, out Duct duct) && duct.Module == null)
			{
				duct.Module = new T { Parent = duct };
				duct.Module.OnPlace(item);

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
				data.Remove(new Point16(posX, posY));
				foreach (Duct neighbor in element.GetVisualNeighbors()) neighbor.UpdateFrame();
				RemoveModule();
				element.OnRemove();

				Item.NewItem(posX * 16, posY * 16, TileSize * 16, TileSize * 16, element.DropItem);
			}
		}

		public void RemoveModule()
		{
			int posX = Player.tileTargetX;
			int posY = Player.tileTargetY;

			if (TryGetValue(posX, posY, out Duct duct) && duct.Module != null)
			{
				duct.Module.OnRemove();
				Item.NewItem(posX * 16, posY * 16, 16, 16, duct.Module.DropItem);
				duct.Module = null;
			}
		}

		public override List<TagCompound> Save() => RoutedNetwork.Networks.Select(network => network.Save()).ToList();

		public override void Update()
		{
			base.Update();

			foreach (RoutedNetwork network in RoutedNetwork.Networks) network.Update();
		}
	}
}