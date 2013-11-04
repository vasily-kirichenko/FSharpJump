using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using FSharpJumpCore;
using System.Threading;

namespace FSharpEditorEnhancements
{
    /// <summary>
    /// Interaction logic for FSharpJumpList.xaml
    /// </summary>
    public partial class FSharpJumpList : UserControl
    {
        PresentationSource _menuSite;
        private IWpfTextView _view;
        private IEnumerable<JumpItem> _items;
        String _filter = "";

        public FSharpJumpList(IWpfTextView view)
        {
            _view = view;
            this.MaxHeight = 400;
            this.MinHeight = 30;
            this.Width = 300;
            InitializeComponent();
            this.lb1.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        #region Event Handling

        private void lb1_Loaded(object sender, RoutedEventArgs e)
        {
            this._items = FSharpJumpCore.Analyzer.Analyse(this._view).ToList();
            this.lb1.ItemsSource = _items;
            this.TakeFocus();
            var caretPosition = this._view.Caret.ContainingTextViewLine.Extent.Start.Position;
            var selection = this._items
                .OrderBy((ji) => Math.Abs(ji.Line.Extent.Start.Position - caretPosition))
                .FirstOrDefault();
            this.lb1.SelectedItem = selection;
            this.lb1.Items.MoveCurrentTo(selection);
            this.lb1.ScrollIntoView(selection);
            this._view.Caret.PositionChanged += this.Caret_PositionChanged;
            this.Dispatcher.BeginInvoke((Action) (() =>{ Thread.Sleep(100); AdjustSize(); }));
        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            this._view.Caret.PositionChanged -= this.Caret_PositionChanged;
            this.RemoveAdornment();
        }

        private void lb1_Unloaded(object sender, RoutedEventArgs e)
        {
            this.ReleaseFocus();
            this._view.VisualElement.Focus();
        }

        void FSharpJumpList_TextInput(object sender, TextCompositionEventArgs e)
        {
            AddToFilter(e.Text);
        }

        private void FSharpJumpList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                e.Handled = true;
                if (e.Key == Key.Enter)
                {
                    JumpToItem();
                }
                RemoveAdornment();
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;
                RemoveFromFilter();
            }
        }

        private void lb1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            JumpToItem();
            RemoveAdornment();
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            var lbi = this.lb1.ItemContainerGenerator.ContainerFromItem(this.lb1.SelectedItem) as ListBoxItem;
            if (lbi != null)
            {
                lbi.Focus();
            }
            else
            {
                this.lb1.Focus();
            }
        }

        #endregion

        #region Navigation

        private void JumpToItem()
        {
            var ji = this.lb1.SelectedItem as FSharpJumpCore.JumpItem;
            if (ji != null)
            {
                var line = ji.Line;
                var lineNumber = ji.Line.LineNumber;
                var point = new SnapshotPoint(line.Snapshot, line.Start);

                this._view.Caret.MoveTo(point);
                this._view.Caret.EnsureVisible();
            }
        }

        void RemoveAdornment()
        {
            this._view.GetAdornmentLayer(FSharpJumpCore.FSharpJumpConstants.adornmentLayerName).RemoveAdornment(this);
        }

        #endregion

        #region Initialization

        public void TakeFocus()
        { 
            _menuSite = HwndSource.FromVisual(this.lb1);
            InputManager.Current.PushMenuMode(_menuSite);
        }

        private void ReleaseFocus()
        {
            Debug.WriteLine("In release focus");
            if (_menuSite != null)
            {
                InputManager.Current.PopMenuMode(_menuSite);
                _menuSite = null;
            }
        }

        private void AdjustSize()
        {
            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);
            if (this._view.ViewportRight < left + this.ActualWidth)
            {
                left = this._view.ViewportRight - this.ActualWidth;
            }
            if (this._view.ViewportBottom < top + this.ActualHeight)
            {
                top = this._view.ViewportBottom - this.ActualHeight;
            }
            if (this._view.ViewportTop > top)
            {
                top = this._view.ViewportTop;
            }
            if (this._view.ViewportLeft > left)
            {
                left = this._view.ViewportLeft;
            }
            this.Height = Math.Min(this._view.ViewportHeight, this.ActualHeight);
            this.Width = Math.Min(this._view.ViewportWidth, this.ActualWidth);
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        #endregion

        #region Filtering

        private void AddToFilter(string p)
        {
            _filter = _filter + p;
            UpdateList();
        }

        private void RemoveFromFilter()
        {
            if (_filter != "")
            {
                _filter = _filter.Substring(0, _filter.Length -1);
                UpdateList();
            }
        }

        private void UpdateList()
        {
            var selection = this.lb1.SelectedItem;
            var q = (from i in _items
                     where i.Name.IndexOf(_filter,StringComparison.CurrentCultureIgnoreCase) >= 0
                     select i).ToList();
            this.lb1.ItemsSource = q;
            this.lb1.SelectedItem = selection;
            this.lb1.ScrollIntoView(selection);
        }

        #endregion

        private void ExpandCollapse_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isExpanded)
            {
                _isExpanded = false;
                Canvas.SetLeft(this, _sLeft);
                Canvas.SetTop(this, _sTop);
                this.Height = _sHeight;
                this.Width = _sWidth;
            }
            else
            {
                _isExpanded = true;
                _sLeft = Canvas.GetLeft(this);
                _sTop = Canvas.GetTop(this);
                _sHeight = this.Height;
                _sWidth = this.Width;
                Canvas.SetLeft(this, this._view.ViewportLeft);
                Canvas.SetTop(this, this._view.ViewportTop);
                this.Height = this._view.ViewportBottom;
                this.Width = this._view.ViewportRight;
            }
        }

        bool _isExpanded = false;
        double _sLeft, _sTop, _sHeight, _sWidth = 0;
    }
}
