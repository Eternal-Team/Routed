﻿using BaseLibrary;
using ContainerLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Routed.Layer
{
	public class NetworkItem
	{
		public Item item;

		public Stack<Point16> path;
		public Point16 CurrentPosition;
		public Point16 PreviousPosition;

		public const int speed = 30;
		public int timer=speed;

		public NetworkItem(Item item, Stack<Point16> path)
		{
			this.item = item;
			this.path = path;
			CurrentPosition = PreviousPosition = path.Pop();
		}

		public void Update()
		{
			if (++timer >= speed)
			{
				PreviousPosition = CurrentPosition;

				if (path.Count == 0)
				{
					if (Utility.TryGetTileEntity(CurrentPosition, out ModTileEntity te) && te is IItemHandler handler)
					{
						handler.Handler.InsertItem(ref item);
					}

					if (!item.IsAir)
					{
						Item.NewItem(CurrentPosition.X * 16, CurrentPosition.Y * 16, 16, 16, item.type, item.stack, pfix: item.prefix);
						item.TurnToAir();
					}
				}
				else
				{
					CurrentPosition = path.Pop();
					// check if current position exists in network
				}

				timer = 0;
			}
		}
	}
}