/*
 * 1- Avoid dead ends by using move availability matrix
 * 2- Reset map after Surface
 * 3- EnemyTracker to scan map to find candidates matching opponent's move history
 * */
 
using System;
using System.Collections.Generic;

static class ArrayExtensions
{
    public static int[,] CloneMap(this int[,] source)
    {
        int dimension1Length = source.GetLength(0);
        int dimension2Length = source.GetLength(1);
        int[,] clone = new int[dimension1Length, dimension2Length];
        Array.Copy(source, clone, dimension1Length * dimension2Length);
        return clone;
    }
}
class MoveInput
{
    public (int, int) MyPosition { get; set; }
    public int TorpedoCooldown { get; set; }
}

class EnemyTracker
{
    protected readonly int[,] Map;

    public EnemyTracker(int[,] map)
    {
        Map = map.CloneMap();
    }
    public (int, int) GuessEnemyLocation(MoveInput input)
    {
        var (x, y) = input.MyPosition;

        if (y >= 4)
        {
            return (x, y - 4);
        }

        return (x, y + 4);
    }
}
class Submarine
{
    readonly List<string> _actions = new List<string>();
    private NavigatorBase _navigator;
    private readonly EnemyTracker _enemyTracker;

    public Submarine(NavigatorBase navigator, EnemyTracker enemyTracker)
    {
        _navigator = navigator;
        _enemyTracker = enemyTracker;
    }

    public void Next(MoveInput moveInput)
    {
        if(moveInput.TorpedoCooldown == 0){
            Torpedo(_enemyTracker.GuessEnemyLocation(moveInput));
        }
        var (x, y) = moveInput.MyPosition;
        var next = _navigator.Next((x,y));
        if (next == null)
        {
            Surface();
        }
        else
        {
            Move(next.Value);
        }
    }

    public void Move(char direction){
        _actions.Add($"MOVE {direction} TORPEDO");
    }
    public void Torpedo((int, int) coordinate)
    {
        var (x, y) = coordinate;
        _actions.Add($"TORPEDO {x} {y}");
    }

    public void Surface()
    {
        Reset();
        _actions.Add("SURFACE");
    }

    private void Reset()
    {
        _actions.Clear();
    }

    public override string ToString()
    {
        return string.Join("|", _actions);

    }
}

class Direction
{
    public const char North = 'N';
    public const char South = 'S';
    public const char West = 'W';
    public const char East = 'E';
}

abstract class NavigatorBase
{
    protected readonly int[,] Map;

    protected NavigatorBase(int[,] map)
    {
        Map = map.CloneMap();
    }
    protected bool CanMoveEast((int, int) currentPosition)
    {
        var (x, y) = currentPosition;
        if (x == 14)
        {
            return false;
        }

        return Map[x + 1, y] == 0;
    }

    protected bool CanMoveSouth((int, int) currentPosition)
    {
        var (x, y) = currentPosition;
        if (y == 14)
        {
            return false;
        }

        return Map[x, y+1] == 0;
    }

    protected bool CanMoveWest((int, int) currentPosition)
    {
        var (x, y) = currentPosition;
        if (x == 0)
        {
            return false;
        }

        return Map[x-1, y] == 0;
    }

    protected bool CanMoveNorth((int, int) currentPosition)
    {
        var (x, y) = currentPosition;
        if (y == 0)
        {
            return false;
        }

        return Map[x, y-1] == 0;
    }

    public abstract char? Next((int, int) currentPosition);
}
class ClockwiseNavigatorStrategy :NavigatorBase
{
    public ClockwiseNavigatorStrategy(int[,] map) : base(map){}
    

    public override char? Next((int, int) currentPosition)
    {
        var (x, y) = currentPosition;
        Map[x, y] = 1;

        if (CanMoveEast(currentPosition))
        {
            return Direction.East;
        }
        if (CanMoveSouth(currentPosition))
        {
            return Direction.South;
        }
        if (CanMoveWest(currentPosition))
        {
            return Direction.West;
        }
        if (CanMoveNorth(currentPosition))
        {
            return Direction.North;
        }

        return null;
    }
}
class Player
{
    
    static void Main(string[] args)
    {
        int[,] map = null;
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);
        map = ScanMap(height);
        var moveStrategy = new ClockwiseNavigatorStrategy(map);
        var enemyTracker = new EnemyTracker(map);

    
        Console.WriteLine("0 14");

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int myLife = int.Parse(inputs[2]);
            int oppLife = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = Console.ReadLine();
            string opponentOrders = Console.ReadLine();
            
            var submarine = new Submarine(moveStrategy, enemyTracker);

            submarine.Next(new MoveInput{MyPosition = (x,y), TorpedoCooldown = torpedoCooldown});
            Console.WriteLine(submarine);
        }
    }

    public static int[,] ScanMap(int height){
        int[,] map = new int[15, 15];
        for (int j = 0; j < height; j++)
        {
            string line = Console.ReadLine();
            char[] rowChars = line.ToCharArray();
            for(int i = 0; i < rowChars.Length; i++){
                map[i, j] = rowChars[i].Equals('.') ? 0 : 1;   
            }
        }
        return map;
    }

}

