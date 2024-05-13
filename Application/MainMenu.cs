using ConsoleUI;

namespace Application;

class MainMenu(IFileSystem fs) : IMenu
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
            Title = "Valuta omregner",
            Description = """
                          Omregn mellem danske kroner og en
                          masse forskellige valutaer.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => new CurrencyMenu()
        },
        new Action
        {
            Title = "Udregn kontant",
            Description = """
                          Udregn hvad der skal betales
                          tilbage på et køb.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => new ChangeMenu()
        },
        new Action
        {
            Title = "Pensionsalder",
            Description = """
                          Find ud af hvornår du kan søge
                          om folkepension.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => new PensionMenu()
        },
        new Action
        {
            Title = "Persondatabase",
            Description = """
                          Database over personer, med
                          data gemt lokalt.
                          """,
            ButtonLabel = "Start",
            OnSelectedDelegate = () => PersonDatabaseMenu.LoadDatabaseMenu(fs)
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

    public Task Initialize(Screen screen)
    {
        _actionSelector = new SelectorWidget(_actions.Select(it => it.Title).ToArray());
        _actionBtn = new ButtonWidget();
        _actionDescription = new LabelWidget("", 0.0) { MinWidth = 40 };

        UpdateAction();

        screen.Elements =
        [
            new FrameWidget(
                new LayoutWidget([
                    new LabelWidget("Velkommen!\nNavigér med piletasten, og vælg\nmed Enter knappen.", 0.5),
                    new SpaceWidget(),
                    new LayoutWidget([
                        _actionSelector,
                        new SpaceWidget(3),
                        new LayoutWidget([
                            _actionDescription,
                            new SpaceWidget(),
                            _actionBtn
                        ], Vec2.Down, 0.0)
                    ], Vec2.Right)
                ], Vec2.Down),
                Vec2.One,
                new Vec2(0.5, 0.5))
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_actionSelector, _actionBtn]);

        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_actionBtn.Pressed)
                return _selectedAction.OnSelectedDelegate();

            UpdateAction();
            screen.RefreshScreen();
        }
    }
}