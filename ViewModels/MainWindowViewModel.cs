using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GBX.NET;
using GBX.NET.Engines.Game;
using System.Collections.ObjectModel;
using Triangle3DAnimation.Animation;
using Triangle3DAnimation.Animation.Transformations;
using Triangle3DAnimation.ObjLoader;
using Triangle3DAnimationGUI.Models;
using System.Text.Json;
using Triangle3DAnimationGUI.Json;
using System.Diagnostics;
using TmEssentials;
using System.ComponentModel;
using System.Timers;
using System.Collections.Generic;

namespace Triangle3DAnimationGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private readonly FilePickerFileType gbxType = new("GameBox") { Patterns = ["*.Gbx"] };
	private readonly FilePickerFileType objType = new("Obj") { Patterns = ["*.obj"] };
	private readonly FilePickerFileType jsonType = new("JSON") { Patterns = ["*.json"] };
	private readonly FilePickerFileType allTypes = new("All") { Patterns = ["*.*"] };

	private string? _currentProjectPath;

	private List<AnimationFrame>? _animationFramesCache;
	private bool _animationFramesDirty = true;

	public MainWindowViewModel()
	{
		TranslationRows.CollectionChanged += (s, e) => OnTranslationRowsChanged(e);
		ScalingRows.CollectionChanged += (s, e) => OnScalingRowsChanged(e);
		RotationRows.CollectionChanged += (s, e) => OnRotationRowsChanged(e);
		OrbitRows.CollectionChanged += (s, e) => OnOrbitRowsChanged(e);
		// Ajout initial
		var t = new TranslationRow(0, 3, 0, 0, 0);
		TranslationRows.Add(t);
		t.PropertyChanged += AnyRow_PropertyChanged;
		var s = new ScalingRow(0, 3, 1, 1, 1);
		ScalingRows.Add(s);
		s.PropertyChanged += AnyRow_PropertyChanged;
		var r = new RotationRow(0, 3, 0, 0, 0, 10);
		RotationRows.Add(r);
		r.PropertyChanged += AnyRow_PropertyChanged;
		var o = new OrbitRow(0, 3, 2, 60);
		OrbitRows.Add(o);
		o.PropertyChanged += AnyRow_PropertyChanged;
		UpdateAnimationDuration();

		// Sélection automatique de la première ligne de chaque animation
		SelectedTranslationRow = TranslationRows.FirstOrDefault();
		SelectedScalingRow = ScalingRows.FirstOrDefault();
		SelectedRotationRow = RotationRows.FirstOrDefault();
		SelectedOrbitRow = OrbitRows.FirstOrDefault();
	}
	public ObservableCollection<TranslationRow> TranslationRows { get; } = [];
	public ObservableCollection<ScalingRow> ScalingRows { get; } = [];
	public ObservableCollection<RotationRow> RotationRows { get; } = [];
	public ObservableCollection<OrbitRow> OrbitRows { get; } = [];
	public bool CanRemoveTranslationRow => SelectedTranslationRow != null;
	public bool CanRemoveScalingRow => SelectedScalingRow != null;
	public bool CanRemoveRotationRow => SelectedRotationRow != null;
	public bool CanRemoveOrbitRow => SelectedOrbitRow != null;

	partial void OnSelectedTranslationRowChanged(TranslationRow? value)
	{
		OnPropertyChanged(nameof(CanRemoveTranslationRow));
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsTranslationTimeInvalid));
	}

	partial void OnSelectedScalingRowChanged(ScalingRow? value)
	{
		OnPropertyChanged(nameof(CanRemoveScalingRow));
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsScalingTimeInvalid));
	}

	partial void OnSelectedRotationRowChanged(RotationRow? value)
	{
		OnPropertyChanged(nameof(CanRemoveRotationRow));
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(IsParamsValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsRotationTimeInvalid));
		OnPropertyChanged(nameof(IsRotationStepsInvalid));
	}

	partial void OnSelectedOrbitRowChanged(OrbitRow? value)
	{
		OnPropertyChanged(nameof(CanRemoveOrbitRow));
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(IsParamsValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsOrbitTimeInvalid));
		OnPropertyChanged(nameof(IsOrbitStepsInvalid));
	}

	partial void OnIsTranslationAnimationChanged(bool value)
	{
		InvalidateAnimationFramesCache();
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsTranslationTimeInvalid));
	}

	partial void OnIsScalingAnimationChanged(bool value)
	{
		InvalidateAnimationFramesCache();
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsScalingTimeInvalid));
	}

	partial void OnIsRotationAnimationChanged(bool value)
	{
		InvalidateAnimationFramesCache();
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsRotationTimeInvalid));
		OnPropertyChanged(nameof(IsRotationStepsInvalid));
	}

	partial void OnIsOrbitEnabledChanged(bool value)
	{
		if (value && OrbitRows.Count == 0)
		{
			var o = new OrbitRow(0, 3, 2, 60);
			OrbitRows.Add(o);
			o.PropertyChanged += AnyRow_PropertyChanged;
		}
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
		OnPropertyChanged(nameof(IsTimeValid));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
		OnPropertyChanged(nameof(CanExportClip));
		OnPropertyChanged(nameof(IsOrbitTimeInvalid));
		OnPropertyChanged(nameof(IsOrbitStepsInvalid));
	}

	[ObservableProperty] private string? _objPath;
	[ObservableProperty] private string? _gbxPath;
	[ObservableProperty] private float? _originX = 512;
	[ObservableProperty] private float? _originY = 0;
	[ObservableProperty] private float? _originZ = 512;
	[ObservableProperty] private string? _statusLabel = "Status: OK";
	[ObservableProperty] private bool _isTranslationAnimation;
	[ObservableProperty] private bool _isScalingAnimation;
	[ObservableProperty] private bool _isRotationAnimation;
	[ObservableProperty] private TranslationRow? _selectedTranslationRow;
	[ObservableProperty] private ScalingRow? _selectedScalingRow;
	[ObservableProperty] private RotationRow? _selectedRotationRow;
	[ObservableProperty] private OrbitRow? _selectedOrbitRow;
	[ObservableProperty] private bool _showWireframe = false;
	[ObservableProperty] private bool _showFaces = true;
	[ObservableProperty] private bool _showVertices = false;
	[ObservableProperty] private bool _showColor = false;
	[ObservableProperty] private bool _showRender = false;
	[ObservableProperty] private float shadingIntensity = 0.0f;
	[ObservableProperty] private int _selectedClipGroup = 2;
	[ObservableProperty] private int _selectedClip = 0;
	[ObservableProperty] private int _selectedTrack = 0;
	[ObservableProperty] private ObservableCollection<string> _dropdownClips = ["Add New Clip"];
	[ObservableProperty] private ObservableCollection<string> _dropdownTracks = ["Add New Track"];
	[ObservableProperty] private bool _showOrigin = false;
	[ObservableProperty] private bool _showMaterialsTable = false;
	[ObservableProperty] private bool _showGrid = true;
	[ObservableProperty] private string? _fileSizeInfo;
	[ObservableProperty] private string? _objFileSize;
	[ObservableProperty] private string? _gbxFileSize;
	[ObservableProperty] private string? _estimatedExportGbxSize;
	[ObservableProperty] private bool _isExportSizeTooLarge;
	[ObservableProperty] private bool _isPlaying = false;
	[ObservableProperty] private float _animationTime = 0f;
	[ObservableProperty] private bool _isLoading = false;
	[ObservableProperty] private bool _isOrbitEnabled = false;
	private System.Timers.Timer? _animationTimer;
	private float _animationDuration = 3f; // Durée par défaut, ajustée dynamiquement

	private bool _isRepeatEnabled = false;
	public bool IsRepeatEnabled
	{
		get => _isRepeatEnabled;
		set => SetProperty(ref _isRepeatEnabled, value);
	}

	public bool ShowClips => SelectedClipGroup is 2 or 3 && ShowOnlyTracks;
	public bool ShowOnlyTracks => CanExportChallenge;
	public bool EnableTracks => SelectedClip > 0 || SelectedClipGroup < 2 || GbxFile?.Node is CGameCtnMediaClip;
	[ObservableProperty] private ObjModel? _obj3DModel;
	[ObservableProperty] private Gbx? _gbxFile;
	public bool CanExport =>
		GbxFile != null &&
		Obj3DModel != null &&
		IsParamsValid;
	public bool CanExportChallenge =>
		CanExport &&
		GbxFile?.Node is CGameCtnChallenge;
	public bool CanExportClip =>
		CanExport &&
		(GbxFile?.Node is CGameCtnMediaClip || GbxFile?.Node is CGameCtnChallenge);

	private void UpdateVisiblity()
	{
		OnPropertyChanged(nameof(ShowClips));
		OnPropertyChanged(nameof(ShowOnlyTracks));
		OnPropertyChanged(nameof(EnableTracks));
		OnPropertyChanged(nameof(CanExport));
		OnPropertyChanged(nameof(CanExportChallenge));
	}

	partial void OnObj3DModelChanged(ObjModel? value)
	{
		InvalidateAnimationFramesCache(); // Force le recalcul du cache d'animation
		UpdateVisiblity();
		UpdateEstimatedExportGbxSize();
	}

	partial void OnGbxFileChanged(Gbx? value)
	{
		UpdateVisiblity();
		UpdateClipGroupSelection();
	}

	partial void OnSelectedClipGroupChanged(int value)
	{
		UpdateVisiblity();
		UpdateClipGroupSelection();
	}

	partial void OnSelectedClipChanged(int value)
	{
		UpdateVisiblity();
		if (GbxFile == null) return;
		if (GbxFile.Node is CGameCtnChallenge challenge)
		{
			CGameCtnMediaClipGroup? clipGroup;

			if (SelectedClipGroup == 2) clipGroup = challenge.ClipGroupInGame;
			else clipGroup = challenge.ClipGroupEndRace;

			if (SelectedClip > 0)
			{
				UpdateTrackSelection(clipGroup?.Clips[SelectedClip - 1].Clip);
			}
			else
			{
				UpdateTrackSelection(null);
			}
		}
		UpdateEstimatedExportGbxSize();
	}

	private void UpdateClipGroupSelection()
	{
		if (GbxFile == null) return;
		if (GbxFile.Node is CGameCtnChallenge challenge)
		{
			CGameCtnMediaClip? clip = null;
			if (SelectedClipGroup is 0 or 1)
			{
				if (SelectedClipGroup == 0) clip = challenge.ClipIntro;
				else
				{
					foreach (var chunk in challenge.Chunks) // Get global clip
					{
						if (chunk.Id != 0x03043026) continue;
						var clipGlobal = (CGameCtnChallenge.Chunk03043026)chunk;
						clip = (CGameCtnMediaClip)clipGlobal.U01!;
					}
				}
			}
			else
			{
				CGameCtnMediaClipGroup? clipGroup;

				if (SelectedClipGroup == 2) clipGroup = challenge.ClipGroupInGame;
				else clipGroup = challenge.ClipGroupEndRace;

				UpdateClipSelection(clipGroup);
			}

			UpdateTrackSelection(clip);
		}
		else if (GbxFile.Node is CGameCtnMediaClip clip)
		{
			UpdateTrackSelection(clip);
		}
	}

	private void UpdateClipSelection(CGameCtnMediaClipGroup? clipGroup)
	{
		DropdownClips.Clear();
		DropdownClips.Add("Add New Clip");
		SelectedClip = 0;

		if (clipGroup == null) return;

		var clipTriggers = clipGroup.Clips;
		for (int i = 0; i < clipTriggers.Count; i++) DropdownClips.Add($"{i} - {clipTriggers[i].Clip.Name}");
	}

	private void UpdateTrackSelection(CGameCtnMediaClip? clip)
	{
		DropdownTracks.Clear();
		DropdownTracks.Add("Add New Track");
		SelectedTrack = 0;

		if (clip == null) return;

		var tracks = clip.Tracks;
		for (int i = 0; i < tracks.Count; i++) DropdownTracks.Add($"{i} - {tracks[i].Name}");
	}

	private static CGameCtnMediaClipGroup CreateMediaClipGroup()
	{
		CGameCtnMediaClipGroup clipGroup = new();
		clipGroup.CreateChunk(0x0307A003);
		return clipGroup;
	}

	private static CGameCtnMediaClip CreateMediaClip()
	{
		CGameCtnMediaClip clip = new();
		clip.CreateChunk(0x03079004);
		clip.CreateChunk(0x03079005);
		clip.CreateChunk(0x03079007);
		clip.Name = "Triangle3DAnimation Clip";
		return clip;
	}

	private static CGameCtnMediaTrack CreateMediaTrack()
	{
		CGameCtnMediaTrack track = new();
		var chunk001 = (CGameCtnMediaTrack.Chunk03078001)track.CreateChunk(0x03078001)!;
		chunk001.U01 = 2;
		track.CreateChunk(0x03078004);
		track.Name = "Triangle3DAnimation Track";
		return track;
	}

	private CGameCtnMediaClip GetGlobalMediaClip(CGameCtnChallenge challenge)
	{
		foreach (var chunk in challenge.Chunks)
		{
			if (chunk.Id != 0x03043026) continue;
			var clipGlobal = (CGameCtnChallenge.Chunk03043026)chunk;
			var clip = (CGameCtnMediaClip?)clipGlobal.U01;
			if (clip == null)
			{
				clip = CreateMediaClip();
				clipGlobal.U01 = clip;
			}
			return clip;
		}

		var impossibleChunk = "Chunk 0x03043026 was not found, which should be impossible.";
		StatusLabel = impossibleChunk;
		throw new UnreachableException(impossibleChunk);
	}

	partial void OnObjPathChanged(string? value)
	{
		LoadObjAsync(value);
	}

	private async void LoadObjAsync(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
		{
			IsLoading = true;
			var dir = Path.GetDirectoryName(value)!;
			var name = Path.GetFileNameWithoutExtension(value)!;
			try
			{
				var model = await Task.Run(() => ObjLoader.ParseObj(dir, name));
				Obj3DModel = model;
				StatusLabel = $"Obj loaded";
				var fileInfo = new FileInfo(value);
				ObjFileSize = FormatFileSize(fileInfo.Length);
			}
			catch { Obj3DModel = null; StatusLabel = "Error loading .obj"; ObjFileSize = null; }
			finally { IsLoading = false; }
		}
		else
		{
			Obj3DModel = null;
			StatusLabel = "No .obj selected";
			ObjFileSize = null;
		}
		UpdateFileSizeInfo();
		UpdateEstimatedExportGbxSize();
	}

	partial void OnGbxPathChanged(string? value)
	{
		if (string.IsNullOrWhiteSpace(value) || !File.Exists(value))
		{
			GbxFile = null;
			StatusLabel = "No .gbx selected";
			GbxFileSize = null;
			UpdateFileSizeInfo();
			UpdateEstimatedExportGbxSize();
			return;
		}

		IsLoading = true;
		Task.Run(() =>
		{
			try
			{
				var gbx = Gbx.Parse(value);
				var fileInfo = new FileInfo(value);
				return (gbx, fileInfo);
			}
			catch
			{
				return ((Gbx?)null, (FileInfo?)null);
			}
		}).ContinueWith(t =>
		{
			var result = t.Result;
			var gbx = result.Item1;
			var fileInfo = result.Item2;
			if (gbx == null || fileInfo == null)
			{
				StatusLabel = $"Error: Failed to parse Gbx. ({value})";
				GbxFile = null;
				GbxFileSize = null;
				UpdateFileSizeInfo();
				UpdateEstimatedExportGbxSize();
				IsLoading = false;
				return;
			}
			GbxFile = gbx;
			GbxFileSize = FormatFileSize(fileInfo.Length);
			if (GbxFile.Node == null)
			{
				StatusLabel = $"Error: Node is null. ({value})";
				GbxFile = null;
				GbxFileSize = null;
				UpdateFileSizeInfo();
				IsLoading = false;
				return;
			}
			switch (GbxFile.Node)
			{
				case CGameCtnChallenge:
					StatusLabel = $"Challenge.Gbx Loaded.";
					break;
				case CGameCtnMediaClip:
					StatusLabel = $"Clip.Gbx Loaded.";
					break;
				default:
					StatusLabel = $"Error: Gbx is not Challenge or Clip. ({GbxFile.Node.GetType()})";
					GbxFile = null;
					GbxFileSize = null;
					UpdateFileSizeInfo();
					UpdateEstimatedExportGbxSize();
					IsLoading = false;
					return;
			}
			UpdateFileSizeInfo();
			UpdateEstimatedExportGbxSize();
			IsLoading = false;
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void UpdateFileSizeInfo()
	{
		string? info = null;
		if (!string.IsNullOrEmpty(ObjFileSize) && !string.IsNullOrEmpty(GbxFileSize))
			info = $"{ObjFileSize} / {GbxFileSize}";
		else if (!string.IsNullOrEmpty(ObjFileSize))
			info = ObjFileSize;
		else if (!string.IsNullOrEmpty(GbxFileSize))
			info = GbxFileSize;

		if (!string.IsNullOrEmpty(EstimatedExportGbxSize))
			info = info != null ? $"{info} → {EstimatedExportGbxSize}" : $"→ {EstimatedExportGbxSize}";
		FileSizeInfo = info;
	}

	private void UpdateEstimatedExportGbxSize()
	{
		// On ne peut estimer que si les deux fichiers sont chargés
		if (Obj3DModel == null || GbxFile == null || string.IsNullOrEmpty(GbxPath))
		{
			EstimatedExportGbxSize = null;
			IsExportSizeTooLarge = false;
			UpdateFileSizeInfo();
			return;
		}
		try
		{
			// Générer le Gbx exporté en mémoire
			var tempGbx = Gbx.Parse(GbxPath!); // Ajout du '!' pour supprimer l'avertissement CS8600
			Apply3DTrianglesTo(tempGbx); // Appliquer les modifs comme pour l'export
			using var ms = new MemoryStream();
			tempGbx.Save(ms);
			EstimatedExportGbxSize = FormatFileSize(ms.Length);
			IsExportSizeTooLarge = false;
			if (GbxPath.EndsWith("Challenge.Gbx", StringComparison.OrdinalIgnoreCase))
			{
				IsExportSizeTooLarge = ms.Length > 512 * 1024;
			}
		}
		catch
		{
			EstimatedExportGbxSize = null;
			IsExportSizeTooLarge = false;
		}
		UpdateFileSizeInfo();
	}

	private void Apply3DTrianglesTo(Gbx gbx)
	{
		var triangleBlock = PrepareTriangles3DBlock();
		if (gbx.Node is CGameCtnChallenge challenge)
		{
			CGameCtnMediaClip clip;
			if (SelectedClipGroup < 2)
			{
				if (SelectedClipGroup == 0) clip = challenge.ClipIntro ??= CreateMediaClip();
				else clip = GetGlobalMediaClip(challenge);
				// Supprimer tous les anciens blocs Triangles 3D
				foreach (var track in clip.Tracks)
					track.Blocks.RemoveAll(b => b is CGameCtnMediaBlockTriangles3D);
				SetTrack(clip.Tracks, triangleBlock);
			}
			else
			{
				CGameCtnMediaClipGroup clipGroup;
				if (SelectedClipGroup == 2) clipGroup = challenge.ClipGroupInGame ??= CreateMediaClipGroup();
				else clipGroup = challenge.ClipGroupEndRace ??= CreateMediaClipGroup();
				if (SelectedClip == 0)
				{
					clipGroup.Clips.Add(new() { Clip = CreateMediaClip() });
					clip = clipGroup.Clips[^1].Clip;
				}
				else clip = clipGroup.Clips[SelectedClip - 1].Clip;
				// Supprimer tous les anciens blocs Triangles 3D
				foreach (var track in clip.Tracks)
					track.Blocks.RemoveAll(b => b is CGameCtnMediaBlockTriangles3D);
				SetTrack(clip.Tracks, triangleBlock);
			}
		}
		else if (gbx.Node is CGameCtnMediaClip clip)
		{
			foreach (var track in clip.Tracks)
				track.Blocks.RemoveAll(b => b is CGameCtnMediaBlockTriangles3D);
			SetTrack(clip.Tracks, triangleBlock);
		}
	}

	private static string FormatFileSizeWithDisk(FileInfo fileInfo)
	{
		long fileSize = fileInfo.Length;
		// Taille sur disque : arrondie à 4096 octets (4 Ko) près
		long sizeOnDisk = ((fileSize + 4095) / 4096) * 4096;
		string real = FormatFileSize(fileSize);
		string disk = FormatFileSize(sizeOnDisk);
		return $"{real} / {disk}";
	}

	[RelayCommand]
	private async Task BrowseObj(Window window)
	{
		var topLevel = TopLevel.GetTopLevel(window);

		if (topLevel == null) return;
		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select Obj",
			AllowMultiple = false,
			FileTypeFilter = [objType, allTypes]
		});

		if (files.Count == 0) return;
		ObjPath = files[0].Path.LocalPath;
	}

	[RelayCommand]
	private async Task BrowseGbx(Window window)
	{
		var topLevel = TopLevel.GetTopLevel(window);

		if (topLevel == null) return;
		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select Gbx",
			AllowMultiple = false,
			FileTypeFilter = [gbxType, allTypes]
		});

		if (files.Count == 0) return;
		GbxPath = files[0].Path.LocalPath;
	}

	[RelayCommand]
	private async Task Open()
	{
		var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
		if (window == null)
			return;
		var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Open file",
			AllowMultiple = false,
			FileTypeFilter = [jsonType, objType, allTypes]
		});
		if (files.Count == 0) return;
		var filePath = files[0].Path.LocalPath;
		if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				var json = await File.ReadAllTextAsync(filePath);
				var settings = JsonSerializer.Deserialize(json, ProjectSettingsJsonContext.Default.ProjectSettings);
				if (settings != null)
				{
					OriginX = settings.OriginX;
					OriginY = settings.OriginY;
					OriginZ = settings.OriginZ;
					ObjPath = settings.ObjPath;
					GbxPath = settings.GbxPath;
					TranslationRows.Clear();
					foreach (var row in settings.TranslationRows) TranslationRows.Add(row);
					RotationRows.Clear();
					foreach (var row in settings.RotationRows) RotationRows.Add(row);
					ScalingRows.Clear();
					foreach (var row in settings.ScalingRows) ScalingRows.Add(row);
					OrbitRows.Clear();
					if (settings.OrbitRows != null)
						foreach (var row in settings.OrbitRows) OrbitRows.Add(row);
					ShadingIntensity = settings.ShadingIntensity;
					if (settings.Materials != null && Obj3DModel != null)
					{
						// Crée une nouvelle instance ObjModel pour forcer la notification
						Obj3DModel = new ObjModel(
							Obj3DModel.Vertices,
							Obj3DModel.Faces,
							settings.Materials,
							Obj3DModel.OriginalFaces
							);
						// Réassocie chaque face au bon matériau (par nom)
						var matDict = Obj3DModel.Materials.ToDictionary(m => m.Name);
						foreach (var face in Obj3DModel.Faces)
						{
							if (face.Material != null && matDict.TryGetValue(face.Material.Name, out var newMat))
								face.Material = newMat;
						}
					}
					IsTranslationAnimation = settings.IsTranslationAnimation;
					IsScalingAnimation = settings.IsScalingAnimation;
					IsRotationAnimation = settings.IsRotationAnimation;
					IsOrbitEnabled = settings.IsOrbitEnabled;
					// Correction : forcer la sélection pour déclencher la mise à jour UI
					SelectedTranslationRow = TranslationRows.Count > 0 ? TranslationRows[0] : null;
					SelectedScalingRow = ScalingRows.Count > 0 ? ScalingRows[0] : null;
					SelectedRotationRow = RotationRows.Count > 0 ? RotationRows[0] : null;
					SelectedOrbitRow = OrbitRows.Count > 0 ? OrbitRows[0] : null;
					StatusLabel = $"Project loaded from {Path.GetFileName(filePath)}";
					_currentProjectPath = filePath; // remember opened project path
					UpdateAnimationDuration();
					// Ajout : notifier la vue pour la validité du temps et l'état des boutons
					OnPropertyChanged(nameof(IsTimeValid));
					OnPropertyChanged(nameof(CanExport));
					OnPropertyChanged(nameof(CanExportChallenge));
					OnPropertyChanged(nameof(CanExportClip));
				}
				else
				{
					StatusLabel = "Error loading project (invalid JSON)";
				}
			}
			catch (Exception ex)
			{
				StatusLabel = $"Error opening project: {ex.Message}";
			}
		}
		else if (filePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
		{
			ObjPath = filePath;
		}
		else
		{
			StatusLabel = "Unsupported file type";
		}
	}

	private CGameCtnMediaBlockTriangles3D PrepareTriangles3DBlock()
	{
		Base baseModel = new BaseModel(Obj3DModel!, ShadingIntensity);

		SingleBlockTriangleAnimation animationWithSingleObj = new(baseModel);

		// Ajout des transformations Translation
		if (IsTranslationAnimation)
		{
			foreach (var row in TranslationRows)
			{
				animationWithSingleObj.AddTransformation(new Translation(
					new Vec3(row.X, row.Y, row.Z),
					TimeSingle.FromSeconds(row.StartTime),
					TimeSingle.FromSeconds(row.EndTime)
				));
			}
		}
		// Ajout des transformations Scaling
		if (IsScalingAnimation)
		{
			foreach (var row in ScalingRows)
			{
				animationWithSingleObj.AddTransformation(new Scaling(
					row.ScaleX, row.ScaleY, row.ScaleZ,
					TimeSingle.FromSeconds(row.StartTime),
					TimeSingle.FromSeconds(row.EndTime)
				));
			}
		}
		// Ajout des transformations Rotation
		if (IsRotationAnimation)
		{
			foreach (var row in RotationRows)
			{
				animationWithSingleObj.AddTransformation(new Rotation(
					row.X, row.Y, row.Z, row.Steps,
					TimeSingle.FromSeconds(row.StartTime),
					TimeSingle.FromSeconds(row.EndTime)
				));
			}
		}

		// Ajout des orbites
		if (IsOrbitEnabled)
		{
			foreach (var row in OrbitRows)
			{
				double duration = row.EndTime - row.StartTime;
				float delta = (float)duration / row.Steps;
				double angleMax = row.Degrees * Math.PI / 180.0;
				for (int i = 0; i < row.Steps; i++)
				{
					double t = duration * i / row.Steps;
					double angle = angleMax * t / duration;
					float offsetX = (float)(row.Radius * Math.Cos(angle));
					float offsetZ = (float)(row.Radius * Math.Sin(angle));
					animationWithSingleObj.AddTransformation(new Translation(
						new Vec3(offsetX, 0f, offsetZ),
						TimeSingle.FromSeconds((float)(row.StartTime + t)),
						TimeSingle.FromSeconds((float)(row.StartTime + t + delta))
					));
				}
			}
		}

		// Si aucune transformation, ajouter une identité par défaut
		if (!IsTranslationAnimation && !IsScalingAnimation && !IsRotationAnimation && (!IsOrbitEnabled || OrbitRows.Count == 0))
		{
			animationWithSingleObj.AddTransformation(new Identity(
				TimeSingle.FromSeconds(0f),
				TimeSingle.FromSeconds(3f)
			));
		}

		animationWithSingleObj.GenerateFrames();

		// Décalage automatique de l'origine Y : on ajoute toujours +8 pour placer sur l'herbe
		float originY = (float)(OriginY ?? 128) + 8f;
		Vec3 posOrigin = new
		(
			(float)(OriginX ?? 512),
			originY,
			(float)(OriginZ ?? 512)
		);

		var triangleBlock = animationWithSingleObj.ToTriangle3DMediaTrackerBlock(posOrigin);

		triangleBlock.CreateChunk(0x03029001); // CGameCtnMediaBlockTriangles 0x001
		var trianglesChunk = (CGameCtnMediaBlockTriangles.Chunk03029001)triangleBlock.Chunks.First();

		trianglesChunk.U01 = 1; // Vertex creation mode. 0 = Double click to place. 1 = Default.
		trianglesChunk.U04 = 1; // DrawPlaneAxis? Default = 1.
		return triangleBlock;
	}

	private void Apply3DTriangles()
	{
		var triangleBlock = PrepareTriangles3DBlock();
		if (GbxFile!.Node is CGameCtnChallenge challenge)
		{
			CGameCtnMediaClip clip;
			if (SelectedClipGroup < 2)
			{
				if (SelectedClipGroup == 0) clip = challenge.ClipIntro ??= CreateMediaClip();
				else clip = GetGlobalMediaClip(challenge);

				SetTrack(clip.Tracks, triangleBlock);
			}
			else
			{
				CGameCtnMediaClipGroup clipGroup;
				if (SelectedClipGroup == 2) clipGroup = challenge.ClipGroupInGame ??= CreateMediaClipGroup();
				else clipGroup = challenge.ClipGroupEndRace ??= CreateMediaClipGroup();

				if (SelectedClip == 0)
				{
					clipGroup.Clips.Add(new()
					{
						Clip = CreateMediaClip()
					});
					clip = clipGroup.Clips[^1].Clip;
				}
				else clip = clipGroup.Clips[SelectedClip - 1].Clip;

				SetTrack(clip.Tracks, triangleBlock);
			}
		}
		else if (GbxFile!.Node is CGameCtnMediaClip clip)
		{
			SetTrack(clip.Tracks, triangleBlock);
		}
	}

	private void SetTrack(List<CGameCtnMediaTrack> tracks, CGameCtnMediaBlockTriangles3D triangleBlock)
	{
		if (SelectedTrack == 0)
		{
			tracks.Add(CreateMediaTrack());
			var blocks = tracks[^1].Blocks;
			blocks.Add(triangleBlock);
		}
		else
		{
			var blocks = tracks[SelectedTrack - 1].Blocks;
			blocks.Clear();
			blocks.Add(triangleBlock);
		}
	}

	[RelayCommand]
	private void Export()
	{
		Apply3DTriangles();
		GbxFile!.Save(GbxPath!);
		UpdateVisiblity();
		UpdateClipGroupSelection();
		StatusLabel = "Export Successful";
	}

	[RelayCommand]
	private async Task ExportAsChallenge()
	{
		var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
		if (window == null)
			return;
		var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Export As Challenge...",
			SuggestedFileName = GbxFile!.GetFileNameWithoutExtension() + ".Challenge",
			FileTypeChoices = [gbxType, allTypes],
		});

		if (file is not null)
		{
			Apply3DTriangles();
			var localPath = file.Path.LocalPath;
			GbxFile!.Save(localPath);
			GbxFile = Gbx.Parse(GbxPath!);
			StatusLabel = "Export As Challenge Successful";
		}
	}

	[RelayCommand]
	private async Task ExportAsClip()
	{
		var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
		if (window == null)
			return;
		var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Export As Clip...",
			SuggestedFileName = GbxFile!.GetFileNameWithoutExtension() + ".Clip.Gbx",
			FileTypeChoices = [gbxType, allTypes],
		});

		if (file is not null)
		{
			Apply3DTriangles();
			var localPath = file.Path.LocalPath;
			if (GbxFile.Node is CGameCtnMediaClip) GbxFile.Save(localPath);
			else if (GbxFile.Node is CGameCtnChallenge challenge)
			{
				if (SelectedClipGroup == 0) challenge.ClipIntro!.Save(localPath);
				else if (SelectedClipGroup == 1) GetGlobalMediaClip(challenge).Save(localPath);
				else if (SelectedClipGroup == 2) challenge.ClipGroupInGame!.Clips[SelectedClip - 1].Clip.Save(localPath);
				else if (SelectedClipGroup == 3) challenge.ClipGroupEndRace!.Clips[SelectedClip - 1].Clip.Save(localPath);
			}

			GbxFile = Gbx.Parse(GbxPath!);

			StatusLabel = "Export As Clip Successful";
		}
	}

	[RelayCommand]
	private void AddTranslationRow()
	{
		float startTime = 0;
		float endTime = 3;
		if (TranslationRows.Count > 0)
		{
			var lastRow = TranslationRows[^1];
			startTime = lastRow.EndTime;
			endTime = startTime + 3;
		}
		var newRow = new TranslationRow(startTime, endTime, 0, 0, 0);
		TranslationRows.Add(newRow);
		SelectedTranslationRow = newRow;
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void RemoveTranslationRow()
	{
		if (TranslationRows.Count <= 1)
			return;
		if (SelectedTranslationRow != null)
		{
			TranslationRows.Remove(SelectedTranslationRow);
			if (TranslationRows.Count > 0)
				SelectedTranslationRow = TranslationRows[^1];
			else
				SelectedTranslationRow = null;
		}
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void AddScalingRow()
	{
		float startTime = 0;
		float endTime = 3;
		if (ScalingRows.Count > 0)
		{
			var lastRow = ScalingRows[^1];
			startTime = lastRow.EndTime;
			endTime = startTime + 3;
		}
		var newRow = new ScalingRow(startTime, endTime, 1, 1, 1);
		ScalingRows.Add(newRow);
		SelectedScalingRow = newRow;
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void RemoveScalingRow()
	{
		if (ScalingRows.Count <= 1)
			return;
		if (SelectedScalingRow != null)
		{
			ScalingRows.Remove(SelectedScalingRow);
			if (ScalingRows.Count > 0)
				SelectedScalingRow = ScalingRows[^1];
			else
				SelectedScalingRow = null;
		}
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void AddRotationRow()
	{
		float startTime = 0;
		float endTime = 3;
		if (RotationRows.Count > 0)
		{
			var lastRow = RotationRows[^1];
			startTime = lastRow.EndTime;
			endTime = startTime + 3;
		}
		var newRow = new RotationRow(startTime, endTime, 0, 0, 0, 10);
		RotationRows.Add(newRow);
		SelectedRotationRow = newRow;
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void RemoveRotationRow()
	{
		if (RotationRows.Count <= 1)
			return;
		if (SelectedRotationRow != null)
		{
			RotationRows.Remove(SelectedRotationRow);
			if (RotationRows.Count > 0)
				SelectedRotationRow = RotationRows[^1];
			else
				SelectedRotationRow = null;
		}
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void AddOrbitRow()
	{
		float startTime = 0;
		float endTime = 3;
		float radius = 2;
		int steps = 60;
		float degrees = 360;
		if (OrbitRows.Count > 0)
		{
			var last = OrbitRows[^1];
			startTime = last.EndTime;
			endTime = startTime + 3;
			radius = last.Radius;
			steps = last.Steps;
			degrees = last.Degrees;
		}
		var newRow = new OrbitRow(startTime, endTime, radius, steps, degrees);
		OrbitRows.Add(newRow);
		SelectedOrbitRow = newRow;
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private void RemoveOrbitRow()
	{
		if (OrbitRows.Count <= 1) return;
		if (SelectedOrbitRow != null)
		{
			OrbitRows.Remove(SelectedOrbitRow);
			if (OrbitRows.Count > 0)
				SelectedOrbitRow = OrbitRows[^1];
			else
				SelectedOrbitRow = null;
		}
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
	}

	[RelayCommand]
	private static void OpenRecent()
	{
		// TODO: Implémenter l'ouverture d'un fichier récent
	}

	[RelayCommand]
	private static void ImportFile()
	{
		// TODO: Implémenter l'import de fichier
	}

	[RelayCommand]
	private async Task Save()
	{
		if (string.IsNullOrWhiteSpace(_currentProjectPath))
		{
			await SaveAs();
			return;
		}
		var settings = new ProjectSettings
		{
			OriginX = OriginX,
			OriginY = OriginY,
			OriginZ = OriginZ,
			TranslationRows = [.. TranslationRows],
			RotationRows = [.. RotationRows],
			ScalingRows = [.. ScalingRows],
			OrbitRows = [.. OrbitRows],
			ShadingIntensity = ShadingIntensity,
			Materials = Obj3DModel?.Materials != null ? new List<Triangle3DAnimation.ObjLoader.ObjMaterial>(Obj3DModel.Materials) : new(),
			ObjPath = ObjPath,
			GbxPath = GbxPath,
			IsTranslationAnimation = IsTranslationAnimation,
			IsScalingAnimation = IsScalingAnimation,
			IsRotationAnimation = IsRotationAnimation,
			IsOrbitEnabled = IsOrbitEnabled
		};
		var json = JsonSerializer.Serialize(settings, ProjectSettingsJsonContext.Default.ProjectSettings);
		File.WriteAllText(_currentProjectPath, json);
		StatusLabel = $"Project saved in {Path.GetFileName(_currentProjectPath)}";
	}

	[RelayCommand]
	private async Task SaveAs()
	{
		var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
		if (window == null)
			return;
		var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Save as...",
			SuggestedFileName = "project.json",
			FileTypeChoices = [jsonType]
		});
		if (file == null)
			return;

		_currentProjectPath = file.Path.LocalPath; // remember new path

		var settings = new ProjectSettings
		{
			OriginX = OriginX,
			OriginY = OriginY,
			OriginZ = OriginZ,
			TranslationRows = [.. TranslationRows],
			RotationRows = [.. RotationRows],
			ScalingRows = [.. ScalingRows],
			OrbitRows = [.. OrbitRows],
			ShadingIntensity = ShadingIntensity,
			Materials = Obj3DModel?.Materials != null ? new List<Triangle3DAnimation.ObjLoader.ObjMaterial>(Obj3DModel.Materials) : new(),
			ObjPath = ObjPath,
			GbxPath = GbxPath,
			IsTranslationAnimation = IsTranslationAnimation,
			IsScalingAnimation = IsScalingAnimation,
			IsRotationAnimation = IsRotationAnimation,
			IsOrbitEnabled = IsOrbitEnabled
		};
		var json = JsonSerializer.Serialize(settings, ProjectSettingsJsonContext.Default.ProjectSettings);
		await using var stream = await file.OpenWriteAsync();
		using var writer = new StreamWriter(stream);
		writer.Write(json);
		await writer.FlushAsync();
		StatusLabel = $"Project saved as {file.Name}";
	}

	[RelayCommand]
	private static void Exit()
	{
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.Shutdown();
	}

	[RelayCommand]
	private void New()
	{
		ObjPath = null;
		GbxPath = null;
		Obj3DModel = null;
		GbxFile = null;
		OriginX = 512;
		OriginY = 0;
		OriginZ = 512;
		ShadingIntensity = 0.0f;
		IsTranslationAnimation = false;
		IsScalingAnimation = false;
		IsRotationAnimation = false;
		ShowWireframe = false;
		ShowFaces = true;
		ShowVertices = false;
		ShowColor = false;
		ShowRender = false;
		ShowOrigin = false;
		ShowMaterialsTable = false;
		ShowGrid = true;
		SelectedClipGroup = 2;
		SelectedClip = 0;
		SelectedTrack = 0;
		TranslationRows.Clear();
		var t = new TranslationRow(0, 3, 0, 0, 0);
		TranslationRows.Add(t);
		t.PropertyChanged += AnyRow_PropertyChanged;
		ScalingRows.Clear();
		var s = new ScalingRow(0, 3, 1, 1, 1);
		ScalingRows.Add(s);
		s.PropertyChanged += AnyRow_PropertyChanged;
		RotationRows.Clear();
		var r = new RotationRow(0, 3, 0, 0, 0, 10);
		RotationRows.Add(r);
		r.PropertyChanged += AnyRow_PropertyChanged;
		OrbitRows.Clear();
		var o = new OrbitRow(0, 3, 2, 60);
		OrbitRows.Add(o);
		o.PropertyChanged += AnyRow_PropertyChanged;
		IsOrbitEnabled = false;
		StatusLabel = "New project";
		FileSizeInfo = null;
		ObjFileSize = null;
		GbxFileSize = null;
		EstimatedExportGbxSize = null;
		IsExportSizeTooLarge = false;
		_currentProjectPath = null;
		UpdateAnimationDuration();
	}

	private static string FormatFileSize(long fileSize)
	{
		if (fileSize < 1024) return $"{fileSize} B";
		if (fileSize < 1024 * 1024) return $"{fileSize / 1024.0:F2} KB";
		if (fileSize < 1024 * 1024 * 1024) return $"{fileSize / (1024.0 * 1024):F2} MB";
		return $"{fileSize / (1024.0 * 1024 * 1024):F2} GB";
	}

	private void OnTranslationRowsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		InvalidateAnimationFramesCache();
		if (e.NewItems != null)
			foreach (TranslationRow row in e.NewItems)
				row.PropertyChanged += AnyRow_PropertyChanged;
		if (e.OldItems != null)
			foreach (TranslationRow row in e.OldItems)
				row.PropertyChanged -= AnyRow_PropertyChanged;
		UpdateAnimationDuration();
	}
	private void OnScalingRowsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		InvalidateAnimationFramesCache();
		if (e.NewItems != null)
			foreach (ScalingRow row in e.NewItems)
				row.PropertyChanged += AnyRow_PropertyChanged;
		if (e.OldItems != null)
			foreach (ScalingRow row in e.OldItems)
				row.PropertyChanged -= AnyRow_PropertyChanged;
		UpdateAnimationDuration();
	}
	private void OnRotationRowsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		InvalidateAnimationFramesCache();
		if (e.NewItems != null)
			foreach (RotationRow row in e.NewItems)
				row.PropertyChanged += AnyRow_PropertyChanged;
		if (e.OldItems != null)
			foreach (RotationRow row in e.OldItems)
				row.PropertyChanged -= AnyRow_PropertyChanged;
		UpdateAnimationDuration();
	}
	private void OnOrbitRowsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		InvalidateAnimationFramesCache();
		if (e.NewItems != null)
			foreach (OrbitRow row in e.NewItems)
				row.PropertyChanged += AnyRow_PropertyChanged;
		if (e.OldItems != null)
			foreach (OrbitRow row in e.OldItems)
				row.PropertyChanged -= AnyRow_PropertyChanged;
		UpdateAnimationDuration();
	}
	private void AnyRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		InvalidateAnimationFramesCache();
		UpdateAnimationDuration();
		if (IsPlaying) Pause(); // Mettre en pause sans remise à zéro
		if (e.PropertyName == nameof(TranslationRow.StartTime) || e.PropertyName == nameof(TranslationRow.EndTime)
			|| e.PropertyName == nameof(ScalingRow.StartTime) || e.PropertyName == nameof(ScalingRow.EndTime)
			|| e.PropertyName == nameof(RotationRow.StartTime) || e.PropertyName == nameof(RotationRow.EndTime)
			|| e.PropertyName == nameof(OrbitRow.StartTime) || e.PropertyName == nameof(OrbitRow.EndTime)
			|| e.PropertyName == nameof(RotationRow.Steps) || e.PropertyName == nameof(OrbitRow.Steps))
		{
			OnPropertyChanged(nameof(IsTimeValid));
			OnPropertyChanged(nameof(IsParamsValid));
			OnPropertyChanged(nameof(CanExport));
			OnPropertyChanged(nameof(CanExportChallenge));
			OnPropertyChanged(nameof(CanExportClip));
			OnPropertyChanged(nameof(IsTranslationTimeInvalid));
			OnPropertyChanged(nameof(IsScalingTimeInvalid));
			OnPropertyChanged(nameof(IsRotationTimeInvalid));
			OnPropertyChanged(nameof(IsOrbitTimeInvalid));
			OnPropertyChanged(nameof(IsRotationStepsInvalid));
			OnPropertyChanged(nameof(IsOrbitStepsInvalid));
		}
	}

	[RelayCommand]
	private void Play()
	{
		// Si l'animation est terminée, on repart du début
		if (AnimationTime >= _animationDuration)
			AnimationTime = 0f;
		StartAnimation();
	}

	[RelayCommand]
	private void Pause()
	{
		PauseAnimation();
	}

	private void StartAnimation()
	{
		if (_animationTimer == null)
		{
			_animationTimer = new System.Timers.Timer(33); // ~30 FPS
			_animationTimer.Elapsed += (s, e) =>
			{
				AnimationTime += 0.033f;
				if (AnimationTime >= _animationDuration)
				{
					AnimationTime = _animationDuration; // Correction ici : on force la valeur exacte
					if (IsRepeatEnabled)
						AnimationTime = 0f;
					else
						PauseAnimation();
				}
				OnPropertyChanged(nameof(AnimationTime));
			};
		}
		_animationTimer.Start();
		IsPlaying = true;
	}

	private void PauseAnimation()
	{
		_animationTimer?.Stop();
		IsPlaying = false;
	}

	public string FormattedAnimationTime => FormatTime(AnimationTime);

	private static string FormatTime(float seconds)
	{
		int totalMilliseconds = (int)(seconds * 1000);
		int m = (totalMilliseconds / 1000) / 60;
		int s = (totalMilliseconds / 1000) % 60;
		int ms = totalMilliseconds % 1000;
		return $"{m:00}:{s:00}:{ms:000}";
	}

	partial void OnAnimationTimeChanged(float value)
	{
		OnPropertyChanged(nameof(FormattedAnimationTime));
		// Ici, on peut notifier la vue ou forcer le rendu si besoin
		// Par exemple, OnPropertyChanged(nameof(AnimationTime));
	}

	private void UpdateAnimationDuration()
	{
		float max = 0f;
		if (IsTranslationAnimation)
			foreach (var row in TranslationRows) if (row.EndTime > max) max = row.EndTime;
		if (IsScalingAnimation)
			foreach (var row in ScalingRows) if (row.EndTime > max) max = row.EndTime;
		if (IsRotationAnimation)
			foreach (var row in RotationRows) if (row.EndTime > max) max = row.EndTime;
		if (IsOrbitEnabled)
			foreach (var row in OrbitRows) if (row.EndTime > max) max = row.EndTime;
		_animationDuration = max;
	}

	[RelayCommand]
	private void ResetAnimation()
	{
		AnimationTime = 0f;
	}

	[RelayCommand]
	private void GoToEndAnimation()
	{
		AnimationTime = _animationDuration;
	}

	public float AnimationDuration => _animationDuration;

	// Correction finale : seules les animations cochées sont prises en compte
	public bool IsTimeValid
	{
		get
		{
			if (IsTranslationAnimation && SelectedTranslationRow != null && SelectedTranslationRow.StartTime > SelectedTranslationRow.EndTime)
				return false;
			if (IsScalingAnimation && SelectedScalingRow != null && SelectedScalingRow.StartTime > SelectedScalingRow.EndTime)
				return false;
			if (IsRotationAnimation && SelectedRotationRow != null && SelectedRotationRow.StartTime > SelectedRotationRow.EndTime)
				return false;
			if (IsOrbitEnabled && SelectedOrbitRow != null && SelectedOrbitRow.StartTime > SelectedOrbitRow.EndTime)
				return false;
			return true;
		}
	}

	public bool IsParamsValid
	{
		get
		{
			if (!IsTimeValid) return false;
			if (IsRotationStepsInvalid) return false;
			if (IsOrbitStepsInvalid) return false;
			return true;
		}
	}

	public bool IsTranslationTimeInvalid => IsTranslationAnimation && SelectedTranslationRow != null && SelectedTranslationRow.StartTime > SelectedTranslationRow.EndTime;
	public bool IsScalingTimeInvalid => IsScalingAnimation && SelectedScalingRow != null && SelectedScalingRow.StartTime > SelectedScalingRow.EndTime;
	public bool IsRotationTimeInvalid => IsRotationAnimation && SelectedRotationRow != null && SelectedRotationRow.StartTime > SelectedRotationRow.EndTime;
	public bool IsOrbitTimeInvalid => IsOrbitEnabled && SelectedOrbitRow != null && SelectedOrbitRow.StartTime > SelectedOrbitRow.EndTime;
	public bool IsRotationStepsInvalid => IsRotationAnimation && SelectedRotationRow != null && SelectedRotationRow.Steps <= 0;
	public bool IsOrbitStepsInvalid => IsOrbitEnabled && SelectedOrbitRow != null && SelectedOrbitRow.Steps <= 0;

	// Méthode à utiliser par la vue pour obtenir les frames d'animation (mise à jour du cache si dirty)
	public List<AnimationFrame> GetOrUpdateAnimationFrames()
	{
		if (_animationFramesCache == null || _animationFramesDirty)
		{
			_animationFramesCache = GenerateAnimationFrames();
			_animationFramesDirty = false;
		}
		return _animationFramesCache;
	}

	// Génère les frames d'animation selon les paramètres courants
	private List<AnimationFrame> GenerateAnimationFrames()
	{
		if (Obj3DModel == null)
			return new List<AnimationFrame>();
		var baseModel = new BaseModel(Obj3DModel, 0f);
		var animation = new SingleBlockTriangleAnimation(baseModel);
		if (IsTranslationAnimation)
		{
			foreach (var row in TranslationRows)
			{
				if (row.StartTime < row.EndTime)
				{
					animation.AddTransformation(new Translation(
						new GBX.NET.Vec3(row.X, row.Y, row.Z),
						TimeSingle.FromSeconds(row.StartTime),
						TimeSingle.FromSeconds(row.EndTime)));
				}
			}
		}
		if (IsScalingAnimation)
		{
			foreach (var row in ScalingRows)
			{
				if (row.StartTime < row.EndTime)
				{
					animation.AddTransformation(new Scaling(
						row.ScaleX, row.ScaleY, row.ScaleZ,
						TimeSingle.FromSeconds(row.StartTime),
						TimeSingle.FromSeconds(row.EndTime)));
				}
			}
		}
		if (IsRotationAnimation)
		{
			foreach (var row in RotationRows)
			{
				if (row.StartTime < row.EndTime)
				{
					animation.AddTransformation(new Rotation(
						row.X, row.Y, row.Z, row.Steps,
						TimeSingle.FromSeconds(row.StartTime),
						TimeSingle.FromSeconds(row.EndTime)));
				}
			}
		}
		animation.GenerateFrames();
		return animation.AnimationFrames.OrderBy(f => f.Time.TotalSeconds).ToList();
	}

	// Invalider le cache d'animation à chaque changement de paramètre
	public void InvalidateAnimationFramesCache()
	{
		_animationFramesDirty = true;
	}
}


