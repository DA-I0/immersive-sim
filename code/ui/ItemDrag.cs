using Godot;
using ImmersiveSim.Gameplay;
using ImmersiveSim.Interfaces;

namespace ImmersiveSim.UI
{
	public partial class ItemDrag : Control
	{
		private Control _source;
		private Control _destination;

		private BaseItem _item;
		private Control _preview;

		public BaseItem Item
		{
			get { return _item; }
		}

		public ItemDrag(Control source, BaseItem item)
		{
			_source = source;
			_item = item;
		}

		public override void _ExitTree()
		{
			((IDraggableItem)_source).UpdateWindowInfo();
		}
	}
}