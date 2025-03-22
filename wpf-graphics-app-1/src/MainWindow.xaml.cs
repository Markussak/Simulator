using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using wpf_graphics_app.Models;

namespace wpf_graphics_app
{
    public partial class MainWindow : Window
    {
        private GeometryModel _geometryModel;
        private Dictionary<MassiveObject, ModelVisual3D> _objectVisuals;
        private Dictionary<List<SpacetimePoint>, ModelVisual3D> _geodesicVisuals;
        private ModelVisual3D _spacetimeGridVisual;
        private ModelVisual3D _gravitationalFieldVisual;
        
        private DispatcherTimer _simulationTimer;
        private double _simulationTime = 0.0;
        private double _timeScale = 1.0;
        private bool _isSimulationRunning = false;
        
        // Camera control variables
        private Point _lastMousePosition;
        private bool _isRotating = false;
        private double _cameraDistance = 5.0;
        
        // Quantum simulation bridge
        private QuantumSimulationBridge _quantumBridge;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize model and collections
            _geometryModel = new GeometryModel();
            _objectVisuals = new Dictionary<MassiveObject, ModelVisual3D>();
            _geodesicVisuals = new Dictionary<List<SpacetimePoint>, ModelVisual3D>();
            
            // Initialize quantum bridge
            _quantumBridge = new QuantumSimulationBridge();
            
            // Initialize spacetime grid
            _geometryModel.InitializeGrid(10, 20.0); // 10x10x10 grid with size 20
            
            // Initialize visualization
            InitializeVisualization();
            
            // Set up simulation timer
            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _simulationTimer.Tick += OnSimulationTick;
            
            // Set up mouse events for camera control
            mainViewport.MouseLeftButtonDown += OnViewportMouseLeftButtonDown;
            mainViewport.MouseLeftButtonUp += OnViewportMouseLeftButtonUp;
            mainViewport.MouseMove += OnViewportMouseMove;
            mainViewport.MouseWheel += OnViewportMouseWheel;
            
            // Update UI
            UpdateUI();
        }
        
        private void InitializeVisualization()
        {
            // Initialize spacetime grid visualization
            _spacetimeGridVisual = CreateSpacetimeGridVisual();
            mainViewport.Children.Add(_spacetimeGridVisual);
            
            // Initialize gravitational field visualization (initially empty)
            _gravitationalFieldVisual = new ModelVisual3D();
            mainViewport.Children.Add(_gravitationalFieldVisual);
        }
        
        private ModelVisual3D CreateSpacetimeGridVisual()
        {
            var gridVisual = new ModelVisual3D();
            var gridGroup = new Model3DGroup();
            
            // Create a simple grid of lines
            var positions = new Point3DCollection();
            var indices = new Int32Collection();
            var colors = new List<Color>();
            
            int gridSize = 10;
            double spacing = 2.0;
            
            // Create grid lines
            int index = 0;
            for (int i = -gridSize; i <= gridSize; i++)
            {
                // X-axis lines
                positions.Add(new Point3D(i * spacing, -gridSize * spacing, 0));
                positions.Add(new Point3D(i * spacing, gridSize * spacing, 0));
                indices.Add(index++);
                indices.Add(index++);
                colors.Add(Colors.Gray);
                colors.Add(Colors.Gray);
                
                // Y-axis lines
                positions.Add(new Point3D(-gridSize * spacing, i * spacing, 0));
                positions.Add(new Point3D(gridSize * spacing, i * spacing, 0));
                indices.Add(index++);
                indices.Add(index++);
                colors.Add(Colors.Gray);
                colors.Add(Colors.Gray);
            }
            
            // Create geometry
            var geometry = new MeshGeometry3D();
            geometry.Positions = positions;
            
            // Create visual
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            var model = new GeometryModel3D(geometry, material);
            
            // Set up lines
            var lineIndices = new Int32Collection();
            for (int i = 0; i < indices.Count; i++)
            {
                lineIndices.Add(indices[i]);
            }
            
            // Create line geometry
            var lineGeometry = new MeshGeometry3D();
            lineGeometry.Positions = positions;
            lineGeometry.TriangleIndices = lineIndices;
            
            // Create line visual
            var lineModel = new GeometryModel3D(lineGeometry, material);
            gridGroup.Children.Add(lineModel);
            
            gridVisual.Content = gridGroup;
            return gridVisual;
        }
        
