﻿using ContainerLibrary;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public abstract class BaseModule
	{
		public abstract int DropItem { get; }

		public RoutedNetwork Network => Parent.Network;
		public Duct Parent;

		public virtual void Draw(SpriteBatch spriteBatch)
		{
		}

		public virtual void Update()
		{
		}

		public virtual ItemHandler GetHandler() => null;

		public virtual void OnPlace(BaseModuleItem item)
		{
		}

		public virtual void OnRemove()
		{
		}

		public virtual bool Interact() => false;

		public virtual bool IsItemValid(Item item) => true;

		public virtual TagCompound Save() => new TagCompound();

		public virtual void Load(TagCompound tag)
		{
		}
	}
}