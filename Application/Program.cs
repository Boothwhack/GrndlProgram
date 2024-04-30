namespace Application;

using ConsoleUI;

interface IMenu
{
    void Initialize(Screen screen);

    IMenu? Run(Screen screen);
}

class MainMenu : IMenu
{
    struct Action
    {
        public string Title;
        public string Description;
        public string ButtonLabel;
        public OnSelected OnSelectedDelegate;

        public delegate IMenu? OnSelected();
    }

    private readonly List<Action> _actions =
    [
        new Action
        {
            Title = "Konverter temperatur",
            Description = """
                          Konverter mellem flere forskellige
                          temperatur enheder.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => new TemperatureMenu()
        },
        new Action
        {
            Title = "Exit",
            Description = """
                          Exit programmet. Farvel og tak for
                          i dag.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => null
        }
    ];

    private ButtonWidget _actionBtn;
    private SelectorWidget _actionSelector;
    private LabelWidget _actionDescription;

    private Action _selectedAction => _actions[_actionSelector.Selection];

    private void UpdateAction()
    {
        _actionBtn.Label = _selectedAction.ButtonLabel;
        _actionDescription.Label = _selectedAction.Description;
    }

    public void Initialize(Screen screen)
    {
        _actionSelector = new SelectorWidget(_actions.Select(it => it.Title).ToArray());
        _actionBtn = new ButtonWidget();
        _actionDescription = new LabelWidget("", 0.0);

        UpdateAction();

        screen.Elements =
        [
            new FrameWidget(new LayoutWidget([
                    _actionSelector,
                    new SpaceWidget(3),
                    new LayoutWidget([
                        _actionDescription,
                        new SpaceWidget(),
                        _actionBtn
                    ], Vec2.Down, 0.0)
                ], Vec2.Right), Vec2.One,
                new Vec2(0.5, 0.5))
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_actionSelector, _actionBtn]);
    }

    public IMenu? Run(Screen screen)
    {
        while (true)
        {
            if (!screen.HandleInput()) continue;

            if (_actionBtn.Pressed)
                return _selectedAction.OnSelectedDelegate();

            UpdateAction();
            screen.RefreshScreen();
        }
    }
}

class TemperatureMenu : IMenu
{
    enum TemperatureUnit
    {
        Celsius,
        Fahrenheit,
        Kelvin,
        Réaumur,
    }

    double FromCelsius(double temperature, TemperatureUnit unit)
    {
        switch (unit)
        {
            case TemperatureUnit.Celsius:
                return temperature;
            case TemperatureUnit.Fahrenheit:
                return (temperature * 9 / 5) + 32;
            case TemperatureUnit.Kelvin:
                return temperature + 273.15;
            case TemperatureUnit.Réaumur:
                return temperature * 0.8;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, "Ukendt TemperatureUnit");
        }
    }

    private ButtonWidget _backBtn;
    private InputFieldWidget _inputField;
    private LabelWidget _outputLabel;
    private SelectorWidget _outputSelector;

    private void UpdateOutputLabel()
    {
        var input = _inputField.Contents;
        if (!double.TryParse(input, out var inputTemperature))
        {
            _outputLabel.Label = "N/A";
            return;
        }

        TemperatureUnit? outputUnit = _outputSelector.Selection switch
        {
            0 => TemperatureUnit.Celsius,
            1 => TemperatureUnit.Kelvin,
            2 => TemperatureUnit.Fahrenheit,
            3 => TemperatureUnit.Réaumur,
            _ => null
        };
        if (outputUnit is null)
        {
            _outputLabel.Label = "N/A";
            return;
        }

        var output = FromCelsius(inputTemperature, (TemperatureUnit)outputUnit);
        _outputLabel.Label = output.ToString("0.##") + ((TemperatureUnit)outputUnit) switch
        {
            TemperatureUnit.Celsius => " \u00b0C",
            TemperatureUnit.Fahrenheit => " \u00b0F",
            TemperatureUnit.Kelvin => " \u00b0K",
            TemperatureUnit.Réaumur => " \u00b0Ré",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Initialize(Screen screen)
    {
        _backBtn = new ButtonWidget { Label = "\u2190" };
        _inputField = new InputFieldWidget(c => char.IsDigit(c) || c == '-', 4, "17");
        _outputLabel = new LabelWidget("", 0.0) { MinWidth = 10 };
        _outputSelector = new SelectorWidget(["Celsius", "Kelvin", "Fahrenheit", "Réaumur"])
        {
            Selection = 2
        };

        screen.Elements =
        [
            _backBtn,
            new FrameWidget(
                new LayoutWidget([
                    _inputField,
                    new SpaceWidget(),
                    new LabelWidget("\u00b0C", 0.0),
                    new SpaceWidget(2),
                    new LabelWidget("\u2192", 0.0),
                    new SpaceWidget(2),
                    _outputSelector,
                    new SpaceWidget(3),
                    _outputLabel
                ], Vec2.Right, 0.6),
                Vec2.One,
                new Vec2(0.5, 0.5)
            )
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_backBtn, _inputField, _outputSelector]);
        screen.FocusElement(_inputField);

        UpdateOutputLabel();
    }

    public IMenu? Run(Screen screen)
    {
        while (true)
        {
            if (!screen.HandleInput()) continue;

            if (_backBtn.Pressed) return null;

            UpdateOutputLabel();
            screen.RefreshScreen();
        }
    }
}

class CalculatorMenu : IMenu
{
    public void Initialize(Screen screen)
    {
        throw new NotImplementedException();
    }

    public IMenu? Run(Screen screen)
    {
        throw new NotImplementedException();
    }
}

class Application
{
    private Screen _screen;

    public Application()
    {
        _screen = new Screen([]);
    }

    public void Run()
    {
        _screen.DrawThread().Start();
        PushMenu(new MainMenu());
        _screen.StopDrawing();
    }

    void PushMenu(IMenu menu)
    {
        while (true)
        {
            menu.Initialize(_screen);
            _screen.RefreshScreen();

            var nextMenu = menu.Run(_screen);
            if (nextMenu is null)
                break;

            PushMenu(nextMenu);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var application = new Application();
        application.Run();
    }
}