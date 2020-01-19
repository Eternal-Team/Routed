using BaseLibrary.Input.Mouse;
using BaseLibrary.UI;
using Microsoft.Xna.Framework;
using Routed.Modules;
using Terraria;

namespace Routed.UI
{
	public class ProviderModulePanel : BaseUIPanel<ProviderModule>
	{
		public ProviderModulePanel(ProviderModule module) : base(module)
		{
			Width.Pixels = 200;
			Height.Pixels = 64;

			UIText textLabel = new UIText("Provider Module")
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
				X = { Percent = 100 },
				Padding = Padding.Zero,
				RenderPanel = false
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UIText textMode = new UIText(module.mode.ToString())
			{
				Width = { Percent = 100 },
				Height = { Pixels = 20 },
				X = { Percent = 50 },
				Y = { Pixels = 28 },
				HorizontalAlignment = HorizontalAlignment.Center
			};
			textMode.OnMouseDown += args =>
			{
				if (args.Button != MouseButton.Left) return;

				args.Handled = true;

				module.mode = module.mode.NextEnum();
				textMode.Text = module.mode.ToString();
			};
			Add(textMode);
		}
	}
}