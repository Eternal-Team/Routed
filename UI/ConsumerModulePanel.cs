using BaseLibrary;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Modules;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Routed.UI
{
	public class ConsumerModulePanel : BaseUIPanel<ConsumerModule>
	{
		public ConsumerModulePanel(ConsumerModule module) : base(module)
		{
			Width.Percent = Height.Percent = 25;


			UIText textLabel = new UIText("Consumer Module")
			{
				Width = { Percent = 100 },
				Height = { Pixels = 20 },
				X = { Percent = 50 },
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Add(textLabel);

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Pixels = -20, Percent = 100 },
				Padding = Padding.Zero,
				RenderPanel = false
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UIGrid<UIConsumerSlot> gridSlots = new UIGrid<UIConsumerSlot>(3)
			{
				Y = { Pixels = 28 },
				Width = { Pixels = 136 },
				Height = { Pixels = -44, Percent = 100 },
				X = { Percent = 50 }
			};
			Add(gridSlots);

			for (int i = 0; i < 9; i++)
			{
				UIConsumerSlot slot = new UIConsumerSlot();
				gridSlots.Add(slot);
			}
		}
	}

	public class UIConsumerSlot : BaseElement
	{
		public Item PreviewItem;

		public UIConsumerSlot()
		{
			Width.Pixels = Height.Pixels = 40;
		}

		protected override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);

			float scale = Math.Min(InnerDimensions.Width / Main.inventoryBackTexture.Width, InnerDimensions.Height / Main.inventoryBackTexture.Height);

			if (PreviewItem != null && !PreviewItem.IsAir) DrawItem(spriteBatch, PreviewItem, scale);
		}

		private void DrawItem(SpriteBatch spriteBatch, Item item, float scale)
		{
			Texture2D itemTexture = Main.itemTexture[item.type];
			Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
			Color newColor = Color.White;
			float pulseScale = 1f;
			ItemSlot.GetItemLight(ref newColor, ref pulseScale, item);
			int height = rect.Height;
			int width = rect.Width;
			float drawScale = 1f;

			float availableWidth = InnerDimensions.Width;
			if (width > availableWidth || height > availableWidth)
			{
				if (width > height) drawScale = availableWidth / width;
				else drawScale = availableWidth / height;
			}

			drawScale *= scale;
			Vector2 position = Dimensions.Position() + Dimensions.Size() * 0.5f;
			Vector2 origin = rect.Size() * 0.5f;

			if (ItemLoader.PreDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale))
			{
				spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
				if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
			}

			ItemLoader.PostDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);
			if (ItemID.Sets.TrapSigned[item.type]) spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
			if (item.stack > 1)
			{
				string text = item.stack.ToString();
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, text, InnerDimensions.Position() + new Vector2(8, InnerDimensions.Height - Main.fontMouseText.MeasureString(text).Y * scale), Color.White, 0f, Vector2.Zero, new Vector2(0.85f), -1f, scale);
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
				Main.HoverItem = item.Clone();
				Main.hoverItemName = Main.HoverItem.Name;

				//if (ItemSlot.ShiftInUse) Hooking.SetCursor("Terraria/UI/Cursor_7");
			}
		}
	}
}