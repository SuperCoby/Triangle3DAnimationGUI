using Avalonia.Controls;
using GBX.NET;
using GBX.NET.LZO;
using Triangle3DAnimationGUI.ViewModels;
using Avalonia;

namespace Triangle3DAnimationGUI.Views;

public partial class MainWindow : Window
{
    private bool _wasPlayingBeforeEdit = false;
    private double? _savedVerticalOffset = null;

    public MainWindow()
    {
        InitializeComponent();
        Gbx.LZO = new Lzo();
        DataContext = new MainWindowViewModel();
        TranslationDataGrid.PreparingCellForEdit += DataGrid_PreparingCellForEdit_SelectAll;
        ScalingDataGrid.PreparingCellForEdit += DataGrid_PreparingCellForEdit_SelectAll;
        RotationDataGrid.PreparingCellForEdit += DataGrid_PreparingCellForEdit_SelectAll;

        TranslationDataGrid.BeginningEdit += DataGrid_BeginningEdit_PauseAnimation;
        ScalingDataGrid.BeginningEdit += DataGrid_BeginningEdit_PauseAnimation;
        RotationDataGrid.BeginningEdit += DataGrid_BeginningEdit_PauseAnimation;
        OrbitDataGrid.BeginningEdit += DataGrid_BeginningEdit_PauseAnimation;

        TranslationDataGrid.CellEditEnded += DataGrid_CellEditEnded_ResumeAnimation;
        ScalingDataGrid.CellEditEnded += DataGrid_CellEditEnded_ResumeAnimation;
        RotationDataGrid.CellEditEnded += DataGrid_CellEditEnded_ResumeAnimation;
        OrbitDataGrid.CellEditEnded += DataGrid_CellEditEnded_ResumeAnimation;

        // Sauvegarde la position du scroll avant ouverture d'un menu
        this.AddHandler(MenuItem.ClickEvent, (sender, e) =>
        {
            if (MainScrollViewer != null)
                _savedVerticalOffset = MainScrollViewer.Offset.Y;
        }, handledEventsToo: true);

        // Restaure la position du scroll après fermeture d'un menu
        this.AddHandler(MenuItem.PointerReleasedEvent, (sender, e) =>
        {
            if (MainScrollViewer != null && _savedVerticalOffset.HasValue)
            {
                MainScrollViewer.Offset = new Avalonia.Vector(MainScrollViewer.Offset.X, _savedVerticalOffset.Value);
                _savedVerticalOffset = null;
            }
        }, handledEventsToo: true);
    }

    private void DataGrid_PreparingCellForEdit_SelectAll(object? sender, Avalonia.Controls.DataGridPreparingCellForEditEventArgs e)
    {
        if (e.EditingElement is TextBox tb)
        {
            tb.SelectAll();
        }
    }

    private void DataGrid_BeginningEdit_PauseAnimation(object? sender, DataGridBeginningEditEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _wasPlayingBeforeEdit = vm.IsPlaying;
            if (vm.IsPlaying)
                vm.PauseCommand.Execute(null);
        }
    }

    private void DataGrid_CellEditEnded_ResumeAnimation(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (_wasPlayingBeforeEdit && DataContext is MainWindowViewModel vm)
        {
            vm.PlayCommand.Execute(null);
        }
        _wasPlayingBeforeEdit = false;
    }

    private void OnAboutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var about = new AboutWindow();
        about.ShowDialog(this);
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO: Sauvegarder le projet courant (fichier ou paramètres)
    }

    private void OnSaveAsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO: Sauvegarder sous un nouveau nom
    }

    private void OnExitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void OnPresetClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var presetWindow = new PresetWindow();
        presetWindow.ShowDialog(this);
    }

    private void OnTopmostPlusClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Topmost = true;
    }

    private void OnTopmostMinusClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Topmost = false;
    }
}