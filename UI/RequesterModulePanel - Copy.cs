﻿//using BaseLibrary;
//using BaseLibrary.UI;
//using BaseLibrary.UI.Elements;
//using ContainerLibrary;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Routed.Modules;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria.UI;
//using Terraria.UI.Chat;

//namespace Routed.UI
//{
//	// todo: sorting modes
//	public class RequesterModulePanel : BaseUIPanel<RequesterModule>, IItemHandlerUI
//	{
//		private const int Columns = 13;
//		private const int Rows = 6;
//		private const int SlotSize = 44;
//		private new const int Padding = 4;
//		public ItemHandler Handler => Container.ReturnHandler;
//		public string GetTexture(Item item) => "Routed/Textures/Modules/RequesterModule";
//		private UIGrid<UIRequesterSlot> gridSlots;

//		private BaseLibrary.Ref<string> search = new BaseLibrary.Ref<string>("");

//		private UIText textQueue;

//		public override void OnActivate()
//		{
//			Hooking.Network = Container.Network;
//			Recipe.FindRecipes();
//		}

//		public override void OnDeactivate()
//		{
//			Hooking.Network = null;
//			Recipe.FindRecipes();
//		}

//		public override void OnInitialize()
//		{
//			Width.Pixels = 60 + (SlotSize + Padding) * Columns - Padding;
//			Height = (144 + (SlotSize + Padding) * (Rows + 2) - Padding * 2, 0);
//			

//			UITextButton buttonClose = new UITextButton("X")
//			{
//				Size = new Vector2(20),
//				X = { Pixels = -20, Percent = 100)
//				Padding = Padding.Zero,
//				RenderPanel = false
//			};
//			buttonClose.OnClick += (args) => PanelUI.Instance.CloseUI(Container);
//			Add(buttonClose);

//			textQueue = new UIText("Queue")
//			{
//				Width = { Pixels = 0.5f)
//				HorizontalAlignment = HorizontalAlignment.Left
//			};
//			Add(textQueue);

//			UIText textLabel = new UIText("Requester Module")
//			{
//				Width = {  Percent = 100)
//				HorizontalAlignment = HorizontalAlignment.Center
//			};
//			Add(textLabel);

//			UIPanel panel = new UIPanel
//			{
//				Y = { Pixels = 28)
//				Width = {  Percent = 100)
//				Height = { Pixels = (SlotSize + Padding) * Rows - Padding)
//				BorderColor = Color.Transparent,
//				BackgroundColor = Utility.ColorPanel_Selected * 0.75f
//			};
//			Add(panel);

//			gridSlots = new UIGrid<UIRequesterSlot>(Columns)
//			{
//				Width = { Pixels = -26, Percent = 100)
//				Height = {  Percent = 100)
//				ListPadding = Padding
//			};
//			gridSlots.SearchSelector += item =>
//			{
//				if (string.IsNullOrWhiteSpace(search.Value)) return true;

//				string itemName = item.Item.HoverName.ToLower();
//				string searchName = search.Value.ToLower();

//				return itemName.Contains(searchName);
//			};
//			panel.Add(gridSlots);

//			gridSlots.scrollbar.X.Percent = 100;
//			gridSlots.scrollbar.Y = { Pixels = 0 };
//			gridSlots.scrollbar.Height. Percent = 100;
//			panel.Add(gridSlots.scrollbar);

//			UITextInput inputSearch = new UITextInput(ref search)
//			{
//				Y = { Pixels = 36 + (SlotSize + Padding } * Rows - Padding)
//				Width = {  Percent = 100)
//				Height = { Pixels = 40)
//				RenderPanel = true,
//				VerticalAlignment = VerticalAlignment.Center,
//				HintText = "Search"
//			};
//			inputSearch.OnTextChange += () => gridSlots.Search();
//			Add(inputSearch);

