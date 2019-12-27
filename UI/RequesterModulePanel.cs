using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Routed.UI
{
	public class RequesterModulePanel : BaseUIPanel<RequesterModule>, IItemHandlerUI
	{
		private const int Columns = 13;
		private const int Rows = 6;
		private const int SlotSize = 44;
		private new const int Padding = 4;

		public ItemHandler Handler => Container.ReturnHandler;
		public string GetTexture(Item item) => "Routed/Textures/Modules/RequesterModule";
		private UIGrid<UIRequesterSlot> gridSlots;
		private UIText textQueue;

		public override void OnActivate()
		{
			Hooking.Network = Container.Parent.Network;
		}

		public override void OnDeactivate()
		{
			Hooking.Network = null;
		}

		public override void OnInitialize()
		{
			Width = (60 + (SlotSize + Padding) * Columns - Padding, 0);
			Height = (96 + (SlotSize + Padding) * (Rows + 2) - Padding * 2, 0);
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

			textQueue = new UIText("Queue")
			{
				Width = (0, 0.5f),
				HorizontalAlignment = HorizontalAlignment.Left
			};
			Append(textQueue);

			UIText textLabel = new UIText("Requester Module")
			{
				Width = (0, 1),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			UIPanel panel = new UIPanel
			{
				Top = (28, 0),
				Width = (0, 1),
				Height = ((SlotSize + Padding) * Rows - Padding, 0),
				BorderColor = Color.Transparent,
				BackgroundColor = Utility.ColorPanel_Selected * 0.75f
			};
			Append(panel);

			gridSlots = new UIGrid<UIRequesterSlot>(Columns)
			{
				Width = (-26, 1),
				Height = (0, 1),
				ListPadding = Padding
			};
			panel.Append(gridSlots);

			gridSlots.scrollbar.HAlign = 1;
			gridSlots.scrollbar.Top = (0, 0);
			gridSlots.scrollbar.Height = (0, 1);
			panel.Append(gridSlots.scrollbar);

			// requested items
			{
				UIText textRequestedItem = new UIText("Requested Items")
				{
					Width = ((SlotSize + Padding) * 10 - Padding, 0),
					MarginLeft = 8,
					Top = (36 + (SlotSize + Padding) * Rows - Padding, 0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				Append(textRequestedItem);

				panel = new UIPanel
				{
					Top = (64 + (SlotSize + Padding) * Rows - Padding, 0),
					Width = (0, 1),
					Height = (16 + (SlotSize + Padding) * 2 - Padding, 0),
					BorderColor = Color.Transparent,
					BackgroundColor = Utility.ColorPanel_Selected * 0.75f
				};
				Append(panel);

				UIGrid<UIContainerSlot> gridOutout = new UIGrid<UIContainerSlot>(10)
				{
					Width = ((SlotSize + Padding) * 10, 0),
					Height = (0, 1),
					ListPadding = Padding
				};
				panel.Append(gridOutout);

				for (int i = 0; i < Container.Handler.Slots; i++)
				{
					UIContainerSlot slot = new UIContainerSlot(() => Container.Handler, i)
					{
						Width = (SlotSize, 0),
						Height = (SlotSize, 0)
					};
					slot.SetPadding(2);
					gridOutout.Add(slot);
				}
			}

			// return items
			{
				UIText textReturnItems = new UIText("Return")
				{
					Width = ((SlotSize + Padding) * 3 - Padding, 0),
					MarginRight = 8,
					HAlign = 1,
					Top = (36 + (SlotSize + Padding) * Rows - Padding, 0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				Append(textReturnItems);

				UIGrid<UIContainerSlot> gridInput = new UIGrid<UIContainerSlot>(3)
				{
					Width = ((SlotSize + Padding) * 3 - Padding, 0),
					Height = (0, 1),
					HAlign = 1,
					ListPadding = Padding
				};
				panel.Append(gridInput);

				for (int i = 0; i < Container.ReturnHandler.Slots; i++)
				{
					UIContainerSlot slot = new UIContainerSlot(() => Container.ReturnHandler, i)
					{
						Width = (SlotSize, 0),
						Height = (SlotSize, 0)
					};
					slot.SetPadding(2);
					gridInput.Add(slot);
				}
			}

			UIButton buttonTransfer = new UIButton(ModContent.GetTexture("BaseLibrary/Textures/UI/QuickStack"))
			{
				VAlign = 0.5f,
				Left = (6 + (SlotSize + Padding) * 10 - Padding, 0),
				Width = (20, 0),
				Height = (20, 0),
				HoverText = "Transfer items"
			};
			buttonTransfer.OnClick += (evt, element) =>
			{
				for (int i = 0; i < Container.Handler.Slots; i++)
				{
					ref Item item = ref Container.Handler.GetItemInSlotByRef(i);
					Container.ReturnHandler.InsertItem(ref item);
					if (!item.IsAir) break;
				}
				Recipe.FindRecipes();
			};
			panel.Append(buttonTransfer);

			UpdateGrid();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			textQueue.Text = $"{Container.RequestQueue.Count} queued item{(Container.RequestQueue.Count != 1 ? "s" : "")}";
		}

		public void UpdateGrid()
		{
			// todo: cache
			Dictionary<int, Item> items = Container.Parent.Network.ProviderModules.SelectMany(module => module.GetHandler()?.Items).Where(item => !item.IsAir).GroupBy(item => item.type).Select(group =>
			{
				Item item = new Item();
				item.SetDefaults(group.Key);
				item.stack = group.Sum(i => i.stack);
				return item;
			}).ToDictionary(x => x.type, x => x);

			// remove
			for (int i = 0; i < gridSlots.Count; i++)
			{
				UIRequesterSlot slot = gridSlots.Items[i];
				if (!items.ContainsKey(slot.Item.type)) gridSlots.Remove(slot);
			}

			// update
			List<Item> remaining = new List<Item>();
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items.ElementAt(i).Value;
				UIRequesterSlot slot = gridSlots.Items.FirstOrDefault(s => s.Item.netID == item.netID);
				if (slot != null) slot.Item.stack = item.stack;
				else remaining.Add(item);
			}

			// add
			foreach (Item item in remaining)
			{
				UIRequesterSlot slot = new UIRequesterSlot
				{
					Width = (SlotSize, 0),
					Height = (SlotSize, 0),
					Item = item
				};
				slot.SetPadding(2);
				slot.OnClick += (evt, element) => Container.RequestItem(item.type, 10);
				gridSlots.Add(slot);
			}
		}

		private class UIRequesterSlot : BaseElement
		{
			public Item Item;

			public UIRequesterSlot()
			{
				Width = Height = (40, 0);
			}

			public override int CompareTo(object obj) => Item.type.CompareTo((obj as UIRequesterSlot)?.Item.type);

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
					string text = item.stack < 1000 ? item.stack.ToString() : item.stack.ToSI("N1");
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, text, InnerDimensions.Position() + new Vector2(8, InnerDimensions.Height - Main.fontMouseText.MeasureString(text).Y * scale), Color.White, 0f, Vector2.Zero, new Vector2(0.85f), -1f, scale);
				}

				if (IsMouseHovering)
				{
					Main.LocalPlayer.showItemIcon = false;
					Main.ItemIconCacheUpdate(0);
					Main.HoverItem = item.Clone();
					Main.hoverItemName = Main.HoverItem.Name;

					if (ItemSlot.ShiftInUse) BaseLibrary.Hooking.SetCursor("Routed/Textures/Modules/RequesterModule");
				}
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);

				float scale = Math.Min(InnerDimensions.Width / Main.inventoryBackTexture.Width, InnerDimensions.Height / Main.inventoryBackTexture.Height);

				if (Item != null && !Item.IsAir) DrawItem(spriteBatch, Item, scale);
			}
		}
	}
}