        private void UpdateGravitationalFieldVisual()
        {
            // Clear existing visualization
            var fieldGroup = new Model3DGroup();
            
            // Create visualization based on metric tensor values
            foreach (var point in _geometryModel.GridPoints)
            {
                if (_geometryModel.SpacetimeMetric.TryGetValue(point, out var metric))
                {
                    // Calculate curvature indicator (simplified)
                    double curvature = Math.Abs(metric[0, 0] + 1.0); // Deviation from flat space
                    
                    if (curvature > 0.001) // Only show significant curvature
                    {
                        // Create a small sphere at this point
                        double size = Math.Min(0.2, curvature * 0.5); // Scale size by curvature
                        var sphere = new MeshGeometry3D();
                        
                        // Create a simple sphere
                        int resolution = 4;
                        CreateSphere(sphere, point.ToPoint3D(), size, resolution);
                        
                        // Color based on curvature
                        Color color = GetCurvatureColor(curvature);
                        var material = new DiffuseMaterial(new SolidColorBrush(color));
                        
                        var sphereModel = new GeometryModel3D(sphere, material);
                        fieldGroup.Children.Add(sphereModel);
                    }
                }
            }
            
            _gravitationalFieldVisual.Content = fieldGroup;
        }
        
        private void CreateSphere(MeshGeometry3D mesh, Point3D center, double radius, int resolution)
        {
            mesh.Positions.Clear();
            mesh.TriangleIndices.Clear();
            
            // Create a simple octahedron for low resolution
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(center.X, center.Y + radius, center.Z),
                new Point3D(center.X + radius, center.Y, center.Z),
                new Point3D(center.X, center.Y, center.Z + radius),
                new Point3D(center.X - radius, center.Y, center.Z),
                new Point3D(center.X, center.Y, center.Z - radius),
                new Point3D(center.X, center.Y - radius, center.Z)
            };
            
            foreach (var vertex in vertices)
            {
                mesh.Positions.Add(vertex);
            }
            
            // Top triangles
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(4);
            
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(1);
            
