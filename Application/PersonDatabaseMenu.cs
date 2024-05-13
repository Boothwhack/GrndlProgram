using ConsoleUI;

namespace Application;

class ConfirmationMenu(
    ConfirmationMenu.OnResult onResult,
    string text,
    string confirmLabel = "Bekræft",
    string cancelLabel = "Annuller") : IMenu
{
    private ButtonWidget _yesBtn = new() { Label = confirmLabel };
    private ButtonWidget _noBtn = new() { Label = cancelLabel };

    public delegate void OnResult(bool confirmed);

    public Task Initialize(Screen screen)
    {
        screen.Elements =
        [
            new FrameWidget(
                new LayoutWidget([
                        new LabelWidget(text, 0.0),
                        new SpaceWidget(3),
                        new LayoutWidget(
                            [_yesBtn, new SpaceWidget(), _noBtn],
                            Vec2.Right
                        ),
                    ],
                    Vec2.Down),
                Vec2.One,
                new Vec2(0.5, 0.5)
            )
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_yesBtn, _noBtn]);
        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_yesBtn.Pressed)
            {
                onResult(true);
                return null;
            }

            if (_noBtn.Pressed)
            {
                onResult(false);
                return null;
            }

            screen.RefreshScreen();
        }
    }
}

class AlertMenu(string text) : IMenu
{
    public delegate IMenu? OnDismissCallback();

    private ButtonWidget _dismissBtn = new() { Label = "OK" };
    public OnDismissCallback OnDismiss = () => null;

    public Task Initialize(Screen screen)
    {
        screen.Elements =
        [
            new FrameWidget(
                new LayoutWidget([
                        new LabelWidget(text, 0.0),
                        _dismissBtn
                    ],
                    Vec2.Down,
                    1.0),
                Vec2.One,
                new Vec2(0.5, 0.5)
            )
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_dismissBtn]);

        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;
            if (_dismissBtn.Pressed) return OnDismiss();
        }
    }
}

class CreatePersonMenu(CreatePersonMenu.OnCreate onCreate) : IMenu
{
    public delegate void OnCreate(string name, DateOnly dateOfBirth);

    private ButtonWidget _createBtn = new() { Label = "Opret" };
    private ButtonWidget _cancelBtn = new() { Label = "Annuller" };

    private InputFieldWidget _nameField = new(_ => true, 30);

    private InputFieldWidget _dayField = new(char.IsDigit, 2, "1");
    private InputFieldWidget _monthField = new(char.IsDigit, 2, "1");
    private InputFieldWidget _yearField = new(char.IsDigit, 4, "1999");

    public Task Initialize(Screen screen)
    {
        screen.Elements =
        [
            new FrameWidget(
                new LayoutWidget([
                        new LayoutWidget([
                            new LabelWidget("Navn", 0.0) { MinWidth = 6 },
                            _nameField,
                        ], Vec2.Right),
                        new SpaceWidget(2),
                        new LabelWidget("Fødselsdag", 0.0),

                        new SpaceWidget(),
                        new LayoutWidget([
                            new LabelWidget("Dag", 0.0) { MinWidth = 6 },
                            _dayField
                        ], Vec2.Right),

                        new SpaceWidget(),
                        new LayoutWidget([
                            new LabelWidget("Måned", 0.0) { MinWidth = 6 },
                            _monthField
                        ], Vec2.Right),

                        new SpaceWidget(),
                        new LayoutWidget([
                            new LabelWidget("År", 0.0) { MinWidth = 6 },
                            _yearField
                        ], Vec2.Right),

                        new SpaceWidget(2),
                        _createBtn,
                        new SpaceWidget(),
                        _cancelBtn
                    ],
                    Vec2.Down, 0.0),
                Vec2.One, new Vec2(0.5, 0.5)
            ),
        ];
        screen.SetFocusOrder(FocusDirection.Vertical,
            [_nameField, _dayField, _monthField, _yearField, _createBtn, _cancelBtn]);
        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_cancelBtn.Pressed) return null;
            if (_createBtn.Pressed)
            {
                var name = _nameField.Contents.Trim();
                if (name.Length == 0)
                    return new AlertMenu("Navn kan ikke være tomt.");

                var day = int.Parse(_dayField.Contents);
                var month = int.Parse(_monthField.Contents);
                var year = int.Parse(_yearField.Contents);

                if (day is < 1 or > 31)
                    return new AlertMenu("Dag skal være et tal mellem 1 og 31.");
                if (month is < 1 or > 12)
                    return new AlertMenu("Måned skal være et tal mellem 1 og 12.");
                if (year is < 1 or > 9999)
                    return new AlertMenu("År skal være et tal mellem 1 og 9999.");

                onCreate(name, new DateOnly(year, month, day));
                return null;
            }

            screen.RefreshScreen();
        }
    }
}

class TableWidget : Widget, Focusable
{
    public PersonDatabase Database = new();

    private int _deleteRequest = -1;

    public int DeleteRequest
    {
        get
        {
            var request = _deleteRequest;
            _deleteRequest = -1;
            return request;
        }
    }

    private bool _createRequest = false;

    public bool CreateRequest
    {
        set => _createRequest = value;
        get
        {
            var request = _createRequest;
            _createRequest = false;
            return request;
        }
    }

    private int _selection = 0;

    private int Selection
    {
        get => _selection;
        set => _selection = Math.Clamp(value, 0, Database.Count);
    }

    public bool Focused { get; set; }

    public Vec2 Measure(Vec2 constraints) => constraints;

