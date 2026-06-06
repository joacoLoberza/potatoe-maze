using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateMaze : MonoBehaviour
{
    public float progress = 0.0f;
    private enum WallSide { Top, Bottom, Right, Left };
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private GameObject doorPrefab;

    private class MazeCell
    {
        public enum CellStatus { Bussy, Free, NoExit, Entrace, Exit, Border };
        public CellStatus status;
        public int x;
        public int y;
        public MazeCell formerCell;
        public List<MazeCell> adyacents = new List<MazeCell>();

        public MazeCell(CellStatus status, int x, int y, MazeCell formerCell = null, List<MazeCell> adyacents = null)
        {
            (this.status, this.x, this.y, this.formerCell, this.adyacents) = (status, x, y, formerCell, adyacents);
        }
    }

    private class MazeWall
    {
        public MazeCell [] neighbors;
        public enum Axis { Vertical, Horizontal };
        public Axis axis;
        public MazeCell [] sideOne;
        public MazeCell [] sideTwo;
        public bool isBorder; 
        public bool isDoor;
        public MazeWall(MazeCell [] neighbors, MazeCell [] sideOne, MazeCell [] sideTwo, Axis axis, bool isBorder = false, bool isDoor = false)
        {
            (this.neighbors, this.sideOne, this.sideTwo, this.axis, this.isBorder, this.isDoor) = (neighbors, sideOne, sideTwo, axis, isBorder, isDoor);   
        }
    }

    public IEnumerator StartGeneration()
    {
        int level = PlayerPrefs.GetInt("level", 1);
        
        int mazeSize = 20 + level * 1.8f;
        yield return null;
    }
    private (List<MazeWall> walls, MazeCell[,] map) DiagramMaze((int mazeSize, int opensAmount, float doorChance) data)
    {
        var (mazeSize, opensAmount, doorChance) = data;
        MazeCell[,] map = new MazeCell[mazeSize, mazeSize];
        List<MazeWall> walls = new List<MazeWall>();
        
        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                if ((x == 0 || x == mazeSize-1) || (y == 0 || y == mazeSize - 1))
                {
                    map[x, y] = new MazeCell(MazeCell.CellStatus.Border, x, y);                 
                } else
                {
                    map[x,y] = new MazeCell(MazeCell.CellStatus.Free, x, y);
                    
                }
            }
        }

        int axis = Random.Range(0,2);
        int xEntrace;
        int yEntrace;
        int xExit;
        int yExit;

        if (axis == 0)
        {
            xEntrace = Random.Range(1,mazeSize-1);
            yEntrace = Random.Range(0,2) == 0? 0 : mazeSize-1;
            xExit = mazeSize -1 - xEntrace;
            yExit = yEntrace == 0? 1 : 0;
        } else
        {
            xEntrace = Random.Range(0,2) == 0? 0 : mazeSize-1;
            yEntrace = Random.Range(1,mazeSize-1);
            xExit = xEntrace == 0? 1 : 0;
            yExit = mazeSize - 1 - yEntrace;
        }

        MazeCell nextCell;
        MazeCell currentCell = map[xEntrace, yEntrace];
        currentCell.status = MazeCell.CellStatus.Entrace;
        map[xExit, yExit].status = MazeCell.CellStatus.Exit;

        if (axis == 0)
        {
            if (yEntrace == 0)
                nextCell = map[xEntrace, yEntrace+1];
            else
                nextCell = map[xEntrace, yEntrace-1];
        }
        else
        {
            if (xEntrace == 0)
                nextCell = map[xEntrace+1, yEntrace];
            else
                nextCell = map[xEntrace-1, yEntrace];
        }

        nextCell.formerCell = currentCell;
        currentCell = nextCell;

        while (true)
        {
            CalcAdya(currentCell.x, currentCell.y, currentCell, map);

            if (currentCell.adyacents.Count == 0)
            {
                if (currentCell.status == MazeCell.CellStatus.Free)
                    currentCell.status = MazeCell.CellStatus.NoExit;
                
                if (currentCell.formerCell.status == MazeCell.CellStatus.Entrace)
                    break;
            
                currentCell = currentCell.formerCell;
                continue;
            }
            
            currentCell.status = MazeCell.CellStatus.Bussy;
            nextCell = currentCell.adyacents[Random.Range(0, currentCell.adyacents.Count)];
            nextCell.formerCell = currentCell;
            
            List<WallSide> wallSide = new List<WallSide>();
            if (currentCell.formerCell.x == nextCell.x)
            {
                wallSide.Add(WallSide.Top);
                wallSide.Add(WallSide.Bottom);
            } else if (currentCell.formerCell.y == nextCell.y)
            {
                wallSide.Add(WallSide.Right);
                wallSide.Add(WallSide.Left);
            } else
            {
                if (currentCell.formerCell == map[currentCell.x + 1, currentCell.y] || nextCell == map[currentCell.x + 1, currentCell.y])
                {
                    wallSide.Add(WallSide.Top);
                } else wallSide.Add(WallSide.Bottom);

                if (currentCell.formerCell == map[currentCell.x, currentCell.y + 1] || nextCell == map[currentCell.x, currentCell.y + 1])
                {
                    wallSide.Add(WallSide.Left);
                } else wallSide.Add(WallSide.Right);
            }

            foreach (WallSide side in wallSide)
            {
                switch (side)
                {
                    case(WallSide.Top):
                        MazeCell topNeighbor = map[currentCell.x + 1, currentCell.y];
                        if (!walls.Any(wall => (
                            (wall.neighbors[0] == currentCell && wall.neighbors[1] == topNeighbor) ||
                            (wall.neighbors[0] == topNeighbor && wall.neighbors[1] == currentCell)
                        )))
                            walls.Add( new MazeWall(
                                new MazeCell[2]
                                {
                                    currentCell,
                                    topNeighbor,
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x, currentCell.y - 1],
                                    map[currentCell.x - 1 ,currentCell.y - 1]
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x, currentCell.y + 1],
                                    map[currentCell.x - 1, currentCell.y + 1]
                                },
                                MazeWall.Axis.Horizontal,
                                map[currentCell.x + 1, currentCell.y].status == MazeCell.CellStatus.Border? true : false
                            ));
                        break;  
                    
                    case(WallSide.Bottom):
                        MazeCell bottomNeighbor = map[currentCell.x - 1, currentCell.y];
                        if (!walls.Any(wall => (
                            (wall.neighbors[0] == currentCell && wall.neighbors[1] == bottomNeighbor) ||
                            (wall.neighbors[0] == bottomNeighbor && wall.neighbors[1] == currentCell)
                        )))
                            walls.Add( new MazeWall(
                                new MazeCell[2]
                                {
                                    currentCell,
                                    bottomNeighbor
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x, currentCell.y - 1],
                                    map[currentCell.x + 1 ,currentCell.y - 1]
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x, currentCell.y + 1],
                                    map[currentCell.x + 1, currentCell.y + 1]
                                },
                                MazeWall.Axis.Horizontal,
                                map[currentCell.x - 1, currentCell.y].status == MazeCell.CellStatus.Border? true : false
                            ));
                        break;
                    
                    case (WallSide.Left):
                        MazeCell leftNeighbor = map[currentCell.x, currentCell.y - 1];
                        if (!walls.Any(wall => (
                            (wall.neighbors[0] == currentCell && wall.neighbors[1] == leftNeighbor) ||
                            (wall.neighbors[0] == leftNeighbor && wall.neighbors[1] == currentCell)
                        )))
                            walls.Add( new MazeWall(
                                new MazeCell[2]
                                {
                                    currentCell,
                                    leftNeighbor
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x - 1, currentCell.y],
                                    map[currentCell.x - 1 ,currentCell.y - 1]
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x + 1, currentCell.y],
                                    map[currentCell.x + 1, currentCell.y - 1]
                                },
                                MazeWall.Axis.Vertical,
                                map[currentCell.x, currentCell.y - 1].status == MazeCell.CellStatus.Border? true : false
                            ));
                        break;
                    
                    case (WallSide.Right):
                        MazeCell rightNeighbor = map[currentCell.x, currentCell.y + 1];
                        if (!walls.Any(wall => (
                            (wall.neighbors[0] == currentCell && wall.neighbors[1] == rightNeighbor) ||
                            (wall.neighbors[0] == rightNeighbor && wall.neighbors[1] == currentCell)
                        )))
                            walls.Add( new MazeWall(
                                new MazeCell[2]
                                {
                                    currentCell,
                                    map[currentCell.x, currentCell.y + 1]
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x - 1, currentCell.y],
                                    map[currentCell.x - 1 ,currentCell.y + 1]
                                },
                                new MazeCell[2]
                                {
                                    map[currentCell.x + 1, currentCell.y],
                                    map[currentCell.x + 1, currentCell.y + 1]
                                },
                                MazeWall.Axis.Horizontal,
                                map[currentCell.x + 1, currentCell.y].status == MazeCell.CellStatus.Border? true : false
                            ));
                        break;  
                }
            }

            currentCell = nextCell;
        }

        int opens = 0;
        while (true)
        {
            MazeWall wall = walls[Random.Range(0, walls.Count)];
            
            if (wall.isBorder) continue;

            bool authRoom1 = false;
            bool authRoom2 = false;

            foreach (MazeWall compWall in walls)
            {
                if (compWall == wall) continue;

                bool alley = false;
                foreach (MazeCell neigh in compWall.neighbors) {
                    if (neigh.status == MazeCell.CellStatus.NoExit)
                    {
                        alley = true;
                        break;
                    }
                }
                if (alley) continue;
 
                foreach (MazeCell compCell in compWall.neighbors)
                {
                    foreach (MazeCell cell in wall.sideOne)
                        if (cell == compCell) {
                            authRoom1 = true;
                            continue;
                        } 

                    foreach (MazeCell cell in wall.sideTwo)
                        if (cell == compCell) authRoom2 = true;
                }
            }

            if (authRoom1 && authRoom2)
            {
                if (Random.value < doorChance) {
                    wall.isDoor = true;
                } else
                    walls.Remove(wall);
                opens ++;
            }
            if (opens == opensAmount) break;
        }

        return (walls, map);
    }

    private void CalcAdya (int x, int y, MazeCell cell, MazeCell[,] map)
    {
        cell.adyacents = new List<MazeCell>();
        MazeCell[] sides = new MazeCell[] {
            map[x+1, y],
            map[x-1, y],
            map[x, y+1],
            map[x, y-1]
        };
        foreach (MazeCell adyCell in sides)
        {
            if (adyCell.status == MazeCell.CellStatus.Free)
                cell.adyacents.Add(adyCell);
        }
    }

    private IEnumerable Generate(List<MazeWall> walls, MazeCell[,] cells)
    {
        int mazeSize = cells.GetLength(0);
        GameObject floor = Instantiate(planePrefab, new Vector3(mazeSize*10f/2f, 0f, mazeSize*10f/2f), Quaternion.identity);
        floor.transform.localScale = new Vector3(mazeSize-20, 0f, mazeSize-20);

        for (MazeWall.Axis axis = 0; (int)axis < 3; axis++) {
            for (int idx = 0; idx < mazeSize-1; idx++)
            {
                int? [] wSecuence = new int? [mazeSize];
                foreach (MazeWall wall in walls)
                {
                    if (wall.axis != axis) continue;
                    if (axis == MazeWall.Axis.Vertical)
                    {
                        MazeCell lefter = wall.neighbors[0].y < wall.neighbors[1].y? wall.neighbors[0] : wall.neighbors[1];
                        if (lefter.y == idx)
                            if (!wall.isDoor)
                            {
                                CreateDoor(wall.neighbors[0].x, idx, wall.axis);
                                continue;
                            }
                            wSecuence[lefter.x] = lefter.x;
                            walls.Remove(wall);

                    } else
                    {
                        MazeCell toper = wall.neighbors[0].x < wall.neighbors[1].x? wall.neighbors[0] : wall.neighbors[1];
                        if (toper.x == idx)
                            if (!wall.isDoor)
                            {
                                CreateDoor(wall.neighbors[0].x, idx, wall.axis);
                                continue;
                            }
                            wSecuence[toper.y] = toper.y;
                            walls.Remove(wall);
                    }
                }

                int?[] large = new int?[2];
                foreach (int? pos in wSecuence)
                {
                    if (pos != null)
                    {
                        if (large[0] == null)
                        {
                            large[0] = pos;
                        } else if (wSecuence[(int)pos+1] == null)
                        {
                            large[1] = pos;
                        }
                    }

                    if (large[0] != null && large[1] != null)
                    {
                        Vector3 position = new Vector3();
                        Vector3 scale = new Vector3();
                        position.z = 2;
                        scale.y = 4;
                        if (axis == MazeWall.Axis.Vertical)
                        {
                            position.x = ((float)large[0] + ((float)large[1] - (float)large[0])/2f) * 10;
                            position.y = idx * 10;
                            scale.x = ((float)large[1] - (float)large[0] + 2) * 10;
                            scale.y = 2;

                        } else
                        {
                            position.y = ((float)large[0] + ((float)large[1] - (float)large[0])/2f) * 10;
                            position.x = idx * 10;
                            scale.y = ((float)large[1] - (float)large[0] + 2) * 10;
                            scale.x = 2;
                        }

                        GameObject largeWall = Instantiate(wallPrefab, position, Quaternion.identity);
                        largeWall.transform.localScale = scale;
                        large = new int? [2];
                    }
                }

            }
        }
        yield return null;
    }

    private GameObject CreateDoor(int x, int z, MazeWall.Axis axis)
    {
        Vector3 position = new Vector3(x*10 + 5f, 2.0f, z*10 + 5f);
        Vector3 scale = axis == MazeWall.Axis.Vertical?
            new Vector3(2f, 4f, 10f):
            new Vector3(10f, 4f, 2f);

        GameObject door = Instantiate(doorPrefab, position, Quaternion.identity);
        door.transform.localScale = scale;
        return door;
    }
}