            // Bottom triangles
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(3);
            
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);
        }
        
        private Color GetCurvatureColor(double curvature)
        {
            // Map curvature to a color gradient (blue to red)
            double normalizedCurvature = Math.Min(1.0, curvature * 5.0);
            byte r = (byte)(normalizedCurvature * 255);
            byte g = 0;
            byte b = (byte)((1 - normalizedCurvature) * 255);
            
            return Color.FromRgb(r, g, b);
        }
        
        private void AddObjectVisual(MassiveObject obj)
        {
            // Create visual representation for the object
            var objVisual = new ModelVisual3D();
            var objGroup = new Model3DGroup();
            
            // Create sphere for the object
            var sphere = new MeshGeometry3D();
            double size = Math.Log10(obj.Mass) * 0.05; // Scale size based on mass
            size = Math.Max(0.2, Math.Min(2.0, size)); // Clamp size
            
            CreateSphere(sphere, obj.Position.ToPoint3D(), size, 8);
            
            // Create material
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
            
            var sphereModel = new GeometryModel3D(sphere, material);
            objGroup.Children.Add(sphereModel);
            
            objVisual.Content = objGroup;
            
            // Add to viewport and dictionary
            mainViewport.Children.Add(objVisual);
            _objectVisuals[obj] = objVisual;
        }
        
        private void AddGeodesicVisual(List<SpacetimePoint> path)
        {
            // Create visual representation for the geodesic path
            var pathVisual = new ModelVisual3D();
            var pathGroup = new Model3DGroup();
            
            // Create line for the path
            var positions = new Point3DCollection();
            var indices = new Int32Collection();
            
            for (int i = 0; i < path.Count; i++)
            {
                positions.Add(path[i].ToPoint3D());
                if (i < path.Count - 1)
                {
                    indices.Add(i);
                    indices.Add(i + 1);
                }
            }
            
            // Create geometry
            var geometry = new MeshGeometry3D();
            geometry.Positions = positions;
            geometry.TriangleIndices = indices;
            
            // Create material
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
            
            var pathModel = new GeometryModel3D(geometry, material);
            pathGroup.Children.Add(pathModel);
            
            pathVisual.Content = pathGroup;
            
            // Add to viewport and dictionary
            mainViewport.Children.Add(pathVisual);
            _geodesicVisuals[path] = pathVisual;
        }
        
        private void UpdateObjectVisuals()
        {
            // Update visual positions for all objects
            foreach (var obj in _objectVisuals.Keys)
            {
                var visual = _objectVisuals[obj];
                var transform = new TranslateTransform3D(
                    obj.Position.X,
                    obj.Position.Y,
                    obj.Position.Z
                );
                
                // Apply transform to visual
                if (visual.Content is Model3DGroup group && group.Children.Count > 0)
                {
                    if (group.Children[0] is GeometryModel3D model)
                    {
                        model.Transform = transform;
                    }
                }
            }
        }
        
        private void UpdateUI()
        {
            // Update object count
            txtObjectCount.Text = $"Objects: {_objectVisuals.Count}";
            
            // Update simulation time
            txtCurrentTime.Text = $"Simulation Time: {_simulationTime:F2}";
            
            // Update time scale
            txtTimeScaleValue.Text = $"{_timeScale:F1}";
        }
        
        private async void RunQuantumSimulation()
        {
            try
            {
                // Update status
                txtSimulationInfo.Text = "Running quantum simulation...";
                
                // Run quantum simulation
                double effectiveG = await _quantumBridge.RunQuantumSimulationAsync();
                
                // Update the gravitational constant in the model
                GeometryModel.G = effectiveG;
                
                // Update status
                txtSimulationInfo.Text = $"Quantum simulation complete. Effective G: {effectiveG:E10}";
                
                // Recalculate metrics with new G
                _geometryModel.CalculateMetrics();
                
                // Update visualizations
                UpdateGravitationalFieldVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running quantum simulation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #region Event Handlers
        
        private void OnAddObjectClick(object sender, RoutedEventArgs e)
        {
            // Show object properties dialog
            objectPropertiesDialog.Visibility = Visibility.Visible;
        }
        
        private void OnCancelObjectClick(object sender, RoutedEventArgs e)
        {
            // Hide object properties dialog
            objectPropertiesDialog.Visibility = Visibility.Collapsed;
        }
        
        private void OnAddObjectConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse input values
                string name = txtObjectName.Text;
                double mass = double.Parse(txtObjectMass.Text);
                double posX = double.Parse(txtObjectPosX.Text);
                double posY = double.Parse(txtObjectPosY.Text);
                double posZ = double.Parse(txtObjectPosZ.Text);
                double velX = double.Parse(txtObjectVelX.Text);
                double velY = double.Parse(txtObjectVelY.Text);
                double velZ = double.Parse(txtObjectVelZ.Text);
                
                // Create object
                var position = new SpacetimePoint(posX, posY, posZ, _simulationTime);
                var velocity = new Vector3D(velX, velY, velZ);
                var obj = new MassiveObject(name, mass, position, velocity);
                
                // Add to model
                _geometryModel.AddObject(obj);
                
                // Add visual
                AddObjectVisual(obj);
                
                // Update gravitational field
                UpdateGravitationalFieldVisual();
                
                // Update UI
                UpdateUI();
                
                // Hide dialog
                objectPropertiesDialog.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding object: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OnCalculateGeodesicClick(object sender, RoutedEventArgs e)
        {
            if (_objectVisuals.Count == 0)
            {
                MessageBox.Show("Add at least one object first.", "No Objects", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                // Create a test particle at origin
                var start = new SpacetimePoint(0, 0, 0, _simulationTime);
                var initialVelocity = new Vector3D(0.1, 0, 0); // Small initial velocity
                
                // Calculate geodesic
                var path = _geometryModel.CalculateGeodesic(start, initialVelocity, 100.0, 1000);
                
                // Add visual
                AddGeodesicVisual(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating geodesic: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            // Stop simulation
            _simulationTimer.Stop();
            _isSimulationRunning = false;
            btnStartSimulation.Content = "Start Simulation";
            btnPauseSimulation.IsEnabled = false;
            
            // Reset time
            _simulationTime = 0.0;
            
            // Clear objects
            foreach (var visual in _objectVisuals.Values)
            {
                mainViewport.Children.Remove(visual);
            }
            _objectVisuals.Clear();
            
            // Clear geodesics
            foreach (var visual in _geodesicVisuals.Values)
            {
                mainViewport.Children.Remove(visual);
            }
            _geodesicVisuals.Clear();
            
            // Reset model
            _geometryModel = new GeometryModel();
            _geometryModel.InitializeGrid(10, 20.0);
            
            // Reset visualization
            UpdateGravitationalFieldVisual();
            
            // Update UI
            UpdateUI();
        }
        
        private void OnVisualizationModeChanged(object sender, SelectionChangedEventArgs e)
        {
            int mode = cmbVisualizationMode.SelectedIndex;
            
            // Update visibility based on mode
            _spacetimeGridVisual.Visibility = (mode == 0 || mode == 2) ? Visibility.Visible : Visibility.Collapsed;
            _gravitationalFieldVisual.Visibility = (mode == 1) ? Visibility.Visible : Visibility.Collapsed;
            
            // Show/hide geodesics
            foreach (var visual in _geodesicVisuals.Values)
            {
                visual.Visibility = (mode == 2) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        private void OnTimeScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _timeScale = sldTimeScale.Value;
            txtTimeScaleValue.Text = $"{_timeScale:F1}";
        }
        
        private void OnStartSimulationClick(object sender, RoutedEventArgs e)
        {
            if (_isSimulationRunning)
            {
                // Stop simulation
                _simulationTimer.Stop();
                _isSimulationRunning = false;
                btnStartSimulation.Content = "Start Simulation";
                btnPauseSimulation.IsEnabled = false;
            }
            else
            {
                // Start simulation
                _simulationTimer.Start();
                _isSimulationRunning = true;
                btnStartSimulation.Content = "Stop Simulation";
                btnPauseSimulation.IsEnabled = true;
            }
        }
        
        private void OnPauseSimulationClick(object sender, RoutedEventArgs e)
        {
            if (_simulationTimer.IsEnabled)
            {
                _simulationTimer.Stop();
                btnPauseSimulation.Content = "Resume";
            }
            else
            {
                _simulationTimer.Start();
                btnPauseSimulation.Content = "Pause";
            }
        }
        
        private void OnStepSimulationClick(object sender, RoutedEventArgs e)
        {
            // Perform a single simulation step
            UpdateSimulation(0.1);
        }
        
        private void OnRunQuantumSimulationClick(object sender, RoutedEventArgs e)
        {
            RunQuantumSimulation();
        }
        
        private void OnSimulationTick(object sender, EventArgs e)
        {
            // Update simulation
            UpdateSimulation(0.016 * _timeScale); // 16ms * time scale
        }
        
        private void UpdateSimulation(double deltaTime)
        {
            // Update simulation time
            _simulationTime += deltaTime;
            
            // Update object positions (simplified)
            foreach (var obj in _objectVisuals.Keys)
            {
                // Update position based on velocity
                obj.Position.X += obj.Velocity.X * deltaTime;
                obj.Position.Y += obj.Velocity.Y * deltaTime;
                obj.Position.Z += obj.Velocity.Z * deltaTime;
                obj.Position.T = _simulationTime;
                
                // Update velocity based on gravitational forces
                foreach (var otherObj in _objectVisuals.Keys)
                {
                    if (obj != otherObj)
                    {
                        // Calculate gravitational force
                        double dx = otherObj.Position.X - obj.Position.X;
                        double dy = otherObj.Position.Y - obj.Position.Y;
                        double dz = otherObj.Position.Z - obj.Position.Z;
                        double r = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        
                        if (r > 0)
                        {
                            double force = GeometryModel.G * otherObj.Mass / (r * r);
                            obj.Velocity.X += force * dx / r * deltaTime;
                            obj.Velocity.Y += force * dy / r * deltaTime;
                            obj.Velocity.Z += force * dz / r * deltaTime;
                        }
                    }
                }
            }
            
            // Recalculate metrics
            _geometryModel.CalculateMetrics();
            
            // Update visualizations
            UpdateObjectVisuals();
            UpdateGravitationalFieldVisual();
            
            // Update UI
            UpdateUI();
        }
        
        #endregion
        
        #region Camera Control
        
        private void OnViewportMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(mainViewport);
            _isRotating = true;
            mainViewport.CaptureMouse();
        }
        
        private void OnViewportMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isRotating = false;
            mainViewport.ReleaseMouseCapture();
        }
        
        private void OnViewportMouseMove(object sender, MouseEventArgs e)
        {
            if (_isRotating)
            {
                Point currentPosition = e.GetPosition(mainViewport);
                Vector delta = currentPosition - _lastMousePosition;
                
                // Rotate camera
                RotateCamera(delta.X, delta.Y);
                
                _lastMousePosition = currentPosition;
            }
        }
        
        private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom camera
            _cameraDistance -= e.Delta * 0.001;
            _cameraDistance = Math.Max(1.0, Math.Min(20.0, _cameraDistance));
            
            UpdateCameraPosition();
        }
        
        private void RotateCamera(double deltaX, double deltaY)
        {
            // Get current camera position
            Vector3D lookDirection = camera.LookDirection;
            Vector3D upDirection = camera.UpDirection;
            
            // Create rotation quaternions
            double angleX = deltaX * 0.01;
            double angleY = deltaY * 0.01;
            
            // Rotate around Y axis (horizontal)
            Quaternion rotationY = new Quaternion(new Vector3D(0, 1, 0), angleX * 180 / Math.PI);
            lookDirection = rotationY.Transform(lookDirection);
            upDirection = rotationY.Transform(upDirection);
            
            // Rotate around X axis (vertical)
            Vector3D rightDirection = Vector3D.CrossProduct(lookDirection, upDirection);
            rightDirection.Normalize();
            Quaternion rotationX = new Quaternion(rightDirection, angleY * 180 / Math.PI);
            lookDirection = rotationX.Transform(lookDirection);
            upDirection = rotationX.Transform(upDirection);
            
            // Update camera
            camera.LookDirection = lookDirection;
            camera.UpDirection = upDirection;
            
            UpdateCameraPosition();
        }
        
        private void UpdateCameraPosition()
        {
            // Update camera position based on look direction and distance
            Vector3D normalizedLookDirection = camera.LookDirection;
            normalizedLookDirection.Normalize();
            
            camera.Position = new Point3D(
                -normalizedLookDirection.X * _cameraDistance,
                -normalizedLookDirection.Y * _cameraDistance,
                -normalizedLookDirection.Z * _cameraDistance
            );
        }
        
        #endregion
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Dispose quantum simulator
            _quantumBridge?.Dispose();
        }
    }
}