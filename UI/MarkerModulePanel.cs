using BaseLibrary;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Routed.Modules;
using Routed.Modules.FilterModes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Routed.UI
{
	public class MarkerModulePanel : BaseUIPanel<MarkerModule>
	{
		public MarkerModulePanel(MarkerModule module) : base(module)
		{
			Width.Pixels = 408;
			Height.Pixels = 172;


			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Pixels = -20, Percent = 100 },
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
					//UIGrid<UIConsumerSlot> grid = new UIGrid<UIConsumerSlot>(9)
					//{
					//	Width = { Percent = 100 },
					//	Height = { Pixels = -28, Percent = 100 },
					//	Y = { Pixels = 28 }
					//};
					//Add(grid);

					//for (int i = 0; i < mode.whitelist.Count; i++)
					//{
					//	UIConsumerSlot slot = new UIConsumerSlot { PreviewItem = new Item() };
					//	var i1 = i;
					//	slot.OnClick += args =>
					//	{
					//		mode.whitelist[i1] = Main.mouseItem.type;
					//		slot.PreviewItem.SetDefaults(Main.mouseItem.type);
					//	};
					//	if (mode.whitelist[i] > 0) slot.PreviewItem.SetDefaults(mode.whitelist[i]);
					//	grid.Add(slot);
					//}

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

							grid.Children.ForEach(modItem => (modItem as UIModItem).TextColor = Color.White);
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
}