﻿using BaseLibrary;
using ContainerLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Routed.Modules
{
	public class ExtractorModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.ExtractorModule>();

		public ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		private int timer;
		private const int maxTimer = 45;

		public override void Update()
		{
			if (timer++ < maxTimer) return;
			timer = 0;

			ItemHandler handler = GetHandler();
			if (handler == null) return;

			for (int i = 0; i < handler.Slots; i++)
			{
				Item item = handler.ExtractItem(i, 10);

				if (!item.IsAir)
				{
					foreach (MarkerModule module in Parent.Network.MarkerModules.Where(module => module.GetHandler() != null))
					{
						//if(module.IsItemValid(item))

						Parent.Network.NetworkItems.Add(new NetworkItem(item, Pathfinding.FindPath(Parent.Network.Tiles, Parent.Position, module.Parent.Position)));
						break;
					}

					break;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(Parent.Position.X * 16 - Main.screenPosition.X), (int)(Parent.Position.Y * 16 - Main.screenPosition.Y), 16, 16), Color.Red * 0.5f);
		}
	}
}