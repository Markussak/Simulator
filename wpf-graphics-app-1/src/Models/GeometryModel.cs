using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace wpf_graphics_app.Models
{
    /// <summary>
    /// Represents a point in 4D spacetime (x, y, z, t)
    /// </summary>
    public class SpacetimePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double T { get; set; } // Time component

        public SpacetimePoint(double x, double y, double z, double t)
        {
            X = x;
            Y = y;
            Z = z;
            T = t;
        }

        // Convert to 3D point for visualization (projecting 4D to 3D)
        public Point3D ToPoint3D()
        {
            return new Point3D(X, Y, Z);
        }
    }

    /// <summary>
    /// Represents a massive object that causes spacetime curvature
    /// </summary>
    public class MassiveObject
    {
        public string Name { get; set; }
        public double Mass { get; set; } // in kg
        public SpacetimePoint Position { get; set; }
        public Vector3D Velocity { get; set; }

        public MassiveObject(string name, double mass, SpacetimePoint position, Vector3D velocity)
        {
            Name = name;
            Mass = mass;
            Position = position;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Represents the metric tensor at a point in spacetime
    /// </summary>
    public class MetricTensor
    {
        // 4x4 matrix representing the metric tensor g_μν
        private double[,] _components;

        public MetricTensor()
        {
            // Initialize with Minkowski metric (flat spacetime)
            _components = new double[4, 4]
            {
                { -1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };
        }

        public double this[int μ, int ν]
        {
            get { return _components[μ, ν]; }
            set { _components[μ, ν] = value; }
        }

        // Calculate the Schwarzschild metric for a point at distance r from a mass M
        public void CalculateSchwarzschildMetric(double r, double M)
        {
            double G = 6.67430e-11; // Gravitational constant
            double c = 299792458;   // Speed of light
            double rs = 2 * G * M / (c * c); // Schwarzschild radius

            // Avoid singularity
            if (r <= rs) r = rs * 1.01;

            // Set metric components
            _components[0, 0] = -(1 - rs / r);
            _components[1, 1] = 1 / (1 - rs / r);
            _components[2, 2] = r * r;
            _components[3, 3] = r * r * Math.Sin(Math.PI / 2) * Math.Sin(Math.PI / 2); // Simplified for equatorial plane
        }
    }

    /// <summary>
    /// Main model class for spacetime geometry simulation
    /// </summary>
    public class GeometryModel
    {
        public List<MassiveObject> Objects { get; private set; }
        public List<SpacetimePoint> GridPoints { get; private set; }
        public Dictionary<SpacetimePoint, MetricTensor> SpacetimeMetric { get; private set; }
        
        // Physical constants
        public const double G = 6.67430e-11; // Gravitational constant
        public const double c = 299792458;   // Speed of light

        public GeometryModel()
        {
            Objects = new List<MassiveObject>();
            GridPoints = new List<SpacetimePoint>();
            SpacetimeMetric = new Dictionary<SpacetimePoint, MetricTensor>();
        }

        public void AddObject(MassiveObject obj)
        {
            Objects.Add(obj);
            // Recalculate metrics when a new object is added
            CalculateMetrics();
        }

        public void InitializeGrid(int resolution, double size)
        {
            GridPoints.Clear();
            SpacetimeMetric.Clear();

            // Create a 3D grid of points (with t=0 initially)
            double step = size / resolution;
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    for (int k = 0; k < resolution; k++)
                    {
                        double x = (i - resolution / 2) * step;
                        double y = (j - resolution / 2) * step;
                        double z = (k - resolution / 2) * step;
                        
                        var point = new SpacetimePoint(x, y, z, 0);
                        GridPoints.Add(point);
                        SpacetimeMetric[point] = new MetricTensor();
                    }
                }
            }
        }

        public void CalculateMetrics()
        {
            foreach (var point in GridPoints)
            {
                var metric = new MetricTensor();
                
                foreach (var obj in Objects)
                {
                    // Calculate distance from point to object
                    double dx = point.X - obj.Position.X;
                    double dy = point.Y - obj.Position.Y;
                    double dz = point.Z - obj.Position.Z;
                    double r = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    
                    // Calculate Schwarzschild metric for this object
                    metric.CalculateSchwarzschildMetric(r, obj.Mass);
                }
                
                SpacetimeMetric[point] = metric;
            }
        }

        // Calculate geodesic path for a test particle
        public List<SpacetimePoint> CalculateGeodesic(SpacetimePoint start, Vector3D initialVelocity, double properTime, int steps)
        {
            var path = new List<SpacetimePoint> { start };
            var currentPoint = start;
            var velocity = initialVelocity;
            double dt = properTime / steps;

            for (int i = 0; i < steps; i++)
            {
                // Simple Euler integration (could be improved with Runge-Kutta)
                double ax = 0, ay = 0, az = 0;
                
                foreach (var obj in Objects)
                {
                    // Calculate gravitational acceleration using Newton's law (simplified)
                    double dx = obj.Position.X - currentPoint.X;
                    double dy = obj.Position.Y - currentPoint.Y;
                    double dz = obj.Position.Z - currentPoint.Z;
                    double r = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    
                    if (r > 0)
                    {
                        double a = G * obj.Mass / (r * r);
                        ax += a * dx / r;
                        ay += a * dy / r;
                        az += a * dz / r;
                    }
                }
                
                // Update velocity
                velocity.X += ax * dt;
                velocity.Y += ay * dt;
                velocity.Z += az * dt;
                
                // Update position
                var nextPoint = new SpacetimePoint(
                    currentPoint.X + velocity.X * dt,
                    currentPoint.Y + velocity.Y * dt,
                    currentPoint.Z + velocity.Z * dt,
                    currentPoint.T + dt
                );
                
                path.Add(nextPoint);
                currentPoint = nextPoint;
            }
            
            return path;
        }
    }
}