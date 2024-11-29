using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace SourceGit.Views
{
    public partial class CommitDetail : UserControl
    {
        private Point? _dragStartPosition;
        private Control _dragStartControl;
        
        public CommitDetail()
        {
            InitializeComponent();
            
            AddHandler(PointerPressedEvent, OnChangesPointerPressed, RoutingStrategies.Bubble, true);
            AddHandler(PointerReleasedEvent, OnChangesPointerReleased, RoutingStrategies.Bubble, true);
            AddHandler(PointerMovedEvent, OnChangesPointerMoved, RoutingStrategies.Bubble);
        }
        
        private void OnChangeDoubleTapped(object sender, TappedEventArgs e)
        {
            if (DataContext is ViewModels.CommitDetail detail && sender is Grid grid && grid.DataContext is Models.Change change)
            {
                detail.ActivePageIndex = 1;
                detail.SelectedChanges = new() { change };
            }

            e.Handled = true;
        }

        private void OnChangeContextRequested(object sender, ContextRequestedEventArgs e)
        {
            if (DataContext is ViewModels.CommitDetail detail && sender is Grid grid && grid.DataContext is Models.Change change)
            {
                var menu = detail.CreateChangeContextMenu(change);
                menu?.Open(grid);
            }

            e.Handled = true;
        }

        private void OnChangesPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.Source is not Control { DataContext: Models.Change } control)
            {
                return;
            }
            
            var pointerPoint = e.GetCurrentPoint(control);
            if (!pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            _dragStartControl = control;
            _dragStartPosition = pointerPoint.Position;
        }
        
        private void OnChangesPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                ClearDragStart();
            }
        }
        
        private async void OnChangesPointerMoved(object sender, PointerEventArgs e)
        {
            if (_dragStartPosition == null || _dragStartControl == null)
            {
                return;
            }
            
            var currentPoint = e.GetPosition(this);
            
            if (Point.Distance(currentPoint, _dragStartPosition.Value) > 3)
            {
                var filePath = "/home/joerg/Source/Dotnet/sourcegit/" +
                               ((Models.Change)_dragStartControl.DataContext).Path;

                var storageFile = await App.GetStorageProvider().TryGetFileFromPathAsync(new Uri(filePath));
                
                var data = new DataObject();
                data.Set(DataFormats.Files, (IEnumerable<IStorageItem>) new IStorageItem[] { storageFile });
                
                ClearDragStart();
                
                await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
            }
        }

        private void ClearDragStart()
        {
            _dragStartPosition = null;
            _dragStartControl = null;
        }
    }
}
