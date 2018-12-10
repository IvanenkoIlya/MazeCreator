using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Maze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Maze2 maze = new Maze2(50,50, MazeCanvas);
        }

        private void GenerateMaze(object sender, RoutedEventArgs e)
        {
            MazeCanvas.Children.Clear();

            Maze1 maze = new Maze1((int)WidthSlider.Value,(int)HeightSlider.Value);

            double renderScale = 1000 / Math.Max(WidthSlider.Value, HeightSlider.Value);

            for(int i = 0; i < maze.cells.GetLength(0); i++)
            {
                for(int j = 0; j < maze.cells.GetLength(1); j++)
                {
                    Cell cell = maze.cells[i, j];
                    if((cell.Walls & Walls.North) == Walls.North)
                    {
                        Line line = new Line();
                        line.Stroke = Brushes.Black;
                        line.StrokeThickness = 2;

                        line.X1 = j * renderScale;
                        line.Y1 = i * renderScale;
                        line.X2 = (j + 1) * renderScale;
                        line.Y2 = line.Y1;

                        MazeCanvas.Children.Add(line);
                    }
                    if((cell.Walls & Walls.West) == Walls.West)
                    {
                        Line line = new Line();
                        line.Stroke = Brushes.Black;
                        line.StrokeThickness = 2;

                        line.X1 = j * renderScale;
                        line.Y1 = i * renderScale;
                        line.X2 = line.X1;
                        line.Y2 = (i+1) * renderScale;

                        MazeCanvas.Children.Add(line);
                    }
                }
            }

            Line eastLine = new Line();
            eastLine.Stroke = Brushes.Black;
            eastLine.StrokeThickness = 2;

            eastLine.X1 = (WidthSlider.Value) * renderScale;
            eastLine.Y1 = 0;
            eastLine.X2 = eastLine.X1;
            eastLine.Y2 = (HeightSlider.Value) * renderScale;

            Line southLine = new Line();
            southLine.Stroke = Brushes.Black;
            southLine.StrokeThickness = 2;

            southLine.X1 = 0;
            southLine.Y1 = (HeightSlider.Value) * renderScale;
            southLine.X2 = (WidthSlider.Value) * renderScale;
            southLine.Y2 = southLine.Y1;

            MazeCanvas.Children.Add(eastLine);
            MazeCanvas.Children.Add(southLine);

            Rectangle startRect = new Rectangle();
            startRect.Fill = Brushes.Green;
            startRect.Opacity = 0.5;
            startRect.Width = startRect.Height = renderScale;

            Canvas.SetTop(startRect, maze.startPoint.Y * renderScale);
            Canvas.SetLeft(startRect, maze.startPoint.X * renderScale);

            Rectangle endRect = new Rectangle();
            endRect.Fill = Brushes.Red;
            endRect.Opacity = 0.5;
            endRect.Width = endRect.Height = renderScale;

            Canvas.SetTop(endRect, maze.endPoint.Y * renderScale);
            Canvas.SetLeft(endRect, maze.endPoint.X * renderScale);

            MazeCanvas.Children.Add(startRect);
            MazeCanvas.Children.Add(endRect);
        }
    }

    public class Maze1
    {
        public Point startPoint;
        public Point endPoint;
        public Cell[,] cells;
        private Random rng;
        private int Width;
        private int Height;

        public Maze1(int width, int height)
        {
            Width = width;
            Height = height;
            rng = new Random();
            cells = new Cell[height, width];

            for(int i = 0; i < cells.GetLength(0); i++)
            {
                for(int j = 0; j < cells.GetLength(1); j++)
                {
                    cells[i, j] = new Cell();
                }
            }

            bool topEntrance = rng.NextDouble() > 0.5;

            if (topEntrance)
            {
                startPoint = new Point( rng.Next(0, width), 0);
                cells[(int)startPoint.Y, (int)startPoint.X].Walls -= Walls.North;
            }
            else
            {
                startPoint = new Point( 0, rng.Next(0, height));
                cells[(int)startPoint.Y, (int)startPoint.X].Walls -= Walls.West;
            }

            endPoint = new Point(width - 1 - startPoint.X, height - 1 - startPoint.Y);
            if(topEntrance)
                cells[(int)endPoint.Y, (int)endPoint.X].Walls -= Walls.South;
            else
                cells[(int)endPoint.Y, (int)endPoint.X].Walls -= Walls.East;


            CarvePassages((int)startPoint.X, (int)startPoint.Y, cells);
        }

        private void CarvePassages(int x, int y, Cell[,] grid)
        {
            grid[y, x].Visited = true;
            List<Walls> directions = new List<Walls>() { Walls.North, Walls.South, Walls.East, Walls.West };
            directions.Shuffle();

            foreach (Walls direction in directions)
            {
                int newX = (int)x;
                int newY = (int)y;

                Walls opposite = Walls.North;
                switch (direction)
                {
                    case Walls.North:
                        newY -= 1;
                        opposite = Walls.South;
                        break;

                    case Walls.South:
                        newY += 1;
                        opposite = Walls.North;
                        break;

                    case Walls.East:
                        newX += 1;
                        opposite = Walls.West;
                        break;

                    case Walls.West:
                        newX -= 1;
                        opposite = Walls.East;
                        break;
                }

                if( newX >= 0 && newX < Width && newY >= 0 && newY < Height && !grid[newY, newX].Visited)
                {
                    grid[y, x].Walls -= direction;
                    grid[newY, newX].Walls -= opposite;
                    CarvePassages(newX, newY, grid);
                }  
            }
        }
    }

    public class Maze2
    {
        public Maze2(int width, int height, Canvas canvas)
        {
            double cellSize = Math.Min(canvas.Width / width, canvas.Height / height);

            SolidColorBrush wallColor = new SolidColorBrush(Colors.Black);
            SolidColorBrush passageColor = new SolidColorBrush(Colors.White);


            foreach ( CellNode node in MakeMaze(width, height))
            {
                Rectangle rect = new Rectangle
                {
                    StrokeThickness = 0,
                    Fill = wallColor,
                    Width = cellSize,
                    Height = cellSize
                };
                Canvas.SetTop(rect, node.Coordinates.X * cellSize);
                Canvas.SetLeft(rect, node.Coordinates.Y * cellSize);

                canvas.Children.Add(rect);
            }
        }

        private IEnumerable<CellNode> MakeMaze(int width, int height)
        {
            Random rand = new Random();

            List<Vector> directions = new List<Vector>() { new Vector(-1, 0), new Vector(1, 0), new Vector(0, -1), new Vector(0, 1) };

            Dictionary<Point, CellNode> maze = new Dictionary<Point, CellNode>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Point coord = new Point(i, j);
                    maze.Add(coord, new CellNode(coord));
                }
            }

            Stack<CellNode> cellStack = new Stack<CellNode>();
            maze.TryGetValue(new Point(rand.Next(width), rand.Next(height)), out CellNode startNode);
            cellStack.Push(startNode);

            while (cellStack.Count != 0)
            {
                CellNode currentCell = cellStack.Pop();
                currentCell.Visited = true;
                directions.Shuffle();

                foreach (Vector vec in directions)
                {
                    Point newPoint = Point.Add(currentCell.Coordinates, vec);

                    if (newPoint.X >= 0 && newPoint.X < width &&
                        newPoint.Y >= 0 && newPoint.Y < height &&
                        maze.TryGetValue(newPoint, out CellNode newNode) && !newNode.Visited)
                    {
                        currentCell.Connections.Add(newNode);
                        newNode.Connections.Add(currentCell);
                        cellStack.Push(newNode);
                    }

                    yield return currentCell;
                }
            }

            //maze.TryGetValue(new Point(0, 0), out CellNode startCell);
        }
    }

    public class CellNode
    {
        public bool Visited;
        public Point Coordinates;
        public List<CellNode> Connections;

        private CellNode()
        {
            Visited = false;
            Connections = new List<CellNode>();
        }

        public CellNode(int x, int y) : this()
        {
            Coordinates = new Point(x, y);
        }

        public CellNode(Point coord) : this()
        {
            Coordinates = coord;
        }
    }

    public class Cell
    {
        public Walls Walls { get; set; }
        public bool Visited { get; set; }

        public Cell()
        {
            Visited = false;
            Walls = Walls.North | Walls.South | Walls.East | Walls.West;
        }
    }

    public enum Walls
    {
        North = 1,
        South = 2,
        East = 4,
        West = 8
    }

    public static class MyExtentions
    {
        public static Random rnd = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while( n > 1)
            {
                int k = rnd.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }
}
