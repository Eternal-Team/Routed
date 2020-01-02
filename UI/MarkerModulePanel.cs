﻿using BaseLibrary;
using BaseLibrary.UI.New;
using Microsoft.Xna.Framework;
using Routed.Modules;
using Routed.Modules.FilterModes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Routed.UI
{
	public class MarkerModulePanel : BaseLibrary.UI.BaseUIPanel<MarkerModule>
	{
		public override void OnInitialize()
		{
			Width = (408, 0);
			Height = (172, 0);
			this.Center();

			BaseLibrary.UI.Elements.UITextButton buttonClose = new BaseLibrary.UI.Elements.UITextButton("X")
			{
				Size = new Vector2(20),
				Left = (-20, 1),
				Padding = (0, 0, 0, 0),
				RenderPanel = false
			};
			buttonClose.OnClick += (evt, element) => BaseLibrary.UI.PanelUI.Instance.CloseUI(Container);
			Append(buttonClose);

			BaseLibrary.UI.Elements.UIText textLabel = new BaseLibrary.UI.Elements.UIText("Marker Module")
			{
				Width = (0, 1),
				Height = (20, 0),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			switch (Container.Mode)
			{
				case FilteredItemsMode mode:
				{
					BaseLibrary.UI.Elements.UIGrid<UIConsumerSlot> grid = new BaseLibrary.UI.Elements.UIGrid<UIConsumerSlot>(9)
					{
						Width = (0, 1),
						Height = (-28, 1),
						Top = (28, 0)
					};
					Append(grid);

					for (int i = 0; i < mode.whitelist.Count; i++)
					{
						UIConsumerSlot slot = new UIConsumerSlot { PreviewItem = new Item() };
						var i1 = i;
						slot.OnClick += (evt, element) =>
						{
							mode.whitelist[i1] = Main.mouseItem.type;
							slot.PreviewItem.SetDefaults(Main.mouseItem.type);
						};
						if (mode.whitelist[i] > 0) slot.PreviewItem.SetDefaults(mode.whitelist[i]);
						grid.Add(slot);
					}

					break;
				}
				case ModBasedMode mode:
				{
					BaseLibrary.UI.Elements.UIGrid<UIModItem> grid = new BaseLibrary.UI.Elements.UIGrid<UIModItem>
					{
						Width = (0, 1),
						Height = (-28, 1),
						Top = (28, 0)
					};
					Append(grid);

					foreach (Mod mod in ModLoader.Mods)
					{
						if (mod.GetValue<Dictionary<string, ModItem>>("items").Count <= 0) continue;

						UIModItem item = new UIModItem(mod)
						{
							Width = (0, 1),
							Height = (24, 0),
							TextColor = mode.mod == mod ? Color.LimeGreen : Color.White
						};
						item.OnClick += (evt, element) =>
						{
							mode.mod = mod;

							grid.Items.ForEach(modItem => modItem.TextColor = Color.White);
							item.TextColor = Color.LimeGreen;
						};
						grid.Add(item);
					}

					break;
				}
			}
		}

		private class UIModItem : BaseLibrary.UI.Elements.BaseElement
		{
			public Color TextColor
			{
				set => text.TextColor = value;
			}

			private Mod mod;
			private BaseLibrary.UI.Elements.UIText text;

			public UIModItem(Mod mod)
			{
				this.mod = mod;
				SetPadding(0);

				text = new BaseLibrary.UI.Elements.UIText(mod.DisplayName)
				{
					Width = (0, 1),
					Height = (0, 1),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center
				};
				Append(text);
			}
		}
	}
}