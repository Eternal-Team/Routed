using BaseLibrary;
using ContainerLibrary;
using Microsoft.Xna.Framework.Graphics;
using Routed.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Routed.Layer
{
	public abstract class BaseModule
	{
		public abstract int DropItem { get; }

		public RoutedNetwork Network => Parent.Network;
		public Duct Parent;
		public Point16 Position => Parent.Position;

		public virtual void Draw(SpriteBatch spriteBatch)
		{
		}

		private int prevType;
		protected ItemHandler CachedHandler;

		internal void InternalUpdate()
		{
			CacheHandler();
			Update();
		}

		protected virtual void Update()
		{
		}

		private void CacheHandler()
		{
			Tile tile = Main.tile[Position.X, Position.Y];
			if (tile.type != prevType)
			{
				prevType = tile.type;

				if (Utility.TryGetTileEntity(Parent.Position, out ModTileEntity te) && te is IItemHandler handler)
				{
					if (CachedHandler != handler.Handler)
					{
						if (CachedHandler != null) CachedHandler.OnContentsChanged -= HandlerContentsChanged;

						CachedHandler = handler.Handler;
						handler.Handler.OnContentsChanged += HandlerContentsChanged;
					}
				}
			}
		}

		private void HandlerContentsChanged(int slot, bool user)
		{
			if (user)
			{
				Network.RegenerateCache();
				Network.UpdateUIs();
			}
		}

		public virtual ItemHandler GetHandler() => CachedHandler;

		public virtual void OnPlace(BaseModuleItem item)
		{
		}

		public virtual void OnRemove()
		{
		}

		public virtual bool Interact() => false;

		public virtual bool IsItemValid(Item item) => true;

		public virtual TagCompound Save() => new TagCompound();

		public virtual void Load(TagCompound tag)
		{
			CacheHandler();
		}
	}
}