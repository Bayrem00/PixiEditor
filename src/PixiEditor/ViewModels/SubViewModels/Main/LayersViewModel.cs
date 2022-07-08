﻿using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Layer", "Image")]
internal class LayersViewModel : SubViewModel<ViewModelMain>
{
    public RelayCommand CreateGroupFromActiveLayersCommand { get; set; }

    public RelayCommand DeleteGroupCommand { get; set; }

    public RelayCommand DeleteLayersCommand { get; set; }

    public RelayCommand DuplicateLayerCommand { get; set; }

    public RelayCommand RenameLayerCommand { get; set; }

    public RelayCommand RenameGroupCommand { get; set; }

    public RelayCommand MoveToBackCommand { get; set; }

    public RelayCommand MoveToFrontCommand { get; set; }

    public RelayCommand MergeSelectedCommand { get; set; }

    public RelayCommand MergeWithAboveCommand { get; set; }

    public RelayCommand MergeWithBelowCommand { get; set; }

    public LayersViewModel(ViewModelMain owner)
        : base(owner)
    {
        CreateGroupFromActiveLayersCommand = new RelayCommand(CreateGroupFromActiveLayers, CanCreateGroupFromSelected);
        DeleteLayersCommand = new RelayCommand(DeleteActiveLayers, CanDeleteActiveLayers);
        DuplicateLayerCommand = new RelayCommand(DuplicateLayer, CanDuplicateLayer);
        MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
        MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
        RenameLayerCommand = new RelayCommand(RenameLayer);
        MergeSelectedCommand = new RelayCommand(MergeSelected, CanMergeSelected);
        MergeWithAboveCommand = new RelayCommand(MergeWithAbove, CanMergeWithAbove);
        MergeWithBelowCommand = new RelayCommand(MergeWithBelow, CanMergeWithBelow);
        RenameGroupCommand = new RelayCommand(RenameGroup);
        DeleteGroupCommand = new RelayCommand(DeleteGroup, CanDeleteGroup);
    }

    public void CreateGroupFromActiveLayers(object parameter)
    {

    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDeleteSelected")]
    public bool CanDeleteSelected(object parameter)
    {
        /*
        bool paramIsLayerOrGroup = parameter is not null and (Layer or LayerGroup);
        bool activeLayerExists = Owner.BitmapManager?.ActiveDocument?.ActiveLayer != null;
        bool activeDocumentExists = Owner.BitmapManager.ActiveDocument != null;
        bool allGood = (paramIsLayerOrGroup || activeLayerExists) && activeDocumentExists;
        if (!allGood)
            return false;

        if (parameter is Layer or LayerStructureItemContainer)
        {
            return CanDeleteActiveLayers(null);
        }
        else if (parameter is LayerGroup group)
        {
            return CanDeleteGroup(group.GuidValue);
        }
        else if (parameter is LayerGroupControl groupControl)
        {
            return CanDeleteGroup(groupControl.GroupGuid);
        }
        else if (Owner.BitmapManager.ActiveDocument.ActiveLayer != null)
        {
            return CanDeleteActiveLayers(null);
        }*/
        return false;
    }

    [Command.Basic("PixiEditor.Layer.DeleteSelected", "Delete selected layer/folder", "", CanExecute = "PixiEditor.Layer.CanDeleteSelected")]
    public void DeleteSelected(object parameter)
    {
        /*
        if (parameter is Layer or LayerStructureItemContainer)
        {
            DeleteActiveLayers(null);
        }
        else if (parameter is LayerGroup group)
        {
            DeleteGroup(group.GuidValue);
        }
        else if (parameter is LayerGroupControl groupControl)
        {
            DeleteGroup(groupControl.GroupGuid);
        }
        else if (Owner.BitmapManager.ActiveDocument.ActiveLayer != null)
        {
            DeleteActiveLayers(null);
        }*/
    }

    public bool CanDeleteGroup(object parameter)
    {
        return false;
    }

    public void DeleteGroup(object parameter)
    {

    }

    public void RenameGroup(object parameter)
    {
    }

    [Command.Basic("PixiEditor.Layer.NewFolder", "New Folder", "Create new folder", CanExecute = "PixiEditor.Layer.CanCreateNewMember")]
    public void NewFolder(object parameter)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.CreateStructureMember(StructureMemberType.Folder);
    }

    public bool CanMergeSelected(object obj)
    {
        return false;
    }

    public bool CanCreateGroupFromSelected(object obj)
    {
        return false;
    }

    [Command.Basic("PixiEditor.Layer.NewLayer", "New Layer", "Create new layer", CanExecute = "PixiEditor.Layer.CanCreateNewMember", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
    public void NewLayer(object parameter)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.CreateStructureMember(StructureMemberType.Layer);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanCreateNewMember")]
    public bool CanCreateNewMember(object parameter)
    {
        return Owner.DocumentManagerSubViewModel.ActiveDocument is not null;
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragStarted")]
    public void OpacitySliderDragStarted(object paramenter)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.UseOpacitySlider();
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragStarted();
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragged")]
    public void OpacitySliderDragged(object paramenter)
    {
        if (paramenter is not double value)
            return;
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragged((float)value);
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragEnded")]
    public void OpacitySliderDragEnded(object paramenter)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragEnded();
    }

    public void SetActiveLayer(object parameter)
    {
        //int index = (int)parameter;

        //var doc = Owner.BitmapManager.ActiveDocument;

        /*if (doc.Layers[index].IsActive && Mouse.RightButton == MouseButtonState.Pressed)
        {
            return;
        }*/

        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            //doc.ToggleLayer(index);
        }
        //else if (Keyboard.IsKeyDown(Key.LeftShift) && Owner.BitmapManager.ActiveDocument.Layers.Any(x => x.IsActive))
        {
            //doc.SelectLayersRange(index);
        }
        //else
        {
            //doc.SetMainActiveLayer(index);
        }
    }

    public void DeleteActiveLayers(object unusedParameter)
    {

    }

    public bool CanDeleteActiveLayers(object unusedParam)
    {
        return false;
    }

    public void DuplicateLayer(object parameter)
    {

    }

    public bool CanDuplicateLayer(object property)
    {
        return false;
    }

    public void RenameLayer(object parameter)
    {

    }

    public bool CanRenameLayer(object parameter)
    {
        return false;
    }

    public void MoveLayerToFront(object parameter)
    {

    }

    public void MoveLayerToBack(object parameter)
    {

    }

    public bool CanMoveToFront(object property)
    {
        return false;
    }

    public bool CanMoveToBack(object property)
    {
        return false;
    }

    public void MergeSelected(object parameter)
    {

    }

    public void MergeWithAbove(object parameter)
    {

    }

    public void MergeWithBelow(object parameter)
    {

    }

    public bool CanMergeWithAbove(object property)
    {
        return false;
    }

    public bool CanMergeWithBelow(object property)
    {
        return false;
    }
}
