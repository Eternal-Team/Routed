using BaseLibrary;
using BaseLibrary.UI.New;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class ProviderModule : BaseModule
	{
		public override int DropItem => ModContent.ItemType<Items.ProviderModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public ProviderModule()
		{
			UUID = Guid.NewGuid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ProviderModule"), position, Color.White);
		}

		public override void Update()
		{
		}

		public override ItemHandler GetHandler()
		{
			if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler) return handler.Handler;
			return null;
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID
		};
	}
}