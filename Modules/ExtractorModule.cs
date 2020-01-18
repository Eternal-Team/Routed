using BaseLibrary;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Routed.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Modules
{
	public class ExtractorModule : BaseModule, IHasUI
	{
		public override int DropItem
		{
			get
			{
				switch (ItemsPerExtraction)
				{
					case 10:
						return ModContent.ItemType<BasicExtractorModule>();
					case 100:
						return ModContent.ItemType<AdvancedExtractorModule>();
					case 1000:
						return ModContent.ItemType<EliteExtractorModule>();
					default:
						return 0;
				}
			}
		}

		private int ExtractionSpeed;
		private int ItemsPerExtraction;

		private int timer;

		public List<int> FilteredItems = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Parent.Position.ToScreenCoordinates(false);
			spriteBatch.Draw(ModContent.GetTexture("Routed/Textures/Modules/ExtractorModule"), position, Color.White);
		}

		public override void Load(TagCompound tag)
		{
			ExtractionSpeed = tag.GetInt("ExtractionSpeed");
			ItemsPerExtraction = tag.GetInt("ItemsPerExtraction");
			FilteredItems = tag.GetList<int>("Filter").ToList();
		}

		public override bool Interact()
		{
			if (Main.netMode != NetmodeID.Server) PanelUI.Instance.HandleUI(this);

			return true;
		}

		public override void OnPlace(BaseModuleItem item)
		{
			if (item is Items.ExtractorModule module)
			{
				ExtractionSpeed = module.ExtractionSpeed;
				ItemsPerExtraction = module.ItemsPerExtraction;
			}
		}

		public override TagCompound Save() => new TagCompound
		{
			["ExtractionSpeed"] = ExtractionSpeed,
			["ItemsPerExtraction"] = ItemsPerExtraction,
			["Filter"] = FilteredItems
		};

		protected override void Update()
		{
			if (timer++ < ExtractionSpeed) return;
			timer = 0;

			ItemHandler handler = GetHandler();
			if (handler == null) return;

			for (int i = 0; i < handler.Slots; i++)
			{
				if (handler.Items[i] == null || handler.Items[i].IsAir) continue;

				if (FilteredItems.Any(filter => filter != -1) && !FilteredItems.Contains(handler.Items[i].type)) continue;

				Item item = handler.ExtractItem(i, ItemsPerExtraction);

				if (Network.PushItem(item, Parent)) break;

				handler.InsertItem(ref item);
			}
		}

		public Guid UUID { get; set; }
		public BaseUIPanel UI { get; set; }
		public LegacySoundStyle CloseSound { get; }
		public LegacySoundStyle OpenSound { get; }
	}
}