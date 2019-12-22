﻿using ContainerLibrary;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public abstract class BaseModule
	{
		public abstract int DropItem { get; }

		public Duct Parent;

		public virtual ItemHandler GetHandler() => null;

		public virtual bool IsItemValid(Item item) => true;

		public virtual void Draw(SpriteBatch spriteBatch)
		{
		}

		public virtual void Update()
		{
		}

		public virtual bool Interact() => false;

		public virtual TagCompound Save() => new TagCompound();

		public virtual void Load(TagCompound tag)
		{
		}
	}
}