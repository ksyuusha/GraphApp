using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GraphApp
{
    public partial class Form1 : Form
    {
        private Dictionary<string, List<Edge>> graph = new Dictionary<string, List<Edge>>();
        private int nodeCount = 0; // Для отслеживания количества вершин

        public Form1()
        {
            InitializeComponent();
        }

        // Добавление ребра
        private void btnAddEdge_Click(object sender, EventArgs e)
        {
            string from = txtFrom.Text;
            string to = txtTo.Text;
            int weight;

            if (!int.TryParse(txtWeight.Text, out weight))
            {
                MessageBox.Show("Вес должен быть числом.");
                return;
            }

            // Добавляем вершину "from" (исходную), если она ещё не существует
            if (!graph.ContainsKey(from))
            {
                graph[from] = new List<Edge>();
                nodeCount++; // Увеличиваем количество вершин
            }

            // Добавляем вершину "to" (конечную), если она ещё не существует
            if (!graph.ContainsKey(to))
            {
                graph[to] = new List<Edge>();
                nodeCount++; // Увеличиваем количество вершин
            }

            // Добавляем ребро в обоих направлениях, так как это неориентированный граф
            graph[from].Add(new Edge(to, weight));
            graph[to].Add(new Edge(from, weight));

            // Обновляем таблицу
            dataGridView1.Rows.Add(from, to, weight);

            // Перерисовать граф
            panelGraph.Invalidate();
        }

        // Нахождение кратчайшего пути с помощью алгоритма Дейкстры
        private void btnFindShortestPath_Click(object sender, EventArgs e)
        {
            string start = txtStart.Text;
            string end = txtEnd.Text;

            var result = Dijkstra(start, end);
            lblResult.Text = result == int.MaxValue ? "Нет пути" : $"Кратчайший путь: {result}";
        }

        // Алгоритм Дейкстры
        private int Dijkstra(string start, string end)
        {
            var distances = new Dictionary<string, int>();
            var previousNodes = new Dictionary<string, string>();
            var nodes = new List<string>(graph.Keys);

            foreach (var node in nodes)
            {
                distances[node] = int.MaxValue;
                previousNodes[node] = null;
            }
            distances[start] = 0;

            while (nodes.Count > 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);
                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == end)
                {
                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var edge in graph[smallest])
                {
                    int alt = distances[smallest] + edge.Weight;
                    if (alt < distances[edge.To])
                    {
                        distances[edge.To] = alt;
                        previousNodes[edge.To] = smallest;
                    }
                }
            }

            return distances.ContainsKey(end) ? distances[end] : int.MaxValue;
        }

        // Отрисовка графа
        private void panelGraph_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int nodeRadius = 20; // Радиус узлов
            Dictionary<string, Point> nodePositions = new Dictionary<string, Point>();

            // Задаём центр и радиус круга для размещения вершин
            int centerX = panelGraph.Width / 2;
            int centerY = panelGraph.Height / 2;
            int circleRadius = Math.Min(centerX, centerY) - 50; // Радиус круга

            // Угол между соседними вершинами (в радианах)
            double angleStep = 2 * Math.PI / nodeCount;

            // Размещаем вершины по окружности
            int i = 0;
            foreach (var node in graph.Keys)
            {
                double angle = i * angleStep; // Угол для текущей вершины
                int x = centerX + (int)(circleRadius * Math.Cos(angle)); // Координата X
                int y = centerY + (int)(circleRadius * Math.Sin(angle)); // Координата Y
                nodePositions[node] = new Point(x, y);
                i++;
            }

            Pen edgePen = new Pen(Color.Black, 2);

            // Рисуем рёбра (линии) без стрелок, так как граф неориентированный
            foreach (var from in graph.Keys)
            {
                foreach (var edge in graph[from])
                {
                    if (nodePositions.TryGetValue(from, out Point fromPos) && nodePositions.TryGetValue(edge.To, out Point toPos))
                    {
                        // Проверяем, не нарисовано ли уже обратное ребро (т.е. ребро от "to" к "from")
                        if (from.CompareTo(edge.To) < 0)
                        {
                            // Вычисляем направление от "from" к "to"
                            double dx = toPos.X - fromPos.X;
                            double dy = toPos.Y - fromPos.Y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            // Вычисляем новые координаты, чтобы линия не пересекала круги вершин
                            int edgeOffset = nodeRadius; // Смещение линии на радиус узла
                            Point adjustedFromPos = new Point(
                                fromPos.X + (int)(edgeOffset * dx / distance),
                                fromPos.Y + (int)(edgeOffset * dy / distance)
                            );
                            Point adjustedToPos = new Point(
                                toPos.X - (int)(edgeOffset * dx / distance),
                                toPos.Y - (int)(edgeOffset * dy / distance)
                            );

                            // Рисуем ребро
                            g.DrawLine(edgePen, adjustedFromPos, adjustedToPos);

                            // Подписываем вес ребра в середине линии
                            Point midPoint = new Point((adjustedFromPos.X + adjustedToPos.X) / 2, (adjustedFromPos.Y + adjustedToPos.Y) / 2);
                            g.DrawString(edge.Weight.ToString(), this.Font, Brushes.Red, midPoint);
                        }
                    }
                }
            }

            // Рисуем узлы (круги)
            foreach (var node in nodePositions)
            {
                Point nodePos = node.Value;
                g.FillEllipse(Brushes.Blue, nodePos.X - nodeRadius, nodePos.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                g.DrawEllipse(edgePen, nodePos.X - nodeRadius, nodePos.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                // Подписываем узел (имя вершины)
                g.DrawString(node.Key, this.Font, Brushes.White, nodePos.X - 10, nodePos.Y - 10);
            }
        }
    }

    // Класс рёбер
    public class Edge
    {
        public string To { get; }
        public int Weight { get; }

        public Edge(string to, int weight)
        {
            To = to;
            Weight = weight;
        }
    }
}
