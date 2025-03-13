using System;
using System.Threading.Tasks;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using QuantumCompressionTheory;

namespace wpf_graphics_app.Models
{
    /// <summary>
    /// Bridge class that connects the Q# quantum simulation with the WPF application
    /// </summary>
    public class QuantumSimulationBridge
    {
        private QuantumSimulator _simulator;
        
        public QuantumSimulationBridge()
        {
            // Initialize quantum simulator
            _simulator = new QuantumSimulator();
        }
        
        /// <summary>
        /// Runs the quantum simulation and returns the effective gravitational constant
        /// </summary>
        /// <returns>The effective gravitational constant calculated by the quantum simulation</returns>
        public async Task<double> RunQuantumSimulationAsync()
        {
            try
            {
                // Run the quantum simulation
                double effectiveG = await RunQuantumCompressionSimulation.Run(_simulator);
                
                return effectiveG;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running quantum simulation: {ex.Message}");
                return GeometryModel.G; // Return default G value on error
            }
        }
        
        /// <summary>
        /// Calculates the effective gravitational constant based on entanglement density
        /// </summary>
        /// <param name="rhoEnt">Entanglement density</param>
        /// <returns>The effective gravitational constant</returns>
        public double CalculateEffectiveG(double rhoEnt)
        {
            // Constants
            double G0 = 6.67430e-11; // Gravitational constant in m^3 kg^-1 s^-2
            double kappa = 1.0e-38;  // Approximate coupling constant
            
            // Calculate effective G using the formula from the Q# code
            double constant = 8.0 * Math.PI * G0 * kappa * rhoEnt;
            double Geff = G0 / (1.0 + constant);
            
            return Geff;
        }
        
        /// <summary>
        /// Disposes the quantum simulator
        /// </summary>
        public void Dispose()
        {
            _simulator?.Dispose();
        }
    }
}