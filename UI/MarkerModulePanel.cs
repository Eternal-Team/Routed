using BaseLibrary;
using BaseLibrary.Input;
using BaseLibrary.Input.Mouse;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Modules;
using Routed.Modules.FilterModes;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Routed.UI
{
	public class MarkerModulePanel : BaseUIPanel<MarkerModule>
	{
		public MarkerModulePanel(MarkerModule module) : base(module)
		{
			Width.Pixels = 16 + (SlotSize + SlotMargin) * 9 - SlotMargin;
			Height.Pixels = 44 + SlotSize;

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Percent = 100 },
				Padding = Padding.Zero,
				RenderPanel = false
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UIText textLabel = new UIText("Marker Module")
			{
				Width = { Percent = 100 },
				Height = { Pixels = 20 },
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Add(textLabel);

			switch (Container.Mode)
			{
				case FilteredItemsMode mode:
				{
					UIGrid<UIMarkerSlot> grid = new UIGrid<UIMarkerSlot>(9)
					{
						Width = { Percent = 100 },
						Height = { Pixels = -28, Percent = 100 },
						Y = { Pixels = 28 }
					};
					Add(grid);

					for (int i = 0; i < mode.whitelist.Count; i++)
					{
						UIMarkerSlot slot = new UIMarkerSlot(module, i);
						grid.Add(slot);
					}

					break;
				}
				case ModBasedMode mode:
				{
					UIGrid<UIModItem> grid = new UIGrid<UIModItem>
					{
						Width = { Percent = 100 },
						Height = { Pixels = -28, Percent = 100 },
						Y = { Pixels = 28 }
					};
					Add(grid);

					foreach (Mod mod in ModLoader.Mods)
					{
						if (mod.GetValue<Dictionary<string, ModItem>>("items").Count <= 0) continue;

						UIModItem item = new UIModItem(mod)
						{
							Width = { Percent = 100 },
							Height = { Pixels = 24 },
							TextColor = mode.mod == mod ? Color.LimeGreen : Color.White
						};
						item.OnClick += args =>
						{
							mode.mod = mod;

							grid.Children.ForEach(modItem => ((UIModItem)modItem).TextColor = Color.White);
							item.TextColor = Color.LimeGreen;
						};
						grid.Add(item);
					}

					break;
				}
			}
		}

		private class UIModItem : BaseElement
		{
			public Color TextColor
			{
				set => text.TextColor = value;
			}

			private Mod mod;
			private UIText text;

			public UIModItem(Mod mod)
			{
				this.mod = mod;
				Padding = Padding.Zero;

				text = new UIText(mod.DisplayName)
				{
					Width = { Percent = 100 },
					Height = { Percent = 100 },
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center
				};
				Add(text);
			}
		}
	}

	public class UIMarkerSlot : BaseElement
	{
		private Item PreviewItem;
		private MarkerModule module;
		private int index;

		public UIMarkerSlot(MarkerModule module, int index)
		{
			this.module = module;
			this.index = index;

			PreviewItem = new Item();
			if (((FilteredItemsMode)module.Mode).whitelist[index] > 0) PreviewItem.SetDefaults(((FilteredItemsMode)module.Mode).whitelist[index]);

			Width.Pixels = Height.Pixels = SlotSize;
		}

		protected override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);

			float scale = Math.Min(InnerDimensions.Width / (float)Main.inventoryBackTexture.Width, InnerDimensions.Height / (float)Main.inventoryBackTexture.Height);

			if (PreviewItem != null && !PreviewItem.IsAir) DrawItem(spriteBatch, PreviewItem, scale);
		}

		protected override void MouseClick(MouseButtonEventArgs args)
		{
			if (args.Button == MouseButton.Left)
			{
				if (!Main.mouseItem.IsAir)
				{
					PreviewItem.SetDefaults(Main.mouseItem.type);
					((FilteredItemsMode)module.Mode).whitelist[index] = Main.mouseItem.type;
				}

				args.Handled = true;
			}
			else if (args.Button == MouseButton.Right)
			{
				PreviewItem.TurnToAir();
				((FilteredItemsMode)module.Mode).whitelist[index] = -1;

				args.Handled = true;
			}
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