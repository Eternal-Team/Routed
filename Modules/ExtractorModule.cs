﻿using BaseLibrary;
using ContainerLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Routed.Modules
{
	public class ExtractorModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.ExtractorModule>();

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		private int timer;
		private const int maxTimer = 30;

		public override void Update()
		{
			if (timer++ < maxTimer) return;
			timer = 0;

			ItemHandler handler = GetHandler();
			if (handler == null) return;

			// todo: add conditions to extraction (leave x in, take specific items, etc.)

			int index = Array.FindIndex(handler.Items, i => !i.IsAir);
			if (index < 0) return;

			Item item = handler.ExtractItem(index, 10);

			foreach (MarkerModule module in Parent.Network.MarkerModules.Where(module => module.GetHandler() != null))
			{
				if (!module.IsItemValid(item)) continue;

				Parent.Network.NetworkItems.Add(new NetworkItem(item, Pathfinding.FindPath(Parent.Network.Tiles, Parent.Position, module.Parent.Position)));
				break;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ExtractorModule"), position, Color.White);
		}
	}
}