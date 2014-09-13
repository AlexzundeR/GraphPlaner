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
        private Random rnd;
        private VisualGraphVertex _lockedVertex;

        public MainWindow()
        {
            InitializeComponent();
            rnd = new Random();
            var vertexAccord = new Dictionary<GraphVertex, VisualGraphVertex>();
            var edgeAccord = new Dictionary<GraphEdge, VisualGraphEdge>();

            Graph graph = new Graph();
            graph.Vertexes = new List<GraphVertex>()
                            {
                                new GraphVertex(),
                                new GraphVertex(),
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
                              new GraphEdge(graph.Vertexes[1],graph.Vertexes[2]),
                              new GraphEdge(graph.Vertexes[2],graph.Vertexes[3]),
                              new GraphEdge(graph.Vertexes[3],graph.Vertexes[4]),
                              new GraphEdge(graph.Vertexes[4],graph.Vertexes[5]),
                              new GraphEdge(graph.Vertexes[5],graph.Vertexes[0]),
                              new GraphEdge(graph.Vertexes[5],graph.Vertexes[6]),
                              new GraphEdge(graph.Vertexes[3],graph.Vertexes[6]),
                              new GraphEdge(graph.Vertexes[1],graph.Vertexes[6]),
                                new GraphEdge(graph.Vertexes[7],graph.Vertexes[0]),
                                new GraphEdge(graph.Vertexes[7],graph.Vertexes[2]),
                                new GraphEdge(graph.Vertexes[7],graph.Vertexes[4]),
                          };

            foreach (var graphVertex in graph.Vertexes)
            {
                VisualGraphVertex vertex = new VisualGraphVertex(graphVertex);
                vertexAccord.Add(graphVertex, vertex);
                vertex.Locate(rnd.Next(400), rnd.Next(400));
                Canvas.Children.Add(vertex.Visual);
                vertex.Locked += vertex_Locked;
                vertex.Unlocked += vertex_Unlocked;
                Canvas.SetZIndex(vertex.Visual, 100);
            }
            foreach (var graphEdge in graph.Edges)
            {
                VisualGraphEdge edge = new VisualGraphEdge(vertexAccord[graph.Vertexes.First(e => graphEdge.FromVertex == e)], vertexAccord[graph.Vertexes.First(e => graphEdge.ToVertex == e)], graphEdge);
                edgeAccord.Add(graphEdge, edge);
                Canvas.Children.Add(edge.Visual);

                Canvas.SetZIndex(edge.Visual,10);
            }
            Physic physic = new Physic(graph, vertexAccord, edgeAccord);

            var framesCount = 24;
            var scrrenUpdateFreq = TimeSpan.FromMilliseconds(40);
            var stepPeriod = TimeSpan.FromSeconds(10);


            Canvas.MouseMove += Canvas_MouseMove;

            Timer timer = new Timer((a) => Dispatcher.BeginInvoke(new Action(() => physic.NextStep(TimeSpan.FromMilliseconds(stepPeriod.TotalMilliseconds / (TimeSpan.FromSeconds(1).TotalMilliseconds / scrrenUpdateFreq.TotalMilliseconds))))));



            timer.Change(TimeSpan.FromMinutes(0), scrrenUpdateFreq);

        }

        void vertex_Unlocked(VisualGraphVertex obj)
        {
            _lockedVertex = null;
            Mouse.PrimaryDevice.Target.ReleaseMouseCapture();
        }

        void vertex_Locked(VisualGraphVertex sender)
        {
            _lockedVertex = sender;
            Mouse.Capture(_lockedVertex.Visual);
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lockedVertex != null)
            {
                _lockedVertex.Locate(e.GetPosition(Canvas));
            }
        }


    }

    public class Physic
    {
        private Graph _graph;
        public Physic(Graph graph, Dictionary<GraphVertex, VisualGraphVertex> vertexAccord, Dictionary<GraphEdge, VisualGraphEdge> edgeAccord)
        {
            _graph = graph;
            _vertexAccord = vertexAccord;
            _physicAccord = new Dictionary<GraphVertex, PhysicVertex>();
            _edgeAccord = edgeAccord;

            foreach (var graphVertex in graph.Vertexes)
            {
                _physicAccord.Add(graphVertex, new PhysicVertex()
                                              {
                                                  Mass = random.Next(20, 50),
                                              });
            }
        }
        Random random = new Random();
        private Dictionary<GraphVertex, VisualGraphVertex> _vertexAccord;
        private Dictionary<GraphVertex, PhysicVertex> _physicAccord;
        private Dictionary<GraphEdge, VisualGraphEdge> _edgeAccord;

        public void NextStep(TimeSpan span)
        {
            foreach (var vertex in _graph.Vertexes)
            {
                var physVert = _physicAccord[vertex];
                var visVert = _vertexAccord[vertex];
                physVert.NetPower.X = 0;
                physVert.NetPower.Y = 0;

                foreach (var graphVertex in _graph.Vertexes)
                {
                    if (graphVertex == vertex) continue;
                    var visSecVErtex = _vertexAccord[graphVertex];

                    var rsq = Math.Pow(visVert.X - visSecVErtex.X, 2) + Math.Pow(visVert.Y - visSecVErtex.Y, 2);
                    physVert.NetPower.X += 150 * (visVert.X - visSecVErtex.X) / rsq;
                    physVert.NetPower.Y += 150 * (visVert.Y - visSecVErtex.Y) / rsq;
                }

                var neighbors = _graph.Edges.Where(e => e.FromVertex == vertex).Select(e => e.ToVertex)
                    .Union(_graph.Edges.Where(e => e.ToVertex == vertex).Select(e => e.FromVertex)).Distinct().ToList();

                foreach (var graphVertex in neighbors)
                {
                    var visSecVErtex = _vertexAccord[graphVertex];
                    physVert.NetPower.X += 0.08 * (visSecVErtex.X - visVert.X);
                    physVert.NetPower.Y += 0.08 * (visSecVErtex.Y - visVert.Y);
                }

                physVert.Velocity.X = (physVert.Velocity.X + physVert.NetPower.X) * 0.85;
                physVert.Velocity.Y = (physVert.Velocity.Y + physVert.NetPower.Y) * 0.85;

                var newX = visVert.X + physVert.Velocity.X;
                var newY = visVert.Y + physVert.Velocity.Y;

                visVert.Locate(newX, newY);
            }

        }
    }

    public class PhysicVertex
    {
        public Double Mass;
        public Vector Velocity;
        public Vector NetPower;
    }

    public class VisualGraphVertex
    {
        private static Random _rnd = new Random();
        public Ellipse Visual
        {
            get;
            private set;
        }

        public VisualGraphVertex(GraphVertex vertex)
        {
            Width = 20;
            Height = 20;
            Visual = DrawEllipse(Width, Height);

            Vertex = vertex;
        }

        public Ellipse DrawEllipse(Double width, Double height)
        {
            var ellipse = new Ellipse();
            ellipse.Width = width;
            ellipse.Height = height;
            var color = Color.FromRgb(0, 0, (byte)_rnd.Next(255));
            ellipse.Fill = new SolidColorBrush(color);
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.StrokeThickness = 1;
            ellipse.MouseDown += ellipse_MouseDown;
            ellipse.MouseUp += ellipse_MouseUp;
            return ellipse;
        }

        public event Action<VisualGraphVertex> Locked;

        protected virtual void OnLocked(VisualGraphVertex obj)
        {
            Action<VisualGraphVertex> handler = Locked;
            if (handler != null) handler(obj);
        }

        public event Action<VisualGraphVertex> Unlocked;

        protected virtual void OnUnLocked(VisualGraphVertex obj)
        {
            Action<VisualGraphVertex> handler = Unlocked;
            if (handler != null) handler(obj);
        }

        public event Action<VisualGraphVertex> Relocated;

        protected virtual void OnRelocated(VisualGraphVertex obj)
        {
            Action<VisualGraphVertex> handler = Relocated;
            if (handler != null) handler(obj);
        }

        void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UnLock();
        }

        void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Lock();

        }

        public Boolean IsLocked { get; private set; }

        public void Lock()
        {
            IsLocked = true;
            OnLocked(this);
        }

        public void UnLock()
        {
            IsLocked = false;
            Unlocked(this);
        }


        public void Locate(Double x, Double y)
        {
            Canvas.SetTop(Visual, y - Height / 2);
            Canvas.SetLeft(Visual, x - Width / 2);

            X = x;
            Y = y;
            OnRelocated(this);
        }

        public Double X { get; private set; }
        public Double Y { get; private set; }

        public Double Width { get; private set; }
        public Double Height { get; private set; }

        public GraphVertex Vertex { get; private set; }

        public void Locate(Point getPosition)
        {
            Locate(getPosition.X, getPosition.Y);
        }
    }

    public class Graph
    {
        public List<GraphVertex> Vertexes { get; set; }
        public List<GraphEdge> Edges { get; set; }

    }

    public class GraphVertex
    {

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

    public class VisualGraphEdge
    {
        private readonly VisualGraphVertex _fromVertex;
        private readonly VisualGraphVertex _toVertex;
        public Line Visual { get; private set; }
        public VisualGraphEdge(VisualGraphVertex fromVertex, VisualGraphVertex toVertex, GraphEdge edge)
        {
            _fromVertex = fromVertex;
            _toVertex = toVertex;
            Visual = new Line();
            Visual.Stroke = new SolidColorBrush(Colors.Black);
            Visual.StrokeThickness = 1;
            
            DrawLine();

            fromVertex.Relocated += FromVertexOnRelocated;
            toVertex.Relocated += toVertex_Relocated;
        }

        private void DrawLine()
        {
            DrawFromPoint();
            DrawToPoint();
        }

        private void DrawFromPoint()
        {
            Visual.X1 = _fromVertex.X;
            Visual.Y1 = _fromVertex.Y;
        }

        private void DrawToPoint()
        {
            Visual.X2 = _toVertex.X;
            Visual.Y2 = _toVertex.Y;
        }

        void toVertex_Relocated(VisualGraphVertex obj)
        {
            DrawToPoint();
        }

        private void FromVertexOnRelocated(VisualGraphVertex visualGraphVertex)
        {
            DrawFromPoint();
        }
    }
}
