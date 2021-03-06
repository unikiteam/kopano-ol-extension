﻿using Acacia.Native;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KComboBox : KAbstractComboBox
    {
        #region Drop-down list

        protected enum CommitSource
        {
            MouseClick,
            KeyTab,
            KeyEnter
        }

        /// <summary>
        /// Custom list for the drop-down. Performs a few functions:
        /// - Prevents grabbing the focus away from the edit when clicked
        /// - Adds hover highlighting
        /// - Only commits selection when clicked or externally (through enter in the edit).
        ///   This prevents updating the text and associated filters when scrolling through the combo.
        /// </summary>
        private class DropList : ListBox
        {
            private readonly KComboBox _owner;
            private int _committedIndex = -1;
            public int ItemWidth { get; set; }

            public DropList(KComboBox owner, bool ownerDraw)
            {
                this._owner = owner;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.Selectable, false);
                BorderStyle = BorderStyle.None;

                if (ownerDraw)
                {
                    DrawMode = DrawMode.OwnerDrawFixed;
                }
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                _owner.OnDrawItem(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                // Perform the select to highlight
                SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                ResetSelectedIndex();
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
            }

            private void ResetSelectedIndex()
            {
                SelectedIndex = _committedIndex >= Items.Count ? -1 : _committedIndex;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                // Select the item under the mouse and commit
                SelectedIndex = IndexFromPoint(PointToClient(Cursor.Position));
                CommitSelection(CommitSource.MouseClick);
            }

            protected override void DefWndProc(ref Message m)
            {
                switch ((WM)m.Msg)
                {
                    // Prevent mouse activity from grabbing the focus away from the edit
                    case WM.MOUSEACTIVATE:
                        m.Result = (IntPtr)MA.NOACTIVATE;
                        return;
                }
                base.DefWndProc(ref m);
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                // Preferred size is simply the size of the (maximum) number of items
                Size prefSize = base.GetPreferredSize(proposedSize);
                int w = Math.Max(prefSize.Width, ItemWidth);
                return new Size(w, ItemHeight * Math.Min(Items.Count, _owner.MaxDropDownItems));
            }

            public CommitSource _commitSource;
            public void CommitSelection(CommitSource source)
            {
                _commitSource = source;
                _committedIndex = SelectedIndex;
                base.OnSelectedIndexChanged(new EventArgs());
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                // Don't notify until committed
            }

            public void ItemsChanged(int selectIndex)
            {
                _committedIndex = SelectedIndex = selectIndex;
            }
        }

        private readonly DropList _list;

        #endregion

        #region Items properties

        [DefaultValue(true)]
        [Localizable(true)]
        [Category("Behavior")]
        public bool IntegralHeight { get { return _list.IntegralHeight; } set { _list.IntegralHeight = value; } }

        [DefaultValue(13)]
        [Localizable(true)]
        [Category("Behavior")]
        public int ItemHeight { get { return _list.ItemHeight; } set { _list.ItemHeight = value; } }

        [DefaultValue(8)]
        [Localizable(true)]
        [Category("Behavior")]
        public int MaxDropDownItems { get; set; }

        #endregion

        private DisplayItem _selectedItem;

        public KComboBox() : this(false)
        {
        }

        protected internal KComboBox(bool ownerDraw)
        { 
            MaxDropDownItems = 8;
            _list = new DropList(this, ownerDraw);
            _list.IntegralHeight = true;
            _list.TabStop = false;
            _list.SelectedIndexChanged += _list_SelectedIndexChanged;
            DropControl = _list;
        }

        private void _list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_list.SelectedIndex >= 0)
            {
                _selectedItem = (DisplayItem)_list.SelectedItem;
                Text = _selectedItem.ToString();
            }
            else
            {
                Text = "";
                _selectedItem = null;
            }
            OnSelectedItemChanged();
        }

        protected CommitSource GetCommitSource() { return _list._commitSource; }

        public DisplayItem SelectedItem
        {
            get { return _selectedItem; }
        }

        public void Select(object data)
        {
            _list.SelectedIndex = -1;
            Text = null;
            _selectedItem = null;
            if (data != null)
            {
                foreach (DisplayItem item in DisplayItems)
                {
                    if (item.Item.Equals(data))
                    {
                        _list.SelectedItem = item;
                        _selectedItem = item;
                        break;
                    }
                }
            }
        }

        public event EventHandler SelectedItemChanged;

        protected virtual void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Wrapper for list items to use custom string formatting
        /// </summary>
        public class DisplayItem
        {
            private readonly KComboBox _owner;
            public readonly object Item;

            public DisplayItem(KComboBox owner, object item)
            {
                this._owner = owner;
                this.Item = item;
            }

            public override string ToString()
            {
                return _owner.DataSource.GetItemText(Item);
            }

            public override bool Equals(object obj)
            {
                return obj is DisplayItem && ((DisplayItem)obj).Item == Item;
            }

            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }
        }

        private KDataSourceRaw _dataSource;
        public KDataSourceRaw DataSource
        {
            get { return _dataSource; }
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    _displayItemCache.Clear();
                    UpdateItems();
                }
            }
        }

        private readonly Dictionary<object, DisplayItem> _displayItemCache = new Dictionary<object, DisplayItem>();

        private void UpdateItems()
        {
            int oldCount = _list.Items.Count;
            _list.BeginUpdate();
            try
            {
                _list.Items.Clear();
                int selected = -1;
                foreach (object item in _dataSource.FilteredItems)
                {
                    DisplayItem displayItem;
                    if (!_displayItemCache.TryGetValue(item, out displayItem))
                    {
                        displayItem = new DisplayItem(this, item);
                        _displayItemCache.Add(item, displayItem);
                    }

                    if (displayItem == _selectedItem)
                        selected = _list.Items.Count;
                    _list.Items.Add(displayItem);
                }

                if (_list.Items.Count == 0)
                {
                    // Create a not-found item if requested
                    object item = _dataSource.NotFoundItem;
                    if (item != null)
                    {
                        _list.Items.Add(new DisplayItem(this, item));
                    }
                }

                // Select the current item only if new number of items is smaller. This means we don't keep selection
                // when the user is removing text, only when they are typing more.
                _list.ItemsChanged(_list.Items.Count < oldCount ? selected : -1);

                MeasureItems();
                UpdateDropDownLayout();
            }
            finally
            {
                _list.EndUpdate();
            }
        }

        protected IEnumerable<DisplayItem> DisplayItems
        {
            get
            {
                foreach (object item in _list.Items)
                    yield return (DisplayItem)item;
            }
        }

        protected DisplayItem GetDisplayItem(int index)
        {
            return (DisplayItem)_list.Items[index];
        }

        protected int DisplayItemCount
        {
            get { return _list.Items.Count; }
        }

        virtual protected void OnDrawItem(DrawItemEventArgs e) { }

        protected virtual void MeasureItems()
        {
            // Virtual placeholder
        }

        protected void SetItemSize(Size size)
        {
            ItemHeight = size.Height;
            _list.ItemWidth = size.Width;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // Update the filter
            if (DataSource != null)
            {
                DataSource.Filter = new KDataFilter(Text);
                UpdateItems();

                if (_settingText == 0)
                {
                    DroppedDown = true;
                }
            }
        }

        protected override int GetDropDownHeightMax()
        {
            return Util.Bound(_list.Items.Count, 1, MaxDropDownItems) * ItemHeight + _list.Margin.Vertical;
        }

        protected override int GetDropDownHeightMin()
        {
            return ItemHeight;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch(e.KeyCode)
            {
                // Forward cursor keys to the list
                case Keys.Down:
                case Keys.Up:
                case Keys.PageDown:
                case Keys.PageUp:
                case Keys.Home:
                case Keys.End:
                    User32.SendMessage(_list.Handle, (int)WM.KEYDOWN, new IntPtr((int)e.KeyCode), IntPtr.Zero);
                    e.IsInputKey = true;
                    break;

                // Enter commits the selected index and closes the drop down
                case Keys.Enter:
                case Keys.Tab:
                    if (DroppedDown)
                    {
                        if (_list.SelectedIndex >= 0)
                            _list.CommitSelection(e.KeyCode == Keys.Enter ? CommitSource.KeyEnter : CommitSource.KeyTab);
                        DroppedDown = false;
                    }
                    e.IsInputKey = e.KeyCode == Keys.Enter;
                    break;
                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            switch ((WM)m.Msg)
            {
                // Forward mouse wheel messages to the list
                case WM.MOUSEWHEEL:
                    m.Result = (IntPtr) User32.SendMessage(_list.Handle, m.Msg, m.WParam, m.LParam);
                    return;
            }
            base.DefWndProc(ref m);
        }
    }
}
