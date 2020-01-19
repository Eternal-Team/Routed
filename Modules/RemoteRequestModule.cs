using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Routed.Layer;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class RemoteRequestModule : BaseModule, IHasUI
	{
		public override int DropItem => ModContent.ItemType<Items.RequesterModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public RemoteRequester Requester;

		public RemoteRequestModule()
		{
			UUID = Guid.NewGuid();
		}

		public override ItemHandler GetHandler() => Requester?.Handler;

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/RequesterModule"), position, Color.White);
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