//			// requested items
//			{
//				UIText textRequestedItem = new UIText("Requested Items")
//				{
//					Width = { Pixels = (SlotSize + Padding) * 10 - Padding)
//					MarginLeft = 8,
//					Y = { Pixels = 84 + (SlotSize + Padding) * Rows - Padding)
//					HorizontalAlignment = HorizontalAlignment.Center
//				};
//				Add(textRequestedItem);

//				panel = new UIPanel
//				{
//					Y = { Pixels = 112 + (SlotSize + Padding) * Rows - Padding)
//					Width = {  Percent = 100)
//					Height = { Pixels = 16 + (SlotSize + Padding) * 2 - Padding)
//					BorderColor = Color.Transparent,
//					BackgroundColor = Utility.ColorPanel_Selected * 0.75f
//				};
//				Add(panel);

//				UIGrid<UIContainerSlot> gridOutout = new UIGrid<UIContainerSlot>(10)
//				{
//					Width = { Pixels = (SlotSize + Padding) * 10)
//					Height = {  Percent = 100)
//					ListPadding = Padding
//				};
//				panel.Add(gridOutout);

//				for (int i = 0; i < Container.Handler.Slots; i++)
//				{
//					UIContainerSlot slot = new UIContainerSlot(() => Container.Handler, i)
//					{
//						Width = { Pixels = SlotSize)
//						Height = { Pixels = SlotSize)
//					};
//					slot.SetPadding(2);
//					gridOutout.Add(slot);
//				}
//			}

//			// return items
//			{
//				UIText textReturnItems = new UIText("Return")
//				{
//					Width = { Pixels = (SlotSize + Padding } * 3 - Padding)
//					MarginRight = 8,
//					X.Percent = 100,
//					Y = { Pixels = 84 + (SlotSize + Padding) * Rows - Padding)
//					HorizontalAlignment = HorizontalAlignment.Center
//				};
//				Add(textReturnItems);

//				UIGrid<UIContainerSlot> gridInput = new UIGrid<UIContainerSlot>(3)
//				{
//					Width = { Pixels = (SlotSize + Padding) * 3 - Padding)
//					Height = {  Percent = 100)
//					X.Percent = 100,
//					ListPadding = Padding
//				};
//				panel.Add(gridInput);

//				for (int i = 0; i < Container.ReturnHandler.Slots; i++)
//				{
//					UIContainerSlot slot = new UIContainerSlot(() => Container.ReturnHandler, i)
//					{
//						Width = { Pixels = SlotSize)
//						Height = { Pixels = SlotSize)
//					};
//					slot.SetPadding(2);
//					gridInput.Add(slot);
//				}
//			}

//			UIButton buttonTransfer = new UIButton(ModContent.GetTexture("BaseLibrary/Textures/UI/QuickStack") }
//			{
//				Y = {Percent=50},
//				X = { Pixels = 6 + (SlotSize + Padding) * 10 - Padding)
//				Width = { Pixels = 20)
//				Height = { Pixels = 20)
//				HoverText = "Transfer items"
//			};
//			buttonTransfer.OnClick += (args) =>
//			{
//				for (int i = 0; i < Container.Handler.Slots; i++)
//				{
//					ref Item item = ref Container.Handler.GetItemInSlotByRef(i);
//					Container.ReturnHandler.InsertItem(ref item);
//					if (!item.IsAir) break;
//				}

//				Recipe.FindRecipes();
//			};
//			panel.Add(buttonTransfer);

//			UpdateGrid();
//		}

//		protected override void Update(GameTime gameTime)
//		{
//			base.Update(gameTime);

//			textQueue.Text = $"{Container.RequestQueue.Count} queued item{(Container.RequestQueue.Count != 1 ? "s" : "")}";
//		}

//		public void UpdateGrid()
//		{
//			// remove
//			for (int i = 0; i < gridSlots.Count; i++)
//			{
//				UIRequesterSlot slot = gridSlots.Items[i];
//				if (!Container.Network.ItemCache.ContainsKey(slot.Item.type)) gridSlots.Remove(slot);
//			}

