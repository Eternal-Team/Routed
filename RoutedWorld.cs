using BaseLibrary;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed
{
	public class RoutedWorld : ModWorld
	{
		public override void Load(TagCompound tag)
		{
			Routed.RoutedLayer.Load(tag.GetList<TagCompound>("RoutedNetwork").ToList());
		}

		public override void NetReceive(BinaryReader reader)
		{
			Routed.RoutedLayer.NetReceive(reader);
		}

		public override void NetSend(BinaryWriter writer)
		{
			Routed.RoutedLayer.NetSend(writer);
		}

		public override void PostDrawTiles()
		{
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Utility.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
			Routed.RoutedLayer.Draw(Main.spriteBatch);
			Main.spriteBatch.End();
		}

		public override void PostUpdate()
		{
			Routed.RoutedLayer.Update();
		}

		public override void PreUpdate()
		{
			Routed.RoutedLayer.Update();
		}

		public override TagCompound Save() => new TagCompound
		{
			["RoutedNetwork"] = Routed.RoutedLayer.Save()
		};
	}
}