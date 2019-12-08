using BaseLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
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

		public override void Load(List<TagCompound> list)
		{
			base.Load(list);

			foreach (Duct duct in data.Values) duct.Merge();
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

		public override void Remove()
		{
			RemoveModule();
			base.Remove();
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

			foreach (NetworkItem item in RoutedNetwork.Networks.SelectMany(network => network.NetworkItems))
			{
				Vector2 previous = item.PreviousPosition.ToScreenCoordinates(false) + new Vector2(8);
				Vector2 current = item.CurrentPosition.ToScreenCoordinates(false) + new Vector2(8);

				Vector2 position = Vector2.Lerp(previous, current, item.timer / (float)NetworkItem.speed);

				spriteBatch.DrawItemInWorld(item.item, position, new Vector2(14));
			}
		}
	}
}