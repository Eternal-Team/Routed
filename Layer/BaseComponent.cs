using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class BaseComponent
	{
		public Duct Parent;

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