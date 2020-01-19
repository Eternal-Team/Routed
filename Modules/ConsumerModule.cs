using BaseLibrary;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	// note: specify amount to keep in-stock
	public class ConsumerModule : BaseModule, IHasUI
	{
		public override int DropItem => ModContent.ItemType<Items.ConsumerModule>();

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound => SoundID.Item1;
		public LegacySoundStyle OpenSound => SoundID.Item1;

		public List<int> Items = Enumerable.Repeat(-1, 9).ToList();

		public ConsumerModule()
		{
			UUID = Guid.NewGuid();
		}

		private int timer;
		private const int maxTimer = 15;

		protected override void Update()
		{
			if (timer++ < maxTimer) return;
			timer = 0;

			foreach (int item in Items)
			{
				if (item <= 0) continue;

				Network.PullItem(item, 100, Parent);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ConsumerModule"), position, Color.White);
		}

		public override bool Interact()
		{
			PanelUI.Instance.HandleUI(this);

			return true;
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			Items = tag.GetList<int>("Items").ToList();
		}

		public override TagCompound Save() => new TagCompound
		{
			["UUID"] = UUID,
			["Items"] = Items
		};
	}
}