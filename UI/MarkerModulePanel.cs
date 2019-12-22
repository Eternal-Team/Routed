using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Routed.Modules;
using System;
using System.Linq;
using Terraria.ModLoader;

namespace Routed.UI
{
	public class MarkerModulePanel : BaseUIPanel<MarkerModule>
	{
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

			UIText textLabel = new UIText("Marker Module")
			{
				Width = (0, 1),
				Height = (20, 0),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Append(textLabel);

			var enumerable = ModContent.GetInstance<Routed>().Code.GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(FilterMode)));

			UIGrid<UIText> grid = new UIGrid<UIText>
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};
			Append(grid);

			foreach (Type type in enumerable)
			{
				UIText text = new UIText(type.Name)
				{
					Width = (0, 1),
					Height = (20, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center
				};
				text.OnClick += (evt, element) =>
				{
					Container.mode = (FilterMode)Activator.CreateInstance(type);
					Container.mode.Module = Container;

					grid.Items.ForEach(uiText => uiText.TextColor = Color.White);
					text.TextColor = Color.Green;
				};
				grid.Add(text);
			}

			// mode switch - dropdown menu of some sorts would be the best
			// custom elements based on mode -> item wl/bl, mod selection, ...
		}
	}
}