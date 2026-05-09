record Path(Dir dir, Path? next) { }

class Solver {
    static List<Dir> build(Path? path) {
        List<Dir> dirs = [];
        for (; path != null; path = path.next)
            dirs.Add(path.dir);
        dirs.Reverse();
        return dirs;
    }

    public static List<Dir>? solve(World start) {
        Queue<(World, Path?)> q = [];
        q.Enqueue((start, null));
        HashSet<World> visited = [start];
        List<Pos> dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];

        while (q.Count > 0) {
            (World w, Path? path) = q.Dequeue();
            if (w.solved()) {
                Console.WriteLine($"explored {visited.Count} states");
                return build(path);
            }
            foreach (Dir dir in dirs) {
                World w1 = new(w);  // clone world
                if (w1.move(dir) && !visited.Contains(w1)) {
                    visited.Add(w1);
                    q.Enqueue((w1, new Path(dir, path)));
                }
            }
        }

        return null;
    }
}