    public List<Drawable> Draw(Vec2 measured)
    {
        var idColumnWidth = measured.X * 0.2;
        var nameColumnWidth = measured.X * 0.5;
        var dobColumnWidth = measured.X * 0.3;

        var idX = 0.0;
        var nameX = idX + idColumnWidth;
        var dobX = nameX + nameColumnWidth;

        var drawables = new List<Drawable>
        {
            new DrawCall
            {
                Position = new Vec2(idX, 0),
                Output = "ID"
            },
            new DrawCall
            {
                Position = new Vec2(nameX, 0),
                Output = "Navn"
            },
            new DrawCall
            {
                Position = new Vec2(dobX, 0),
                Output = "Fødselsdag"
            }
        };

        var y = 0;
        foreach (var person in Database.Persons)
        {
            var selected = Focused && y == _selection;
            ConsoleColor? backgroundColor = selected
                ? ConsoleColor.White
                : y % 2 == 1
                    ? ConsoleColor.DarkGray
                    : null;
            ConsoleColor? foregroundColor = selected
                ? ConsoleColor.Black
                : y % 2 == 1
                    ? ConsoleColor.White
                    : null;
            drawables.Add(new DrawCall
            {
                BackgroundColor = backgroundColor,
                ForegroundColor = foregroundColor,
                Position = new Vec2(idX, 1 + y),
                Output = person.ID.ToString().PadRight((int)idColumnWidth)
            });
            drawables.Add(new DrawCall
            {
                BackgroundColor = backgroundColor,
                ForegroundColor = foregroundColor,
                Position = new Vec2(nameX, 1 + y),
                Output = person.Name.PadRight((int)nameColumnWidth)
            });
            drawables.Add(new DrawCall
            {
                BackgroundColor = backgroundColor,
                ForegroundColor = foregroundColor,
                Position = new Vec2(dobX, 1 + y),
                Output = person.DateOfBirth.ToShortDateString().PadRight((int)dobColumnWidth)
            });
            ++y;
        }

        bool newSelected = Focused && y == _selection;
        drawables.Add(new DrawCall
        {
            BackgroundColor = newSelected ? ConsoleColor.White : null,
            ForegroundColor = newSelected ? ConsoleColor.Black : null,
            Position = new Vec2(measured.X / 2.0, y + 1),
            Output = " Tilføj person ",
            HorizontalAlignment = 0.5
        });

        return drawables;
    }

    public bool OnKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.DownArrow:
                ++Selection;
                break;
            case ConsoleKey.UpArrow:
                --Selection;
                break;
            case ConsoleKey.Delete:
                if (Selection < Database.Count)
                {
                    var id = Database.Persons.ToList()[Selection].ID;
                    _deleteRequest = id;
                }

                break;
            case ConsoleKey.Enter:
                if (Selection == Database.Count)
                {
                    _createRequest = true;
                }

                break;
            default:
                return false;
        }

        return true;
    }

    public Vec2 Hotspot(Vec2 measured) => Vec2.Zero;
}

class PersonDatabaseMenu : IMenu
{
    private IFileSystem _fs;
    private ButtonWidget _backBtn = new() { Label = "\u2190" };
    private TableWidget _tableWidget;

    private const string DbPath = "person-db.txt";

    private static PersonDatabase DefaultDatabase
    {
        get
        {
            var database = new PersonDatabase();
            database.CreatePerson("Karoline Rasmussen", new DateOnly(1999, 12, 20));
            return database;
        }
    }

    private void SaveDatabase()
    {
        var writer = _fs.WriteFile(DbPath);
        PersonDatabase.WriteTo(_tableWidget.Database, writer);
        writer.Close();
    }

    public static IMenu LoadDatabaseMenu(IFileSystem fs)
    {
        var reader = fs.ReadFile(DbPath);
        try
        {
            var database = reader is null ? DefaultDatabase : PersonDatabase.ReadFrom(reader);
            return new PersonDatabaseMenu(fs, database);
        }
        catch (DatabaseParseException) // database has become corrupt
        {
            return new AlertMenu("Databasen er tilsyneladende korrupt.\nEn ny database vil blive skabt.")
            {
                OnDismiss = () =>
                {
                    // save database immediately to overwrite the corrupted one
                    var menu = new PersonDatabaseMenu(fs, DefaultDatabase);
                    menu.SaveDatabase();
                    return menu;
                }
            };
        }
    }

    public PersonDatabaseMenu(IFileSystem fs, PersonDatabase database)
    {
        _fs = fs;
        _tableWidget = new TableWidget { Database = database };
    }

    public Task Initialize(Screen screen)
    {
        screen.Elements =
        [
            new LayoutWidget(
                [_backBtn, _tableWidget],
                Vec2.Right,
                0.0
            ),
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_backBtn, _tableWidget]);
        screen.FocusElement(_tableWidget);

        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_backBtn.Pressed) return null;

            var delete = _tableWidget.DeleteRequest;
            if (delete != -1)
            {
                return new ConfirmationMenu(result =>
                    {
                        if (!result) return;
                        _tableWidget.Database.DeletePerson(delete);
                        SaveDatabase();
                    }, $"Er du sikker på at du vil\nslette bruger med id '{delete}'?");
            }

            if (_tableWidget.CreateRequest)
            {
                return new CreatePersonMenu((name, dateOfBirth) =>
                {
                    _tableWidget.Database.CreatePerson(name, dateOfBirth);
                    SaveDatabase();
                });
            }

            screen.RefreshScreen();
        }
    }
}