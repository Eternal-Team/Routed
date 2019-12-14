using BaseLibrary;
using LayerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public class Duct : ModLayerElement<Duct>
	{
		public override string Texture => "";

		public override int DropItem => ModContent.ItemType<BasicDuct>();

		#region Static
		private static Vector2 Origin = new Vector2(14);

		private static Texture2D textureNormal;
		private static Texture2D textureDiagonal;
		private static Texture2D textureAll;
		private static Texture2D textureIntersection;
		private static Texture2D textureStraightV;
		private static Texture2D textureStraightH;

		private const string TextureLocation = "Routed/Textures/Duct/";

		static Duct()
		{
			textureNormal = ModContent.GetTexture(TextureLocation + "Normal");
			textureDiagonal = ModContent.GetTexture(TextureLocation + "Diagonal");
			textureAll = ModContent.GetTexture(TextureLocation + "All");
			textureIntersection = ModContent.GetTexture(TextureLocation + "Intersection");
			textureStraightV = ModContent.GetTexture(TextureLocation + "StraightV");
			textureStraightH = ModContent.GetTexture(TextureLocation + "StraightH");
		}
		#endregion

		private ushort frame;
		public RoutedNetwork Network;
		public BaseModule Module;

		public override void OnPlace()
		{
			Merge();
		}

		public override void OnRemove()
		{
			Network.RemoveTile(this);
		}

		public void Merge()
		{
			foreach (Duct duct in GetNeighbors()) duct.Network.Merge(Network);
		}

		public IEnumerable<Duct> GetVisualNeighbors()
		{
			if (Layer.ContainsKey(Position.X + 1, Position.Y)) yield return Layer[Position.X + 1, Position.Y];
			if (Layer.ContainsKey(Position.X - 1, Position.Y)) yield return Layer[Position.X - 1, Position.Y];
			if (Layer.ContainsKey(Position.X, Position.Y + 1)) yield return Layer[Position.X, Position.Y + 1];
			if (Layer.ContainsKey(Position.X, Position.Y - 1)) yield return Layer[Position.X, Position.Y - 1];
			if (Layer.ContainsKey(Position.X + 1, Position.Y + 1)) yield return Layer[Position.X + 1, Position.Y + 1];
			if (Layer.ContainsKey(Position.X - 1, Position.Y - 1)) yield return Layer[Position.X - 1, Position.Y - 1];
			if (Layer.ContainsKey(Position.X - 1, Position.Y + 1)) yield return Layer[Position.X - 1, Position.Y + 1];
			if (Layer.ContainsKey(Position.X + 1, Position.Y - 1)) yield return Layer[Position.X + 1, Position.Y - 1];
		}

		public override void UpdateFrame()
		{
			frame = 0;

			if (Layer.ContainsKey(Position.X - 1, Position.Y)) frame |= 1;
			if (Layer.ContainsKey(Position.X - 1, Position.Y - 1)) frame |= 2;
			if (Layer.ContainsKey(Position.X, Position.Y - 1)) frame |= 4;
			if (Layer.ContainsKey(Position.X + 1, Position.Y - 1)) frame |= 8;
			if (Layer.ContainsKey(Position.X + 1, Position.Y)) frame |= 16;
			if (Layer.ContainsKey(Position.X + 1, Position.Y + 1)) frame |= 32;
			if (Layer.ContainsKey(Position.X, Position.Y + 1)) frame |= 64;
			if (Layer.ContainsKey(Position.X - 1, Position.Y + 1)) frame |= 128;
			if (Layer.ContainsKey(Position.X - 1, Position.Y)) frame |= 256;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 position = Position.ToScreenCoordinates(false) + new Vector2(8);
			Color color = Lighting.GetColor(Position.X, Position.Y);

			if (frame == 0)
			{
				for (int i = 0; i < 4; i++) spriteBatch.Draw(textureNormal, position, null, color, MathHelper.PiOver2 * i, Origin, 1f, SpriteEffects.None, 0f);
			}
			else if (frame == 511)
			{
				Vector2 origin = Origin + new Vector2(1);
				for (int i = 0; i < 4; i++) spriteBatch.Draw(textureAll, position, null, color, MathHelper.PiOver2 * i, origin, 1f, SpriteEffects.None, 0f);
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					float angle = MathHelper.PiOver2 * i;

					ushort pow = (ushort)Math.Pow(4, i);
					bool bit1 = (frame & (1 * pow)) == 0;
					bool bit2 = (frame & (2 * pow)) == 0;
					bool bit3 = (frame & (4 * pow)) == 0;

					if (bit1 && bit2 && bit3) spriteBatch.Draw(textureNormal, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (bit1 && !bit2 && bit3) spriteBatch.Draw(textureDiagonal, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && bit2 && !bit3) spriteBatch.Draw(textureIntersection, position, null, color, angle, Origin + new Vector2(2), 1f, SpriteEffects.None, 0f);
					else if (bit1 && bit2) spriteBatch.Draw(textureStraightV, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && bit2) spriteBatch.Draw(textureStraightH, position, null, color, angle, Origin, 1f, SpriteEffects.None, 0f);
					else if (!bit1 && !bit3) spriteBatch.Draw(textureAll, position, null, color, angle, Origin + new Vector2(1), 1f, SpriteEffects.None, 0f);
				}
			}

			Module?.Draw(spriteBatch);
		}

		public override void Update()
		{
			Module?.Update();
		}

		public override bool Interact() => Module?.Interact() ?? false;

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			if (Module != null)
			{
				tag["Module"] = new TagCompound
				{
					["Type"] = Module.GetType().AssemblyQualifiedName,
					["Data"] = Module.Save()
				};
			}

			return tag;
		}

		public override void Load(TagCompound tag)
		{
			if (tag.ContainsKey("Module"))
			{
				TagCompound module = tag.GetCompound("Module");
				Module = (BaseModule)Activator.CreateInstance(Type.GetType(module.GetString("Type")));
				Module.Parent = this;
				Module.Load(module.GetCompound("Data"));
			}
		}
	}
}