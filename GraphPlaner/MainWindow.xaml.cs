using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphPlaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            var vertexAccord = new Dictionary<GraphVertex, VisualGraphVertex>();
            Graph graph = new Graph();
            graph.Vertexes = new List<GraphVertex>()
                            {
                                new GraphVertex(),
                                new GraphVertex(),
                                new GraphVertex(),
                                new GraphVertex(),
                                new GraphVertex(),
                                new GraphVertex(),
                            };

            graph.Edges = new List<GraphEdge>()
                          {
                              new GraphEdge(graph.Vertexes[0],graph.Vertexes[1]),
                              new GraphEdge(graph.Vertexes[0],graph.Vertexes[2]),
                              new GraphEdge(graph.Vertexes[0],graph.Vertexes[3]),
                              new GraphEdge(graph.Vertexes[0],graph.Vertexes[4]),
                          };
            Random rnd = new Random();

            foreach (var graphVertex in graph.Vertexes)
            {
                VisualGraphVertex vertex = new VisualGraphVertex(DrawEllipse(), graphVertex);
                vertexAccord.Add(graphVertex, vertex);
                vertex.Locate(rnd.Next(200), rnd.Next(200));
            }
            Physic physic = new Physic(graph, vertexAccord);

            var framesCount = 24;
            var scrrenUpdateFreq = TimeSpan.FromMilliseconds(10);
            var stepPeriod = TimeSpan.FromSeconds(1);




            Timer timer = new Timer((a) => Dispatcher.BeginInvoke(new Action(() => physic.NextStep(TimeSpan.FromMilliseconds(stepPeriod.TotalMilliseconds / (TimeSpan.FromSeconds(1).TotalMilliseconds / scrrenUpdateFreq.TotalMilliseconds))))));



            timer.Change(TimeSpan.FromMinutes(0), scrrenUpdateFreq);

        }

        public Ellipse DrawEllipse()
        {
            var ellipse = new Ellipse();
            ellipse.Width = 100;
            ellipse.Height = 100;
            ellipse.Fill = new SolidColorBrush(Colors.Blue);
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.StrokeThickness = 1;
            Canvas.SetTop(ellipse, 100);
            Canvas.SetLeft(ellipse, 100);
            Canvas.Children.Add(ellipse);
            return ellipse;
        }

    }

    public class Physic
    {
        private Graph _graph;
        public Physic(Graph graph, Dictionary<GraphVertex, VisualGraphVertex> vertexAccord)
        {
            _graph = graph;
            _vertexAccord = vertexAccord;
            _physicAccord = new Dictionary<GraphVertex, PhysicVertex>();

            foreach (var graphVertex in graph.Vertexes)
            {
                _physicAccord.Add(graphVertex, new PhysicVertex()
                                              {
                                                  Mass = random.Next(20,50),
                                                  Acceleration = new Vector(0, 9.8)
                                              });
            }


        }
        Random random = new Random();
        private Dictionary<GraphVertex, VisualGraphVertex> _vertexAccord;
        private Dictionary<GraphVertex, PhysicVertex> _physicAccord;

        public void NextStep(TimeSpan span)
        {
            foreach (var vertex in _graph.Vertexes)
            {
                var physVert = _physicAccord[vertex];
                var visVert = _vertexAccord[vertex];

                if (visVert.Vertex.Y < 500)
                {
                    physVert.Velocity.Y = physVert.Velocity.Y + physVert.Acceleration.Y * span.TotalSeconds;
                }
                else
                {
                    physVert.Velocity.Y = -(physVert.Velocity.Y + physVert.Acceleration.Y * span.TotalSeconds);
                    physVert.Velocity.Y += physVert.Mass/10;
                    visVert.Vertex.Y = 500;
                }


                var newX = visVert.Vertex.X;
                var newY = visVert.Vertex.Y + physVert.Velocity.Y * 1 / 2;



                visVert.Locate(newX, newY);
            }

        }
    }

    public class PhysicVertex
    {
        public Double Mass;
        public Vector Velocity;
        public Vector Acceleration;
    }

    public class VisualGraphVertex
    {
        public Ellipse Visual
        {
            get;
            private set;
        }

        public VisualGraphVertex(Ellipse visual, GraphVertex vertex)
        {
            Visual = visual;
            Vertex = vertex;
        }

        public void Locate(Double x, Double y)
        {
            Canvas.SetTop(Visual, y);
            Canvas.SetLeft(Visual, x);

            Vertex.X = x;
            Vertex.Y = y;
        }



        public GraphVertex Vertex { get; private set; }
    }

    public class Graph
    {
        public List<GraphVertex> Vertexes { get; set; }
        public List<GraphEdge> Edges { get; set; }

    }

    public class GraphVertex
    {
        public Double X { get; set; }
        public Double Y { get; set; }
    }

    public class GraphEdge
    {
        public GraphEdge(GraphVertex from, GraphVertex to)
        {
            FromVertex = from;
            ToVertex = to;
        }
        public GraphVertex FromVertex
        {
            get;
            private set;
        }

        public GraphVertex ToVertex
        {
            get;
            private set;
        }
    }
}
