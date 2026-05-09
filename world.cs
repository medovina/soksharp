global using static Obj;

global using Pos = (int x, int y);
global using Dir = (int dx, int dy);

enum Obj { None, Box, Wall }

delegate void Notify();

class World {
    public Obj[,] map;
    List<Pos> targets = [];
    public Pos player;
    public event Notify? changed;    // fires whenever the board changes

    public World(string filename) {
        List<string> lines =
            [.. File.ReadLines(filename).Select(line => line.Trim())];
        int height = lines.Count, width = lines[0].Length;
        map = new Obj[width, height];

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                char c = lines[y][x];
                map[x, y] = c switch {
                    '#' => Wall,
                    '$' or '*' => Box,
                    _ => None};
                if (c == '@')
                    player = (x, y);
                else if (c is '.' or '*')
                    targets.Add((x, y));
            }
        }
    }

    public World(World from) {
        map = (Obj[,]) from.map.Clone();
        targets = [.. from.targets];
        player = from.player;
    }

    public override bool Equals(object? o) =>
        o is World w &&
        map.Cast<Obj>().SequenceEqual(w.map.Cast<Obj>()) &&
        player == w.player;

    public override int GetHashCode() {
        HashCode h = new();
        foreach (Obj o in map.Cast<Obj>())
            h.Add(o);
        h.Add(player);
        return h.ToHashCode();
    }

    public int width => map.GetLength(0);
    public int height => map.GetLength(1);

    public Obj at(Pos p) => map[p.x, p.y];

    public bool is_target(Pos p) => targets.Contains(p);

    public bool move(Dir dir) {
        Pos p1 = (player.x + dir.dx, player.y + dir.dy);
        if (at(p1) == None) {
            player = p1;
            changed?.Invoke();
            return true;
        }
        if (at(p1) == Box) {
            Pos p2 = (p1.x + dir.dx, p1.y + dir.dy);
            if (at(p2) == None) {
                map[p2.x, p2.y] = Box;
                map[p1.x, p1.y] = None;
                player = p1;
                changed?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool solved() => targets.All(pos => at(pos) == Box);
}
