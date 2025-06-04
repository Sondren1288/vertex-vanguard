using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Graph
{
    private int[,] adjMatrix;
    private int size;
    public string[] vertexData;

    public Graph(int size)
    {
        this.adjMatrix = new int[size, size];
        this.size = size;
        this.vertexData = new string[size];
        for (int i = 0; i < size; i++)
        {
            vertexData[i] = "";
        }
    }

    public void AddEdge(int u, int v, int weight)
    {
        if (0 <= u && u < size && 0 <= v && v < size)
        {
            adjMatrix[u, v] = weight;
            // adjMatrix[v, u] = weight; // For undirected graph
        }
    }

    public void AddVertexData(int vertex, string data)
    {
        if (0 <= vertex && vertex < size)
        {
            vertexData[vertex] = data;
        }
    }

    public (int, string[]) Dijkstra(string startVertexData, string endVertexData)
    {
        int startVertex = Array.IndexOf(vertexData, startVertexData);
        int endVertex = Array.IndexOf(vertexData, endVertexData);
        
        int[] distances = Enumerable.Repeat(int.MaxValue, size).ToArray();
        int[] predecessors = Enumerable.Repeat(-1, size).ToArray();
        distances[startVertex] = 0;
        bool[] visited = new bool[size];

        for (int i = 0; i < size; i++)
        {
            int minDistance = int.MaxValue;
            int u = -1;
            
            for (int j = 0; j < size; j++)
            {
                if (!visited[j] && distances[j] < minDistance)
                {
                    minDistance = distances[j];
                    u = j;
                }
            }

            if (u == -1 || u == endVertex)
            {
                Debug.Log($"Breaking out of loop. Current vertex: {(u != -1 ? vertexData[u] : "None")}");
                Debug.Log($"Distances: [{string.Join(", ", distances)}]");
                break;
            }

            visited[u] = true;
            Debug.Log($"Visited vertex: {vertexData[u]}");

            for (int v = 0; v < size; v++)
            {
                if (adjMatrix[u, v] != 0 && !visited[v])
                {
                    int alt = distances[u] + adjMatrix[u, v];
                    if (alt < distances[v])
                    {
                        distances[v] = alt;
                        predecessors[v] = u;
                    }
                }
            }
        }

        return (distances[endVertex], GetPath(predecessors, startVertexData, endVertexData));
    }

    public string[] GetPath(int[] predecessors, string startVertex, string endVertex)
    {
        List<string> path = new List<string>();
        int current = Array.IndexOf(vertexData, endVertex);
        
        while (current != -1)
        {
            path.Insert(0, vertexData[current]);
            current = predecessors[current];
            
            if (current == Array.IndexOf(vertexData, startVertex))
            {
                path.Insert(0, startVertex);
                break;
            }
        }
        
        return path.ToArray();
    }
}