//			// update
//			var remaining = new Dictionary<int, int>();
//			foreach (var pair in Container.Network.ItemCache)
//			{
//				UIRequesterSlot slot = gridSlots.Items.FirstOrDefault(s => s.Item.netID == pair.Key);
//				if (slot != null) slot.Item.stack = pair.Value;
//				else remaining.Add(pair.Key, pair.Value);
//			}

//			// add
//			foreach (var pair in remaining)
//			{
//				Item item = new Item();
//				item.SetDefaults(pair.Key);
//				item.stack = pair.Value;

//				UIRequesterSlot slot = new UIRequesterSlot
//				{
//					Width = { Pixels = SlotSize)
//					Height = { Pixels = SlotSize)
//					Item = item
//				};
//				slot.SetPadding(2);
//				slot.OnClick += (args) => Container.RequestItem(item.type, 10);
//				gridSlots.Add(slot);
//			}
//		}

//		private class UIRequesterSlot : BaseElement
//		{
//			public Item Item;

//			public UIRequesterSlot()
//			{
//				Width = { Pixels = 40 };
//				Height.Pixels = 40;
//			}

//			public override int CompareTo(object obj) => Item.type.CompareTo((obj as UIRequesterSlot)?.Item.type);

//			protected override void Draw(SpriteBatch spriteBatch }
//			{
//				spriteBatch.DrawSlot(Dimensions, Color.White, Main.inventoryBackTexture);

//				float scale = Math.Min(InnerDimensions.Width / Main.inventoryBackTexture.Width, InnerDimensions.Height / Main.inventoryBackTexture.Height);

//				if (Item != null && !Item.IsAir) DrawItem(spriteBatch, Item, scale);
//			}

//			private void DrawItem(SpriteBatch spriteBatch, Item item, float scale)
//			{
//				Texture2D itemTexture = Main.itemTexture[item.type];
//				Rectangle rect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Frame();
//				Color newColor = Color.White;
//				float pulseScale = 1f;
//				ItemSlot.GetItemLight(ref newColor, ref pulseScale, item);
//				int height = rect.Height;
//				int width = rect.Width;
//				float drawScale = 1f;

//				float availableWidth = InnerDimensions.Width;
//				if (width > availableWidth || height > availableWidth)
//				{
//					if (width > height) drawScale = availableWidth / width;
//					else drawScale = availableWidth / height;
//				}

//				drawScale *= scale;
//				Vector2 position = Dimensions.Position() + Dimensions.Size() * 0.5f;
//				Vector2 origin = rect.Size() * 0.5f;

//				if (ItemLoader.PreDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale))
//				{
//					spriteBatch.Draw(itemTexture, position, rect, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
//					if (item.color != Color.Transparent) spriteBatch.Draw(itemTexture, position, rect, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
//				}

//				ItemLoader.PostDrawInInventory(item, spriteBatch, position - rect.Size() * 0.5f * drawScale, rect, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);
//				if (ItemID.Sets.TrapSigned[item.type]) spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
//				if (item.stack > 1)
//				{
//					string text = item.stack < 1000 ? item.stack.ToString() : item.stack.ToSI("N1");
//					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, text, InnerDimensions.Position() + new Vector2(8, InnerDimensions.Height - Main.fontMouseText.MeasureString(text).Y * scale), Color.White, 0f, Vector2.Zero, new Vector2(0.85f), -1f, scale);
//				}

//				if (IsMouseHovering)
//				{
//					Main.LocalPlayer.showItemIcon = false;
//					Main.ItemIconCacheUpdate(0);
//					Main.HoverItem = item.Clone();
//					Main.hoverItemName = Main.HoverItem.Name;

//					if (ItemSlot.ShiftInUse) BaseLibrary.Hooking.SetCursor("Routed/Textures/Modules/RequesterModule");
//				}
//			}
//		}
//	}
//}
