using System;

namespace wpf_graphics_app.Models
{
    public class GeometryModel
    {
        public string ShapeName { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }

        public GeometryModel(string shapeName, double area, double perimeter)
        {
            ShapeName = shapeName;
            Area = area;
            Perimeter = perimeter;
        }

        public override string ToString()
        {
            return $"{ShapeName}: Area = {Area}, Perimeter = {Perimeter}";
        }
    }
}