using System;
using System.Collections.Generic;
using UnityEngine;

public class GridAnalyzer : MonoBehaviour
{
    private int[] _thresholds;

    private bool[,] _visitedMask;
    private Queue<Vector2Int> _searchQueue;
    private List<Vector2Int> _currentGroup;

    private readonly int[] _rowDir = { 0, 0, 1, -1 };
    private readonly int[] _colDir = { 1, -1, 0, 0 };

    public void Initialize(int rows, int cols, int[] thresholds)
    {
        _visitedMask = new bool[rows, cols];
        _searchQueue = new Queue<Vector2Int>(rows * cols);
        _currentGroup = new List<Vector2Int>(rows * cols);

        _thresholds = thresholds;
    }

    /// <summary>
    /// Finds group sizes and triggers change icon
    /// </summary>
    /// <param name="boardData"> Grid based board</param>
    /// <param name="M"> row count</param>
    /// <param name="N"> column count</param>
    /// <param name="onBlockAnalyzed">Trigger for block visual update</param>
    /// <returns>is game deadlocked</returns>
    public bool AnalyzeGrid(byte[,] boardData, int M, int N, System.Action<int, int, int> onBlockAnalyzed)
    {
        System.Array.Clear(_visitedMask, 0, _visitedMask.Length);
        bool hasAnyGroup = false;

        for (int r = 0; r < M; r++)
        {
            for (int c = 0; c < N; c++)
            {
                if (_visitedMask[r, c]) continue;

                byte targetColor = boardData[r, c];

                _searchQueue.Clear();
                _currentGroup.Clear();

                _searchQueue.Enqueue(new Vector2Int(r, c));
                _visitedMask[r, c] = true;
                _currentGroup.Add(new Vector2Int(r, c));

                SearchColor(M, N, boardData, targetColor);
                
                int groupSize = _currentGroup.Count;

                if (groupSize >= 2)
                {
                    hasAnyGroup = true;
                }

                int iconType = DetermineIconType(groupSize);

                foreach (var blockPos in _currentGroup)
                {
                    onBlockAnalyzed?.Invoke(blockPos.x, blockPos.y, iconType);
                }
            }
        }

        return !hasAnyGroup;
    }

    //TODO: Find better name for this
    /// <summary>
    /// Searchs color groups by color.
    /// Uses BFS.
    /// </summary>
    /// <param name="M"> Row count</param>
    /// <param name="N"> Column Count</param>
    /// <param name="boardData"> Grid based game board</param>
    /// <param name="targetColor"> first blocks color</param>
    private void SearchColor(int M, int N, byte[,] boardData, int targetColor)
    {
        while (_searchQueue.Count > 0)
        {
            Vector2Int current = _searchQueue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nr = current.x + _rowDir[i];
                int nc = current.y + _colDir[i];

                if (nr >= 0 && nr < M && nc >= 0 && nc < N)
                {
                    if (!_visitedMask[nr, nc] && boardData[nr, nc] == targetColor)
                    {
                        _visitedMask[nr, nc] = true;
                        Vector2Int neighbor = new Vector2Int(nr, nc);
                        _searchQueue.Enqueue(neighbor);
                        _currentGroup.Add(neighbor);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Determines color based on count.
    /// Uses thresholds in class.
    /// </summary>
    /// <param name="count"> size of group</param>
    /// <returns>Icon type</returns>
    private int DetermineIconType(int count)
    {
        for (int i = _thresholds.Length-1; i >= 0; i--)
        {
            if (_thresholds[i] < count)
            {
                // 0 is always default.
                return i + 1;
            }
        }
        return 0;
    }
    // A 3 B 5 C 8
    // 0 1 2 3 = 0
    // 4 5 = 1
    // 6 7 8 = 2
    // 9 10 11 = 3
}