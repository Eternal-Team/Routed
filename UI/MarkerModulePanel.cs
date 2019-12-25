using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Routed.Modules;
using Terraria;

namespace Routed.UI
{
	public class MarkerModulePanel : BaseUIPanel<MarkerModule>
	{
		public override void OnInitialize()
		{
			// todo: conditional width and height
			Width = (408, 0);
			Height = (172, 0);
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

			UIText textLabel = new UIText("Marker Module")
			{
				Width = (0, 1),
				Height = (20, 0),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			switch (Container.Mode)
			{
				case AnyItemsMode _:
					break;
				case InInventoryMode _:
					break;
				case FilteredItemsMode mode:
				{
					UIGrid<UIConsumerSlot> grid = new UIGrid<UIConsumerSlot>(9)
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
			}
		}
	}
}