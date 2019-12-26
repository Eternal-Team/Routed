using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Layer;
using Routed.Modules;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Routed.UI
{
	public class RequesterModulePanel : BaseUIPanel<RequesterModule>
	{
		private UIGrid<UISlot> gridSlots;

		public override void OnInitialize()
		{
			Width = (408, 0);
			Height = (308, 0);
			this.Center();

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				Left = (-20, 1),
				Padding = (0, 0, 0, 0),
				RenderPanel = false
			};
			buttonClose.OnClick += (evt, element) => BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(Container);
			Append(buttonClose);

			UIText textLabel = new UIText("Requester Module")
			{
				Width = (0, 1),
				Height = (20, 0),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			gridSlots = new UIGrid<UISlot>(9)
			{
				Top = (28, 0),
				Width = (0, 1),
				Height = (128, 0)
			};
			Append(gridSlots);

			UIText textRequestedItem = new UIText("Requested Items")
			{
				Width = (0, 1),
				Top = (180, 0),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textRequestedItem);

			UIGrid<UIContainerSlot> gridOutout = new UIGrid<UIContainerSlot>(9)
			{
				Top = (164 + 44, 0),
				Width = (0, 1),
				Height = (128, 0)
			};
			Append(gridOutout);

			for (int i = 0; i < Container.Handler.Slots; i++)
			{
				UIContainerSlot slot = new UIContainerSlot(() => Container.Handler, i);
				gridOutout.Add(slot);
			}

			PopulateGrid();
		}

		public void PopulateGrid()
		{
			gridSlots.Clear();

			foreach (MarkerModule module in Container.Parent.Network.MarkerModules)
			{
				ItemHandler handler = module.GetHandler();
				if (handler == null) continue;

				for (int i = 0; i < handler.Slots; i++)
				{
					Item item = handler.Items[i];
					if (item.IsAir) continue;

					UISlot slot = new UISlot(i) { PreviewItem = item };
					int i1 = i;
					slot.OnClick += (evt, element) =>
					{
						NetworkItem networkItem = new NetworkItem(handler.ExtractItem(i1, 10), module.Parent, Container.Parent);
						Container.Parent.Network.NetworkItems.Add(networkItem);

						PopulateGrid();
					};
					gridSlots.Add(slot);
				}
			}
		}

		private class UISlot : BaseElement
		{
			private int index;
			public Item PreviewItem;

			public UISlot(int index)
			{
				this.index = index;
				Width = Height = (40, 0);
			}

			public override int CompareTo(object obj)
			{
				return index.CompareTo((obj as UISlot)?.index);
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

					spriteBatch.Draw(Utility.ImmediateState, () => { ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, text, InnerDimensions.Position() + new Vector2(8, InnerDimensions.Height - Main.fontMouseText.MeasureString(text).Y * scale), Color.White, 0f, Vector2.Zero, new Vector2(0.85f), -1f, scale); });
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

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);

				float scale = Math.Min(InnerDimensions.Width / Main.inventoryBackTexture.Width, InnerDimensions.Height / Main.inventoryBackTexture.Height);

				if (PreviewItem != null && !PreviewItem.IsAir) DrawItem(spriteBatch, PreviewItem, scale);
			}
		}
	}
}