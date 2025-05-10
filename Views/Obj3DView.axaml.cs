using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using Triangle3DAnimation.ObjLoader;
using Triangle3DAnimation.Animation;
using Triangle3DAnimation.Animation.Transformations;
using TmEssentials;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;

namespace Triangle3DAnimationGUI.Views
{
    public partial class Obj3DView : UserControl
    {
        public static readonly StyledProperty<ObjModel?> ModelProperty =
            AvaloniaProperty.Register<Obj3DView, ObjModel?>(nameof(Model));

        public static readonly StyledProperty<bool> ShowWireframeProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowWireframe), true);
        public static readonly StyledProperty<bool> ShowFacesProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowFaces), true);
        public static readonly StyledProperty<bool> ShowVerticesProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowVertices), false);
        public static readonly StyledProperty<bool> ShowColorProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowColor), false);
        public static readonly StyledProperty<bool> ShowRenderProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowRender), false);
        public static readonly StyledProperty<bool> ShowOriginProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowOrigin), false);
        public static readonly StyledProperty<bool> ShowGridProperty =
            AvaloniaProperty.Register<Obj3DView, bool>(nameof(ShowGrid), false);

        public static readonly StyledProperty<float?> OriginXProperty =
            AvaloniaProperty.Register<Obj3DView, float?>(nameof(OriginX), 0f);
        public static readonly StyledProperty<float?> OriginYProperty =
            AvaloniaProperty.Register<Obj3DView, float?>(nameof(OriginY), 0f);
        public static readonly StyledProperty<float?> OriginZProperty =
            AvaloniaProperty.Register<Obj3DView, float?>(nameof(OriginZ), 0f);

        public static readonly StyledProperty<float> AnimationTimeProperty =
            AvaloniaProperty.Register<Obj3DView, float>(nameof(AnimationTime), 0f);

        public ObjModel? Model
        {
            get => GetValue(ModelProperty);
            set
            {
                SetValue(ModelProperty, value);
                InvalidateVisual();
            }
        }

        public bool ShowWireframe
        {
            get => GetValue(ShowWireframeProperty);
            set
            {
                SetValue(ShowWireframeProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowFaces
        {
            get => GetValue(ShowFacesProperty);
            set
            {
                SetValue(ShowFacesProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowVertices
        {
            get => GetValue(ShowVerticesProperty);
            set
            {
                SetValue(ShowVerticesProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowColor
        {
            get => GetValue(ShowColorProperty);
            set
            {
                SetValue(ShowColorProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowRender
        {
            get => GetValue(ShowRenderProperty);
            set
            {
                SetValue(ShowRenderProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowOrigin
        {
            get => GetValue(ShowOriginProperty);
            set
            {
                SetValue(ShowOriginProperty, value);
                InvalidateVisual();
            }
        }
        public bool ShowGrid
        {
            get => GetValue(ShowGridProperty);
            set
            {
                SetValue(ShowGridProperty, value);
                InvalidateVisual();
            }
        }

        public float? OriginX
        {
            get => GetValue(OriginXProperty);
            set
            {
                SetValue(OriginXProperty, value);
                InvalidateVisual();
            }
        }
        public float? OriginY
        {
            get => GetValue(OriginYProperty);
            set
            {
                SetValue(OriginYProperty, value);
                InvalidateVisual();
            }
        }
        public float? OriginZ
        {
            get => GetValue(OriginZProperty);
            set
            {
                SetValue(OriginZProperty, value);
                InvalidateVisual();
            }
        }

        public float AnimationTime
        {
            get => GetValue(AnimationTimeProperty);
            set
            {
                SetValue(AnimationTimeProperty, value);
                InvalidateVisual();
            }
        }

        private double _angleX = 30; // Angle initial pour éviter un alignement parfait
        private double _angleY = 30; // Angle initial pour une vue 3D directe
        private Point? _lastPos;

        private double _zoom = 1.0; // Zoom factor

        private DateTime _lastRender = DateTime.MinValue;
        private static readonly TimeSpan RenderThrottle = TimeSpan.FromMilliseconds(16); // ~60 FPS

        private List<System.ComponentModel.INotifyPropertyChanged>? _lastMaterialSubscriptions;

        public Obj3DView()
        {
            InitializeComponent();
            InvalidateVisual(); // Forcer le rafraîchissement dès la création
            PropertyChanged += (s, e) =>
            {
                if (e.Property == ModelProperty || e.Property == ShowWireframeProperty || e.Property == ShowFacesProperty || e.Property == ShowVerticesProperty || e.Property == ShowColorProperty || e.Property == ShowRenderProperty || e.Property == ShowOriginProperty || e.Property == ShowGridProperty || e.Property == OriginXProperty || e.Property == OriginYProperty || e.Property == OriginZProperty || e.Property == AnimationTimeProperty)
                    InvalidateVisual();
                if (e.Property == ModelProperty)
                {
                    AttachMaterialPropertyChangedHandler();
                }
            };
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += (_, __) => _lastPos = null;
            PointerWheelChanged += OnPointerWheelChanged;
            AttachMaterialPropertyChangedHandler();
        }

        private void AttachMaterialPropertyChangedHandler()
        {
            // Détacher les anciens abonnements
            if (_lastMaterialSubscriptions != null)
            {
                foreach (var mat in _lastMaterialSubscriptions)
                {
                    mat.PropertyChanged -= Material_PropertyChanged;
                }
            }
            _lastMaterialSubscriptions = null;

            // S'abonner aux nouveaux matériaux
            if (Model?.Materials != null)
            {
                _lastMaterialSubscriptions = new List<System.ComponentModel.INotifyPropertyChanged>();
                foreach (var mat in Model.Materials)
                {
                    if (mat is System.ComponentModel.INotifyPropertyChanged inpc)
                    {
                        inpc.PropertyChanged -= Material_PropertyChanged;
                        inpc.PropertyChanged += Material_PropertyChanged;
                        _lastMaterialSubscriptions.Add(inpc);
                    }
                }
            }
            InvalidateVisual(); // Force le rafraîchissement du rendu
        }

        private void Material_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _lastPos = e.GetPosition(this);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_lastPos is Point last && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var pos = e.GetPosition(this);
                _angleY -= (pos.X - last.X) * 0.5; // Inversion of horizontal direction
                _angleX += (pos.Y - last.Y) * 0.5;
                _lastPos = pos;
                var now = DateTime.Now;
                if (now - _lastRender > RenderThrottle)
                {
                    _lastRender = now;
                    InvalidateVisual();
                }
            }
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            // Direction of scroll: Delta.Y > 0 = zoom in, < 0 = zoom out
            if (e.Delta.Y > 0)
                _zoom *= 1.1;
            else if (e.Delta.Y < 0)
                _zoom /= 1.1;
            // Clamp zoom to avoid extreme values
            _zoom = Math.Clamp(_zoom, 0.1, 10.0);
            InvalidateVisual();
            e.Handled = true; // Empêche la propagation à la ScrollBar
        }

        // Applique une rotation de 90° autour de l'axe Y à tous les sommets du modèle
        private void OnRotateYClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Model?.Vertices == null)
                return;
            double angle = Math.PI / 2.0; // 90° en radians
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            foreach (var v in Model.Vertices)
            {
                float x = v.X;
                float z = v.Z;
                float newX = (float)(x * cos - z * sin);
                float newZ = (float)(x * sin + z * cos);
                typeof(Triangle3DAnimation.ObjLoader.ObjVertex).GetProperty("X")?.SetValue(v, newX);
                typeof(Triangle3DAnimation.ObjLoader.ObjVertex).GetProperty("Z")?.SetValue(v, newZ);
            }
            if (DataContext is Triangle3DAnimationGUI.ViewModels.MainWindowViewModel vm)
                vm.InvalidateAnimationFramesCache();
            InvalidateVisual();
        }

        // Applique une rotation de 90° autour de l'axe X à tous les sommets du modèle
        private void OnRotateXClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Model?.Vertices == null)
                return;
            double angle = Math.PI / 2.0; // 90° en radians
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            foreach (var v in Model.Vertices)
            {
                float y = v.Y;
                float z = v.Z;
                float newY = (float)(y * cos - z * sin);
                float newZ = (float)(y * sin + z * cos);
                typeof(Triangle3DAnimation.ObjLoader.ObjVertex).GetProperty("Y")?.SetValue(v, newY);
                typeof(Triangle3DAnimation.ObjLoader.ObjVertex).GetProperty("Z")?.SetValue(v, newZ);
            }
            if (DataContext is Triangle3DAnimationGUI.ViewModels.MainWindowViewModel vm)
                vm.InvalidateAnimationFramesCache();
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Add a dark gray background that extends upwards
            context.FillRectangle(new SolidColorBrush(Color.FromRgb(34, 34, 34)), new Rect(0, -50, Bounds.Width, Bounds.Height + 50));

            // Déclarer les variables de transformation UNE SEULE FOIS
            var width = Bounds.Width;
            var height = Bounds.Height;
            var center = new Point(width / 2, height / 2);
            var scale = Math.Min(width, height) * 0.01 * _zoom;
            var rx = Math.PI * _angleX / 180.0;
            var ry = Math.PI * _angleY / 180.0;
            var cosx = Math.Cos(rx); var sinx = Math.Sin(rx);
            var cosy = Math.Cos(ry); var siny = Math.Sin(ry);

            // === SOL 3D et GRILLE/Axes ===
            if (ShowGrid)
            {
                // --- Plane (sol gris) ---
                double solY = -1.0;
                double solSize = 512.0; // 512 unités de chaque côté, soit 1024x1024
                var solPoints3D = new[]
                {
                    new { X = -solSize, Y = solY, Z = -solSize },
                    new { X =  solSize, Y = solY, Z = -solSize },
                    new { X =  solSize, Y = solY, Z =  solSize },
                    new { X = -solSize, Y = solY, Z =  solSize }
                };
                var solPoints2D = new Point[4];
                for (int i = 0; i < 4; i++)
                {
                    var p = solPoints3D[i];
                    // Rotation Y
                    var x = p.X * cosy - p.Z * siny;
                    var z = p.X * siny + p.Z * cosy;
                    // Rotation X
                    var y = p.Y * cosx - z * sinx;
                    z = p.Y * sinx + z * cosx;
                    // Projection
                    var px = center.X + x * scale;
                    var py = center.Y - y * scale;
                    solPoints2D[i] = new Point(px, py);
                }
                var solGeometry = new StreamGeometry();
                using (var ctx = solGeometry.Open())
                {
                    ctx.BeginFigure(solPoints2D[0], true);
                    ctx.LineTo(solPoints2D[1]);
                    ctx.LineTo(solPoints2D[2]);
                    ctx.LineTo(solPoints2D[3]);
                    ctx.EndFigure(true);
                }
                context.DrawGeometry(new SolidColorBrush(Color.FromRgb(80, 80, 80)), null, solGeometry);

                // --- Grille ---
                int gridStep = 32;
                var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(120, 120, 120)), 1);
                // Lignes parallèles à X (Z varie)
                for (int z = -(int)solSize; z <= (int)solSize; z += gridStep)
                {
                    var p1 = new { X = -solSize, Y = solY, Z = (double)z };
                    var p2 = new { X = solSize, Y = solY, Z = (double)z };
                    // Projection 3D -> 2D (harmonisée)
                    double x1 = p1.X * cosy - p1.Z * siny;
                    double z1 = p1.X * siny + p1.Z * cosy;
                    double y1 = p1.Y * cosx - z1 * sinx;
                    z1 = p1.Y * sinx + z1 * cosx;
                    double px1 = center.X + x1 * scale;
                    double py1 = center.Y - y1 * scale;

                    double x2 = p2.X * cosy - p2.Z * siny;
                    double z2 = p2.X * siny + p2.Z * cosy;
                    double y2_grid = p2.Y * cosx - z2 * sinx;
                    z2 = p2.Y * sinx + z2 * cosx;
                    double px2 = center.X + x2 * scale;
                    double py2 = center.Y - y2_grid * scale;

                    context.DrawLine(gridPen, new Point(px1, py1), new Point(px2, py2));
                }
                // Lignes parallèles à Z (X varie)
                for (int x = -(int)solSize; x <= (int)solSize; x += gridStep)
                {
                    var p1 = new { X = (double)x, Y = solY, Z = -solSize };
                    var p2 = new { X = (double)x, Y = solY, Z = solSize };
                    // Projection 3D -> 2D
                    double x1 = p1.X * cosy - p1.Z * siny;
                    double z1 = p1.X * siny + p1.Z * cosy;
                    double y1 = p1.Y * cosx - z1 * sinx;
                    z1 = p1.Y * sinx + z1 * cosx;
                    double px1 = center.X + x1 * scale;
                    double py1 = center.Y - y1 * scale;

                    double x2 = p2.X * cosy - p2.Z * siny;
                    double z2 = p2.X * siny + p2.Z * cosy;
                    double y2_grid = p2.Y * cosx - z2 * sinx;
                    z2 = p2.Y * sinx + z2 * cosx;
                    double px2 = center.X + x2 * scale;
                    double py2 = center.Y - y2_grid * scale;

                    context.DrawLine(gridPen, new Point(px1, py1), new Point(px2, py2));
                }

                // --- Axes ---
                // Axe Z (bleu, vertical)
                var zAxisPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 50, 205, 50)), 2); // 50% transparent green
                var zLength = solSize; // même longueur que les autres axes
                var zStart3D = new { X = 0.0, Y = solY, Z = 0.0 };
                var zEnd3D = new { X = 0.0, Y = solY + zLength, Z = 0.0 };
                // Projection 3D -> 2D pour Z (vertical)
                double zx1 = zStart3D.X * cosy - zStart3D.Z * siny;
                double zz1 = zStart3D.X * siny + zStart3D.Z * cosy;
                double zy1_proj = zStart3D.Y * cosx - zz1 * sinx;
                zz1 = zStart3D.Y * sinx + zz1 * cosx;
                double pzx1 = center.X + zx1 * scale;
                double pzy1 = center.Y - zy1_proj * scale;

                double zx2 = zEnd3D.X * cosy - zEnd3D.Z * siny;
                double zz2 = zEnd3D.X * siny + zEnd3D.Z * cosy;
                double zy2_proj = zEnd3D.Y * cosx - zz2 * sinx;
                zz2 = zEnd3D.Y * sinx + zz2 * cosx;
                double pzx2 = center.X + zx2 * scale;
                double pzy2 = center.Y - zy2_proj * scale;

                context.DrawLine(zAxisPen, new Point(pzx1, pzy1), new Point(pzx2, pzy2));

                // Axe X (rouge, horizontal)
                var xAxisPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)), 2); // 50% transparent red
                var xStart3D = new { X = -solSize, Y = solY, Z = 0.0 };
                var xEnd3D = new { X = solSize, Y = solY, Z = 0.0 };
                // Projection 3D -> 2D pour X (horizontal)
                double xx1 = xStart3D.X * cosy - xStart3D.Z * siny;
                double xz1 = xStart3D.X * siny + xStart3D.Z * cosy;
                double xy1_proj = xStart3D.Y * cosx - xz1 * sinx;
                xz1 = xStart3D.Y * sinx + xz1 * cosx;
                double pxx1 = center.X + xx1 * scale;
                double pxy1 = center.Y - xy1_proj * scale;

                double xx2 = xEnd3D.X * cosy - xEnd3D.Z * siny;
                double xz2 = xEnd3D.X * siny + xEnd3D.Z * cosy;
                double xy2_proj = xEnd3D.Y * cosx - xz2 * sinx;
                xz2 = xEnd3D.Y * sinx + xz2 * cosx;
                double pxx2 = center.X + xx2 * scale;
                double pxy2 = center.Y - xy2_proj * scale;

                context.DrawLine(xAxisPen, new Point(pxx1, pxy1), new Point(pxx2, pxy2));

                // Axe Y (vert, horizontal perpendiculaire à X)
                var yAxisPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 2); // 50% transparent blue
                var yStart3D = new { X = 0.0, Y = solY, Z = -solSize };
                var yEnd3D = new { X = 0.0, Y = solY, Z = solSize };
                // Projection 3D -> 2D pour Y (horizontal profondeur)
                double yx1 = yStart3D.X * cosy - yStart3D.Z * siny;
                double yz1 = yStart3D.X * siny + yStart3D.Z * cosy;
                double yy1_proj = yStart3D.Y * cosx - yz1 * sinx;
                yz1 = yStart3D.Y * sinx + yz1 * cosx;
                double pyx1 = center.X + yx1 * scale;
                double pyy1 = center.Y - yy1_proj * scale;

                double yx2 = yEnd3D.X * cosy - yEnd3D.Z * siny;
                double yz2 = yEnd3D.X * siny + yEnd3D.Z * cosy;
                double yy2_proj = yEnd3D.Y * cosx - yz2 * sinx;
                yz2 = yEnd3D.Y * sinx + yz2 * cosx;
                double pyx2 = center.X + yx2 * scale;
                double pyy2 = center.Y - yy2_proj * scale;

                context.DrawLine(yAxisPen, new Point(pyx1, pyy1), new Point(pyx2, pyy2));
            }
            // === FIN GRILLE ===

            if (!ShowRender)
            {
                // Do not display anything if ShowRender is false
                return;
            }

            if (Model == null)
            {
                context.DrawText(
                    new FormattedText("Model null", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 12, Brushes.White),
                    new Point(10, 30));
                // Display unit of measurement even if the model is null
                var unitText = new FormattedText("1 unit = 1 meter", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 12, Brushes.LightGray);
                var unitPos = new Point(10, Bounds.Height - unitText.Height - 10);
                context.DrawText(unitText, unitPos);
                return;
            }

            // --- Animation du modèle ---
            // Utilisation du cache d'animation du ViewModel
            if (DataContext is not Triangle3DAnimationGUI.ViewModels.MainWindowViewModel vm)
            {
                // fallback : pas d'animation
                goto PasAnim;
            }
            var frames = vm.GetOrUpdateAnimationFrames();
            if (frames.Count == 0)
                goto PasAnim;
            AnimationFrame? before = null, after = null;
            foreach (var f in frames)
            {
                if (f.Time.TotalSeconds <= AnimationTime)
                    before = f;
                if (f.Time.TotalSeconds >= AnimationTime)
                {
                    after = f;
                    break;
                }
            }
            if (before == null) before = frames.First();
            if (after == null) after = frames.Last();
            List<GBX.NET.Vec3> animatedVertices;
            if (before == after || before == null || after == null)
            {
                animatedVertices = (before ?? after ?? frames[0]).VerticesPositions;
            }
            else
            {
                float t0 = (float)before.Time.TotalSeconds;
                float t1 = (float)after.Time.TotalSeconds;
                float alpha = (t1 - t0) > 0 ? (AnimationTime - t0) / (t1 - t0) : 0f;
                animatedVertices = new List<GBX.NET.Vec3>(before.VerticesPositions.Count);
                for (int i = 0; i < before.VerticesPositions.Count; i++)
                {
                    var v0 = before.VerticesPositions[i];
                    var v1 = after.VerticesPositions[i];
                    var interp = new GBX.NET.Vec3(
                        v0.X + (v1.X - v0.X) * alpha,
                        v0.Y + (v1.Y - v0.Y) * alpha,
                        v0.Z + (v1.Z - v0.Z) * alpha
                    );
                    animatedVertices.Add(interp);
                }
            }

            if (Model.Vertices == null || Model.Vertices.Count == 0 || Model.Faces == null || Model.Faces.Count == 0)
            {
                context.DrawText(
                    new FormattedText($"Empty model: {Model.Vertices?.Count ?? 0} vertices, {Model.Faces?.Count ?? 0} faces", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 16, Brushes.Red),
                    new Point(10, 30));
                // Display unit of measurement even if the model is empty
                var unitText = new FormattedText("1 unit = 1 meter", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 12, Brushes.LightGray);
                var unitPos = new Point(10, Bounds.Height - unitText.Height - 10);
                context.DrawText(unitText, unitPos);
                return;
            }

            // Calculer le minY du modèle pour poser le bas du modèle sur la grille (sol à Y = -1)
            float minYModel = float.MaxValue;
            foreach (var v in Model.Vertices)
                if (v.Y < minYModel) minYModel = v.Y;
            float decalageY = -1f - minYModel;
            float originX = (float)(OriginX ?? 0f) - 512f;
            float originY = (float)(OriginY ?? 0f);
            float originZ = (float)(OriginZ ?? 0f) - 512f;
            var projectedVertices = new Point[animatedVertices.Count];

            // Paramètres de l'orbite
            double orbitRadius = 100.0;
            double duration = vm != null ? vm.AnimationDuration : 1.0;
            double degrees = 360.0;
            if (vm != null && vm.IsOrbitEnabled && vm.OrbitRows.Count > 0)
            {
                var row = vm.OrbitRows[0];
                orbitRadius = row.Radius * -10.0;
                degrees = row.Degrees;
                duration = row.EndTime - row.StartTime;
            }
            double orbitAngle = degrees * Math.PI / 180.0 * (AnimationTime / duration);
            double orbitCenterX = originX;
            double orbitCenterZ = originZ;

            // Calcul du décalage d'orbite
            double orbitOffsetX = 0.0;
            double orbitOffsetZ = 0.0;
            if (vm != null && vm.IsOrbitEnabled && vm.OrbitRows.Count > 0 && AnimationTime > 0)
            {
                // À t=0, le modèle doit rester à sa position d'origine
                orbitOffsetX = orbitRadius * (Math.Cos(orbitAngle) - 1.0);
                orbitOffsetZ = orbitRadius * Math.Sin(orbitAngle);
                orbitOffsetX += orbitCenterX;
                orbitOffsetZ += orbitCenterZ;
            }

            for (int i = 0; i < animatedVertices.Count; i++)
            {
                var vert = animatedVertices[i];
                // Application du décalage d'origine
                var xDecale = vert.X + originX;
                var yDecale = vert.Y + decalageY + originY;
                var zDecale = vert.Z + originZ;

                // Ajout du déplacement d'orbite (le cube tourne autour d'un point)
                if (vm != null && vm.IsOrbitEnabled && vm.OrbitRows.Count > 0 && AnimationTime > 0)
                {
                    xDecale += (float)(orbitOffsetX - orbitCenterX);
                    zDecale += (float)(orbitOffsetZ - orbitCenterZ);
                }

                // Rotation Y
                var x = xDecale * cosy - zDecale * siny;
                var z = xDecale * siny + zDecale * cosy;
                // Rotation X
                var y = yDecale * cosx - z * sinx;
                z = yDecale * sinx + z * cosx;
                // Projection
                var px = center.X + x * scale;
                var py = center.Y - y * scale;
                projectedVertices[i] = new Point(px, py);
            }

            int bmpWidth = (int)Bounds.Width;
            int bmpHeight = (int)Bounds.Height;
            var wbmp = new WriteableBitmap(new PixelSize(bmpWidth, bmpHeight), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
            float[,] zBuffer = new float[bmpWidth, bmpHeight];
            for (int x = 0; x < bmpWidth; x++)
                for (int y = 0; y < bmpHeight; y++)
                    zBuffer[x, y] = float.MaxValue;
            int[] pixels = new int[bmpWidth * bmpHeight];

            // Display faces if ShowFaces is checked
            if (ShowFaces)
            {
                var facesWithDepth = new List<(ObjFace face, double depth, bool isDegenerate, bool isVisible, List<Point> points2D, List<(float x, float y, float z)> pointsCam)>();
                foreach (var face in Model.Faces)
                {
                    var verts = face.Vertices.ToList();
                    var points2D = new List<Point>();
                    var pointsCam = new List<(float x, float y, float z)>();
                    double zsum = 0;
                    int count = 0;
                    var idxs = new HashSet<int>();
                    foreach (var v in verts)
                    {
                        var idx = v.Vertex.Index - 1;
                        if (idx >= 0 && idx < animatedVertices.Count)
                        {
                            var vert = animatedVertices[idx];
                            var xDecale = vert.X + originX;
                            var yDecale = vert.Y + decalageY + originY;
                            var zDecale = vert.Z + originZ;
                            if (vm != null && vm.IsOrbitEnabled && vm.OrbitRows.Count > 0 && AnimationTime > 0)
                            {
                                xDecale += (float)(orbitOffsetX - orbitCenterX);
                                zDecale += (float)(orbitOffsetZ - orbitCenterZ);
                            }
                            var x = xDecale * cosy - zDecale * siny;
                            var z = xDecale * siny + zDecale * cosy;
                            var y = yDecale * cosx - z * sinx;
                            z = yDecale * sinx + z * cosx;
                            zsum += z;
                            count++;
                            idxs.Add(idx);
                            var px = (int)Math.Round(center.X + x * scale);
                            var py = (int)Math.Round(center.Y - y * scale);
                            points2D.Add(new Point(px, py));
                            pointsCam.Add((px, py, (float)z));
                        }
                    }
                    bool isDegenerate = idxs.Count < 3;
                    bool isVisible = true; // Affiche toutes les faces, sans backface culling
                    if (!isDegenerate && verts.Count >= 3)
                    {
                        var a = animatedVertices[verts[0].Vertex.Index - 1];
                        var b = animatedVertices[verts[1].Vertex.Index - 1];
                        var c = animatedVertices[verts[2].Vertex.Index - 1];
                        var ux = b.X - a.X; var uy = b.Y - a.Y; var uz = b.Z - a.Z;
                        var vx = c.X - a.X; var vy = c.Y - a.Y; var vz = c.Z - a.Z;
                        var nx = uy * vz - uz * vy;
                        var ny = uz * vx - ux * vz;
                        var nz = ux * vy - uy * vx;
                        if (nx == 0 && ny == 0 && nz == 0)
                            isDegenerate = true;
                    }
                    if (count > 0)
                        facesWithDepth.Add((face, zsum / count, isDegenerate, isVisible, points2D, pointsCam));
                }
                foreach (var (face, _, isDegenerate, isVisible, points2D, pointsCam) in facesWithDepth.OrderBy(f => f.depth))
                {
                    // if (!isVisible) continue; // Désactivé : on affiche toutes les faces
                    if (points2D.Count < 3) continue;
                    IBrush brush = Brushes.Gray;
                    if (isDegenerate)
                        brush = Brushes.Red;
                    else if (ShowColor && face.Material != null)
                    {
                        var r = face.Material.DiffuseR;
                        var g = face.Material.DiffuseG;
                        var b = face.Material.DiffuseB;
                        brush = new SolidColorBrush(Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255)));
                    }
                    RasterizeTriangleZBuffer(pointsCam, brush, pixels, bmpWidth, bmpHeight, zBuffer);
                }
                using (var lfb = wbmp.Lock())
                {
                    Marshal.Copy(pixels, 0, lfb.Address, pixels.Length);
                }
                context.DrawImage(wbmp, new Rect(0, 0, bmpWidth, bmpHeight));
            }

            // Display edges if ShowWireframe is checked
            if (ShowWireframe)
            {
                var wirePen = new Pen(Brushes.White, 1);
                var points2D = new List<Point>(16);
                if (Model.OriginalFaces != null && Model.OriginalFaces.Count > 0)
                {
                    foreach (var face in Model.OriginalFaces)
                    {
                        points2D.Clear();
                        foreach (var v in face)
                        {
                            var idx = v.Vertex.Index - 1;
                            if (idx >= 0 && idx < projectedVertices.Length)
                                points2D.Add(projectedVertices[idx]);
                        }
                        if (points2D.Count >= 2)
                        {
                            for (int i = 0; i < points2D.Count; i++)
                            {
                                var p1 = points2D[i];
                                var p2 = points2D[(i + 1) % points2D.Count];
                                context.DrawLine(wirePen, p1, p2);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var face in Model.Faces)
                    {
                        points2D.Clear();
                        foreach (var v in face.Vertices)
                        {
                            var idx = v.Vertex.Index - 1;
                            if (idx >= 0 && idx < projectedVertices.Length)
                                points2D.Add(projectedVertices[idx]);
                        }
                        if (points2D.Count >= 2)
                        {
                            for (int i = 0; i < points2D.Count; i++)
                            {
                                var p1 = points2D[i];
                                var p2 = points2D[(i + 1) % points2D.Count];
                                context.DrawLine(wirePen, p1, p2);
                            }
                        }
                    }
                }
            }

            // Display vertices if ShowVertices is checked
            if (ShowVertices)
            {
                for (int i = 0; i < projectedVertices.Length; i++)
                {
                    var pt = projectedVertices[i];
                    context.DrawEllipse(Brushes.White, null, pt, 2, 2);
                }
            }

            // === Display model origin ===
            if (ShowOrigin)
            {
                float modelOriginX = 0f;
                float modelOriginY = -1f;
                float modelOriginZ = 0f;
                var ox = modelOriginX * cosy - modelOriginZ * siny;
                var oz = modelOriginX * siny + modelOriginZ * cosy;
                var oy = modelOriginY * cosx - oz * sinx;
                oz = modelOriginY * sinx + oz * cosx;
                var opx = center.X + ox * scale;
                var opy = center.Y - oy * scale;
                var origin2D = new Point(opx, opy);
                context.DrawEllipse(Brushes.Yellow, null, origin2D, 3, 3);
            }

            // === Affichage du temps d'animation en haut au centre ===
            if (ShowRender && vm != null && vm.FormattedAnimationTime != null)
            {
                var timeText = new FormattedText(vm.FormattedAnimationTime, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Consolas"), 14, Brushes.White);
                var x = (Bounds.Width - timeText.Width) / 2;
                var y = 8.0;
                context.DrawText(timeText, new Point(x, y));
            }

            // === Affichage des textes toujours au premier plan ===
            var labelFont = new Typeface("Arial");
            var labelBrush = Brushes.LightGray;
            var valueBrush = Brushes.LightGray;
            double labelFontSize = 12;
            double valueFontSize = 12;
            double xLabel = 10;
            double spacing = 16;
            double yStart = 10;
            double lineSpacing = 4;

            var verticesLabel = new FormattedText("Vertices", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, labelFont, labelFontSize, labelBrush);
            var verticesValue = new FormattedText(Model.Vertices.Count.ToString("N0"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, labelFont, valueFontSize, valueBrush);
            double xValue = xLabel + verticesLabel.Width + spacing;
            context.DrawText(verticesLabel, new Point(xLabel, yStart));
            context.DrawText(verticesValue, new Point(xValue, yStart));

            double y2 = yStart + verticesLabel.Height + lineSpacing;
            var facesLabel = new FormattedText("Faces", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, labelFont, labelFontSize, labelBrush);
            int facesCount;
            if (ShowWireframe && Model.OriginalFaces != null && Model.OriginalFaces.Count > 0)
                facesCount = Model.OriginalFaces.Count;
            else if (ShowFaces && Model.OriginalFaces != null && Model.OriginalFaces.Count > 0)
                facesCount = Model.OriginalFaces.Count;
            else
                facesCount = Model.Faces.Count;
            var facesValue = new FormattedText(facesCount.ToString("N0"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, labelFont, valueFontSize, valueBrush);
            double xValue2 = xLabel + facesLabel.Width + spacing;
            context.DrawText(facesLabel, new Point(xLabel, y2));
            context.DrawText(facesValue, new Point(xValue2, y2));

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            foreach (var v in Model.Vertices)
            {
                if (v.X < minX) minX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Z < minZ) minZ = v.Z;
                if (v.X > maxX) maxX = v.X;
                if (v.Y > maxY) maxY = v.Y;
                if (v.Z > maxZ) maxZ = v.Z;
            }
            float widthModel = maxX - minX;
            float heightModel = maxY - minY;
            float depthModel = maxZ - minZ;
            string dimensionsText = $"W{widthModel:0.##}m x H{heightModel:0.##}m x D{depthModel:0.##}m";
            var dimText = new FormattedText(dimensionsText, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 12, Brushes.LightGray);
            var dimPos = new Point(10, Bounds.Height - dimText.Height - 10);
            context.DrawText(dimText, dimPos);
        PasAnim:
            return;
        }

        private void RasterizeTriangleZBuffer(List<(float x, float y, float z)> pts, IBrush brush, int[] pixels, int width, int height, float[,] zBuffer)
        {
            if (pts.Count < 3) return;
            var (x0, y0, z0) = pts[0];
            var (x1, y1, z1) = pts[1];
            var (x2, y2, z2) = pts[2];
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(x0, Math.Min(x1, x2))));
            int maxX = (int)Math.Min(width - 1, Math.Ceiling(Math.Max(x0, Math.Max(x1, x2))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(y0, Math.Min(y1, y2))));
            int maxY = (int)Math.Min(height - 1, Math.Ceiling(Math.Max(y0, Math.Max(y1, y2))));
            var color = (brush as SolidColorBrush)?.Color ?? Colors.Gray;
            uint col = (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
            float denom = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2);
            if (denom == 0) return;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float l1 = ((y1 - y2) * (x - x2) + (x2 - x1) * (y - y2)) / denom;
                    float l2 = ((y2 - y0) * (x - x2) + (x0 - x2) * (y - y2)) / denom;
                    float l3 = 1 - l1 - l2;
                    if (l1 >= 0 && l2 >= 0 && l3 >= 0)
                    {
                        float z = -(l1 * z0 + l2 * z1 + l3 * z2); // Inverse la profondeur pour le z-buffer
                        if (z < zBuffer[x, y])
                        {
                            zBuffer[x, y] = z;
                            int idx = y * width + x;
                            pixels[idx] = (int)col;
                        }
                    }
                }
            }
        }
    }
}
