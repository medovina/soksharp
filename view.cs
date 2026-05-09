using Cairo;
using static Gdk.Constants;
using static Gdk.Functions;
using Gtk;
using Pixbuf = GdkPixbuf.Pixbuf;

class View : ApplicationWindow {
    World world;
    DrawingArea area;
    Queue<Dir> moves = [];

    static Pixbuf read_image(string name) => Pixbuf.NewFromFile($"images/{name}.png")!;

    Pixbuf box = read_image("box"), box_target = read_image("box_target"),
           empty = read_image("empty"), guy = read_image("guy"),
           target = read_image("target"), wall = read_image("wall");

    const int SIZE = 64;

    public View(Application app, World world) {
        Application = app;
        this.world = world;
        Title = "sokoban";

        area = new();
        area.SetDrawFunc(draw);
        area.SetSizeRequest(world.width * SIZE, world.height * SIZE);
        Child = area;
        world.changed += area.QueueDraw;

        EventControllerKey key_controller = new();
        key_controller.OnKeyPressed += on_key_pressed;
        AddController(key_controller);
    }

    void draw(DrawingArea area, Context c, int width, int height) {
        for (int y = 0; y < world.height; ++y)
            for (int x = 0; x < world.width; ++x) {
                CairoSetSourcePixbuf(c, empty, x * SIZE, y * SIZE);
                c.Paint();

                Pixbuf image = world.at((x, y)) switch {
                    None =>
                        (x, y) == world.player ? guy :
                        world.is_target((x, y)) ? target : empty,
                    Obj.Box =>
                        world.is_target((x, y)) ? box_target : box,
                    Wall => wall,
                    _ => throw new Exception("bad value")
                };
                CairoSetSourcePixbuf(c, image,
                    x * SIZE + (SIZE - image.Width) / 2,
                    y * SIZE + (SIZE - image.Height) / 2);
                c.Paint();
            }
    }

    void move(Dir dir) {
        world.move(dir);
        moves.Clear();
    }

    void solve() {
        if (moves.Count == 0)
            if (Solver.solve(world) is List<Dir> dirs) {
                Console.WriteLine($"solved in {dirs.Count} moves");
                moves = new(dirs);
            }
            else Console.WriteLine("no solution");

        if (moves.TryDequeue(out Dir dir))
            world.move(dir);
    }

    bool on_key_pressed(EventControllerKey sender, EventControllerKey.KeyPressedSignalArgs args) {
        switch (args.Keyval) {
            case KEY_Left: move((-1, 0)); break;
            case KEY_Right: move((1, 0)); break;
            case KEY_Up: move((0, -1)); break;
            case KEY_Down: move((0, 1)); break;
            case KEY_s: solve(); break;
        }
        return true;
    }
}

class Hello : Application {
    World world;

    public Hello(string level) : base([]) {
        world = new($"levels/{level}.sok");
        OnActivate += on_activate;
    }

    void on_activate(Gio.Application app, EventArgs args) {
        View w = new((Application) app, world);
        w.Show();
    }

    static void Main(string[] args) {
        if (args.Length != 1) {
            Console.WriteLine("usage: dotnet run <level>");
            return;
        }
        new Hello(args[0]).Run(args);
    }
}
