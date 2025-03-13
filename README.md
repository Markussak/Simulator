# Spacetime Geometry and Gravitational Interactions Simulator

## Overview

This project is a sophisticated numerical simulation that realistically models spacetime curvature according to general relativity, visualizing gravitational interactions between massive objects including their dynamic movement along geodesics. The simulation represents spacetime geometry using a four-dimensional metric grid that reflects Riemannian geometry and curvature defined by the Einstein tensor.

The application consists of two main components:
1. A quantum simulation (Q#) that models quantum compression theory and its relation to gravity
2. A visualization application (WPF) that provides an interactive 3D representation of spacetime geometry

## Features

- **Spacetime Geometry Visualization**: View the curvature of spacetime caused by massive objects
- **Gravitational Field Visualization**: See how mass affects the surrounding spacetime
- **Geodesic Path Calculation**: Calculate and visualize the path of test particles in curved spacetime
- **Dynamic Simulation**: Watch objects move under the influence of gravity
- **Quantum Gravity Integration**: Run quantum simulations that affect the effective gravitational constant
- **Interactive Camera Control**: Rotate and zoom to explore the 3D visualization

## Components

### Quantum Simulation (Q#)

The quantum component uses Q# to simulate quantum compression theory, which models how quantum entanglement might affect the effective gravitational constant. The simulation:

- Creates quantum states with specific quantum numbers
- Calculates entanglement density
- Simulates the effective gravitational constant based on entanglement density

### Visualization Application (WPF)

The WPF application provides a 3D visualization of spacetime geometry and gravitational interactions. It includes:

- A 3D viewport for visualizing spacetime geometry
- Controls for adding massive objects with custom properties
- Simulation controls (start, pause, step)
- Multiple visualization modes (spacetime grid, gravitational field, geodesic paths)
- Camera controls for exploring the 3D space

## How to Use

1. **Add Objects**: Click the "Add Object" button to add massive objects to the simulation. You can specify the object's name, mass, position, and velocity.

2. **Run Simulation**: Use the "Start Simulation" button to start the dynamic simulation. You can pause, resume, or step through the simulation.

3. **Calculate Geodesics**: Click the "Calculate Geodesic" button to calculate and visualize the path of a test particle in the curved spacetime.

4. **Run Quantum Simulation**: Click the "Run Quantum Simulation" button to run the quantum simulation and update the effective gravitational constant.

5. **Change Visualization Mode**: Use the dropdown menu to switch between different visualization modes:
   - Spacetime Grid: Shows the basic grid structure of spacetime
   - Gravitational Field: Visualizes the curvature of spacetime caused by massive objects
   - Geodesic Paths: Shows both the spacetime grid and calculated geodesic paths

6. **Camera Control**: Use the mouse to control the camera:
   - Left-click and drag to rotate the view
   - Mouse wheel to zoom in and out

## Future Development

The long-term goal is to extend the simulation to include:
- Multi-scalar analysis of gravitational interactions from atomic to cosmological scales
- Integration of quantum field theory and the standard model of particles
- More accurate numerical methods for solving Einstein's field equations
- Advanced visualization techniques for higher-dimensional spacetime

## Technical Details

The project uses:
- Q# for quantum simulation
- C# and WPF for the visualization application
- 3D graphics for spacetime visualization
- Numerical methods for solving differential equations

The simulation implements simplified versions of:
- Einstein's field equations
- Schwarzschild metric for spherically symmetric spacetime
- Geodesic equations for particle motion
- Quantum entanglement effects on gravity