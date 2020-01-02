using BaseLibrary;
using BaseLibrary.UI.New;
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
	public class ConsumerModulePanel : BaseLibrary.UI.BaseUIPanel<ConsumerModule>
	{
		public override void OnInitialize()
		{
			Width = Height = (0, 0.25f);
			this.Center();

			BaseLibrary.UI.Elements.UIText textLabel = new BaseLibrary.UI.Elements.UIText("Consumer Module")
			{
				Width = (0, 1),
				Height = (20, 0),
				HAlign = 0.5f,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			BaseLibrary.UI.Elements.UITextButton buttonClose = new BaseLibrary.UI.Elements.UITextButton("X")
			{
				Size = new Vector2(20),
				Left = (-20, 1),
				Padding = (0, 0, 0, 0),
				RenderPanel = false
			};
			buttonClose.OnClick += (evt, element) => BaseLibrary.UI.PanelUI.Instance.CloseUI(Container);
			Append(buttonClose);

			BaseLibrary.UI.Elements.UIGrid<UIConsumerSlot> gridSlots = new BaseLibrary.UI.Elements.UIGrid<UIConsumerSlot>(3)
			{
				Top = (28, 0),
				Width = (136, 0),
				Height = (-44, 1),
				HAlign = 0.5f
			};
			Append(gridSlots);

			for (int i = 0; i < 9; i++)
			{
				UIConsumerSlot slot = new UIConsumerSlot();
				gridSlots.Add(slot);
			}
		}
	}

	public class UIConsumerSlot : BaseLibrary.UI.Elements.BaseElement
	{
		public Item PreviewItem;

		public UIConsumerSlot()
		{
			Width = Height = (40, 0);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
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
			Vector2 origin = Utils.Size(rect) * 0.5f;

			if (ItemLoader.PreDrawInInventory(item, spriteBatch, position - Utils.Size(rect) * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale))
			{
				spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
				if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
			}

			ItemLoader.PostDrawInInventory(item, spriteBatch, position - Utils.Size(rect) * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);
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