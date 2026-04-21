# Test task: Geometry Hit-Testing (2011)
The task here was to create a WPF application which implements custom hit-testing (that is, **not** using WPF's hit-testing) for various closed geomeries loaded from provided XML files. The hit testing needed to be visualized the following way: 
- when mouse click is made within the poligon, fill it with green if the polygon is convex, with red otherwise
- do nothing if mouse clicks outside the polygon

Additionally, for non-convex polygon also generate and render the minimal convex polygon including all its vertices.

The **GeometryUtils** class is where hit-testing and other geometric functions are implemented.

![Non-convex polygon demo](Demo%201.gif)

![Convex polygon demo](Demo%202.gif)

