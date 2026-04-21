using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GeometryHitTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<Edge> edgesList;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            edgesList = SerializationUtils.LoadPolygon(@"TestGeometries\polyC.xml").ToList();

            polygonShape.Points = new PointCollection(
                GeometryUtils.GetPolygonPoints(edgesList));
            convexPolygonShape.Points = new PointCollection(
                GeometryUtils.GetPolygonPoints(GeometryUtils.ToConvex(edgesList)));
        }

        private void Viewbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(relativeTo: polygonShape);

            if (GeometryUtils.IsPointInPolygon(edgesList, mousePosition))
            {
                polygonShape.Fill = 
                    GeometryUtils.IsPolygonConvex(edgesList) ? 
                    Brushes.Green : 
                    Brushes.Red;
            }
        }

        private void viewbox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            polygonShape.Fill = null;
        }
    }
}
