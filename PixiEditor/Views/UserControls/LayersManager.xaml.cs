﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        private object cachedItem;

        public ObservableCollection<object> LayerTreeRoot
        {
            get { return (ObservableCollection<object>)GetValue(LayerTreeRootProperty); }
            set { SetValue(LayerTreeRootProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerTreeRoot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerTreeRootProperty =
            DependencyProperty.Register(
                "LayerTreeRoot",
                typeof(ObservableCollection<object>),
                typeof(LayersManager),
                new PropertyMetadata(default(ObservableCollection<object>), ItemsChanged));
        public LayersViewModel LayerCommandsViewModel
        {
            get { return (LayersViewModel)GetValue(LayerCommandsViewModelProperty); }
            set { SetValue(LayerCommandsViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerCommandsViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerCommandsViewModelProperty =
            DependencyProperty.Register("LayerCommandsViewModel", typeof(LayersViewModel), typeof(LayersManager), new PropertyMetadata(default(LayersViewModel)));

        public bool OpacityInputEnabled
        {
            get { return (bool)GetValue(OpacityInputEnabledProperty); }
            set { SetValue(OpacityInputEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityInputEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityInputEnabledProperty =
            DependencyProperty.Register("OpacityInputEnabled", typeof(bool), typeof(LayersManager), new PropertyMetadata(false));

        public LayersManager()
        {
            InitializeComponent();
        }

        private void LayerStructureItemContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerStructureItemContainer container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(container, container, DragDropEffects.Move);
            }
        }

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var items = (ObservableCollection<object>)e.NewValue;
            LayersManager manager = (LayersManager)d;
            var numberInput = manager.numberInput;
            object item = manager.treeView.SelectedItem;
            if (items != null && items.Count > 0 && (e.OldValue == null || ((ObservableCollection<object>)e.OldValue).Count == 0))
            {
                item = items[0];
                manager.cachedItem = item;
            }

            SetInputOpacity(item, numberInput);
        }

        private static void SetInputOpacity(object item, NumberInput numberInput)
        {
            if (item is Layer layer)
            {
                numberInput.Value = layer.Opacity * 100f;
            }
            else if (item is LayerGroup group)
            {
                numberInput.Value = group.StructureData.Opacity * 100f;
            }
        }


        private void HandleGroupOpacityChange(LayerGroup group, float value)
        {
            if (LayerCommandsViewModel.Owner?.BitmapManager?.ActiveDocument != null)
            {
                var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;

                var processArgs = new object[] { group.StructureData.GroupGuid, value };
                var reverseProcessArgs = new object[] { group.StructureData.GroupGuid, group.StructureData.Opacity };

                ChangeGroupOpacityProcess(processArgs);

                doc.UndoManager.AddUndoChange(
                new Change(
                    ChangeGroupOpacityProcess,
                    reverseProcessArgs,
                    ChangeGroupOpacityProcess,
                    processArgs,
                    $"Change {group.Name} opacity"), false);
            }
        }

        private void ChangeGroupOpacityProcess(object[] processArgs)
        {
            if (processArgs.Length > 0 && processArgs[0] is Guid groupGuid && processArgs[1] is float opacity)
            {
                var structure = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure;
                var group = structure.GetGroupByGuid(groupGuid);
                group.Opacity = opacity;
                var layers = structure.GetGroupLayers(group);
                layers.ForEach(x => x.Opacity = x.Opacity); // This might seems stupid, but it raises property changed, without setting any value. This is used to trigger converters that use group opacity
                numberInput.Value = opacity * 100;
            }
        }

        private void LayerGroup_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerGroupControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(container, container, DragDropEffects.Move);
            }
        }

        private void NumberInput_LostFocus(object sender, RoutedEventArgs e)
        {
            float val = numberInput.Value / 100f;

            object item = treeView.SelectedItem;

            if (item == null && cachedItem != null)
            {
                item = cachedItem;
            }

            if (item is Layer layer)
            {
                float oldOpacity = layer.Opacity;
                layer.OpacityUndoTriggerable = val;
                UndoManager undoManager = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.UndoManager;
                undoManager.AddUndoChange(
                    new Change(
                        UpdateNumberInputLayerOpacityProcess,
                        new object[] { oldOpacity },
                        UpdateNumberInputLayerOpacityProcess,
                        new object[] { val }));
                undoManager.SquashUndoChanges(2);
            }
            else if (item is LayerGroup group)
            {
                HandleGroupOpacityChange(group, val);
            }
        }

        private void UpdateNumberInputLayerOpacityProcess(object[] args)
        {
            if (args.Length > 0 && args[0] is float opacity)
            {
                numberInput.Value = opacity * 100;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetInputOpacity(treeView.SelectedItem, numberInput);
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (sender is TreeView treeView && treeView.SelectedItem is LayerGroup group)
                {
                    group.StructureData.IsRenaming = true;
                    e.Handled = true;
                }
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            dropBorder.BorderBrush = Brushes.Transparent;

            if (e.Data.GetDataPresent(LayerGroupControl.LayerContainerDataName))
            {
                HandleLayerDrop(e.Data);
            }

            if (e.Data.GetDataPresent(LayerGroupControl.LayerGroupControlDataName))
            {
                HandleGroupControlDrop(e.Data);
            }
        }

        private void HandleLayerDrop(IDataObject data)
        {
            var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
            if (doc.Layers.Count == 0) return;

            var layerContainer = (LayerStructureItemContainer)data.GetData(LayerGroupControl.LayerContainerDataName);
            var refLayer = doc.Layers[0].LayerGuid;
            doc.MoveLayerInStructure(layerContainer.Layer.LayerGuid, refLayer);
            doc.LayerStructure.AssignParent(layerContainer.Layer.LayerGuid, null);
        }

        private void HandleGroupControlDrop(IDataObject data)
        {
            var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
            var groupContainer = (LayerGroupControl)data.GetData(LayerGroupControl.LayerGroupControlDataName);
            doc.LayerStructure.MoveGroup(groupContainer.GroupGuid, 0);
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            ((Border)sender).BorderBrush = LayerItem.HighlightColor;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            ((Border)sender).BorderBrush = Brushes.Transparent;
        }
    }
}