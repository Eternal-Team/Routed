using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class ProviderModule : BaseModule, IHasUI
	{
		public enum Mode
		{
			TakeAll,
			LeaveStack,
			Leave1
		}

		public override int DropItem => ModContent.ItemType<Items.ProviderModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public Mode mode = Mode.TakeAll;

		public ProviderModule()
		{
			UUID = Guid.NewGuid();
		}

		public int GetAvailableItems(int type)
		{
			int count = CachedHandler.CountItems(type);
			switch (mode)
			{
				case Mode.Leave1:
					return count - 1;
				case Mode.LeaveStack:
				{
					int max = 0;
					for (int i = 0; i < CachedHandler.Slots; i++)
					{
						Item item = CachedHandler.GetItemInSlot(i);
						if (item.IsAir || item.type != type) continue;
						if (item.stack > max) max = item.stack;
					}

					return count - max;
				}
				default:
					return count;
			}
		}

		public override bool Interact()
		{
			if (!Main.dedServ) PanelUI.Instance.HandleUI(this);

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ProviderModule"), position, Color.White);
		}

		public override void Load(TagCompound tag)
		{
			base.Load(tag);

			UUID = tag.Get<Guid>("UUID");
			mode = (Mode)tag.GetInt("Mode");
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Mode"] = (int)mode
		};
	}
}