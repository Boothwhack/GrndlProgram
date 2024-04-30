using ConsoleUI;

namespace Application;

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