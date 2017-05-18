# PlotEquation
Some snippets of code for a plugin in Rhino that can plot equations

I've successfully created a plugin that can graph explicit 2D, 3D, and 4D equations for the CAD software Rhinoceros3D. I've extended the features of my program to include stereographic projections, mandelbrot outputs, and 2D L-Systems. I'm currently working on implementing a 3D L-System. To do so, I need to make a 3D turtle with relative rotations such as yaw, pitch, and roll.

I have had issues with rotating a 3D turtle, and haven't found a lot of concrete pseudocode on how to program one. Vectors Quaternions have to be used to create it, and because I couldn't find Quaternion or Vector structs that have all the feature I need I created my own. My issues are with using Quaternions and Vectors to rotate and move my 3D turtle correctly.

For my mandelbrot features, I've also created my own Complex number class. It works fine, but it needs to be optimized as best as it can - especially with raising a complex number to a non-integer power or applying trigonometry to it. It will speed up my fractal generating immensely.
