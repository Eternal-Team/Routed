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
		public override void PostDrawTiles()
		{
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
			ModContent.GetInstance<Routed>().RoutedLayer.Draw(Main.spriteBatch);
			Main.spriteBatch.End();
		}

		public override void NetSend(BinaryWriter writer)
		{
			ModContent.GetInstance<Routed>().RoutedLayer.NetSend(writer);
		}

		public override void NetReceive(BinaryReader reader)
		{
			ModContent.GetInstance<Routed>().RoutedLayer.NetReceive(reader);
		}

		public override TagCompound Save() => new TagCompound
		{
			["RoutedNetwork"] = ModContent.GetInstance<Routed>().RoutedLayer.Save()
		};

		public override void Load(TagCompound tag)
		{
			ModContent.GetInstance<Routed>().RoutedLayer.Load(tag.GetList<TagCompound>("RoutedNetwork").ToList());
		}

		public override void PostUpdate()
		{
			ModContent.GetInstance<Routed>().RoutedLayer.Update();
		}
	}
}