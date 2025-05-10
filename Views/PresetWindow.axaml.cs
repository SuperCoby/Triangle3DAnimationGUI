using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using Triangle3DAnimationGUI.Models;

namespace Triangle3DAnimationGUI.Views;

public partial class PresetWindow : Window
{
    public PresetWindow()
    {
        InitializeComponent();
    }

    private List<ObjFileNode> GetObjFileTree(string dir)
    {
        var nodes = new List<ObjFileNode>();
        // Fichiers .obj à ce niveau
        foreach (var file in Directory.GetFiles(dir, "*.obj"))
        {
            nodes.Add(new ObjFileNode { Name = Path.GetFileName(file), FullPath = file });
        }
        // Sous-dossiers
        foreach (var subDir in Directory.GetDirectories(dir))
        {
            var children = GetObjFileTree(subDir);
            if (children.Count > 0)
            {
                nodes.Add(new ObjFileNode { Name = Path.GetFileName(subDir), Children = children });
            }
        }
        return nodes;
    }

    private void LoadObjFiles()
    {
        // 1. Chercher Preset à côté de l'exécutable (publish)
        var exeDir = AppContext.BaseDirectory;
        var presetDir = Path.Combine(exeDir, "Preset");
        if (!Directory.Exists(presetDir) || Directory.GetFiles(presetDir, "*.obj").Length == 0)
        {
            // 2. Sinon, chercher à la racine du projet (debug)
            var dir = exeDir;
            for (int i = 0; i < 4; i++)
                dir = Directory.GetParent(dir)!.FullName;
            var debugPresetDir = Path.Combine(dir, "Preset");
            if (Directory.Exists(debugPresetDir))
                presetDir = debugPresetDir;
        }
        // Affichage temporaire du chemin utilisé pour debug
        Title = $"Presets ({presetDir})";
        if (!Directory.Exists(presetDir))
            Directory.CreateDirectory(presetDir);
        var tree = GetObjFileTree(presetDir);
        ObjTreeView.ItemsSource = tree;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        LoadObjFiles(); // Recharge la liste à chaque ouverture
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnImportClick(object? sender, RoutedEventArgs e)
    {
        if (ObjTreeView.SelectedItem is not ObjFileNode node || !node.IsFile)
            return;
        var fullPath = node.FullPath!;
        if (this.Owner is MainWindow mainWindow && mainWindow.DataContext is Triangle3DAnimationGUI.ViewModels.MainWindowViewModel vm)
        {
            vm.ObjPath = fullPath;
        }
        Close();
    }

    private void ObjListBox_DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && files.Any(f => f.Path != null && f.Path.LocalPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)))
            {
                e.DragEffects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }
    }

    private void ObjListBox_Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null)
            {
                var exeDir = AppContext.BaseDirectory;
                var presetDir = Path.Combine(exeDir, "Preset");
                if (!Directory.Exists(presetDir))
                {
                    // Recherche debug
                    var dir = exeDir;
                    for (int i = 0; i < 4; i++)
                        dir = Directory.GetParent(dir)!.FullName;
                    presetDir = Path.Combine(dir, "Preset");
                    if (!Directory.Exists(presetDir))
                        Directory.CreateDirectory(presetDir);
                }
                var objFiles = files.Where(f => f.Path != null && f.Path.LocalPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase));
                foreach (var file in objFiles)
                {
                    var sourcePath = file.Path.LocalPath;
                    var dest = Path.Combine(presetDir, Path.GetFileName(sourcePath));
                    // Si le fichier est déjà dans le dossier Preset, ne rien faire
                    if (string.Equals(Path.GetDirectoryName(sourcePath)?.TrimEnd(Path.DirectorySeparatorChar), presetDir.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                        continue;
                    try { File.Copy(sourcePath, dest, overwrite: true); } catch { /* Ignorer les erreurs de copie */ }
                }
                LoadObjFiles();
            }
        }
    }

    private void OnOpenPresetFolderClick(object? sender, RoutedEventArgs e)
    {
        var exeDir = AppContext.BaseDirectory;
        var presetDir = Path.Combine(exeDir, "Preset");
        if (!Directory.Exists(presetDir))
        {
            // Recherche debug
            var dir = exeDir;
            for (int i = 0; i < 4; i++)
                dir = Directory.GetParent(dir)!.FullName;
            presetDir = Path.Combine(dir, "Preset");
            if (!Directory.Exists(presetDir))
                Directory.CreateDirectory(presetDir);
        }
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = presetDir,
                UseShellExecute = true
            });
        }
        catch { /* Ignorer les erreurs */ }
    }
}
