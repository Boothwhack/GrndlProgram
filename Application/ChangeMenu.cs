using ConsoleUI;

namespace Application;

class ChangeMenu : IMenu
{
    private ButtonWidget _exitBtn;
    private InputFieldWidget _inputField;
    private LabelWidget _outputLabel;
    private SelectorWidget _currencySelector;

    private Dictionary<string, ChangeCurrency> _currencies = new()
    {
        {
            "DKK", new ChangeCurrency
            {
                Label = "DKK", Units =
                [
                    new Unit { Label = "500 kr. seddel", Cents = 500_00 },
                    new Unit { Label = "200 kr. seddel", Cents = 200_00 },
                    new Unit { Label = "100 kr. seddel", Cents = 100_00 },
                    new Unit { Label = "50 kr. seddel", Cents = 50_00 },
                    new Unit { Label = "20'er", Cents = 20_00 },
                    new Unit { Label = "10'er", Cents = 10_00 },
                    new Unit { Label = "5'er", Cents = 5_00 },
                    new Unit { Label = "2 krone", Cents = 2_00 },
                    new Unit { Label = "1 krone", Cents = 1_00 },
                    new Unit { Label = "50 øre", Cents = 50 },
                ]
            }
        },
        {
            "USD", new ChangeCurrency
            {
                Label = "USD", Units =
                [
                    new Unit { Label = "Benjamin (100$)", Cents = 100_00 },
                    new Unit { Label = "Grant (50$)", Cents = 50_00 },
                    new Unit { Label = "Jackson (20$)", Cents = 20_00 },
                    new Unit { Label = "Hamilton (10$)", Cents = 10_00 },
                    new Unit { Label = "Lincoln (5$)", Cents = 5_00 },
                    new Unit { Label = "Washington (1$)", Cents = 1_00 },
                    new Unit { Label = "50\u00a2", Cents = 50 },
                    new Unit { Label = "quarter", Cents = 25 },
                    new Unit { Label = "dime", Cents = 10 },
                    new Unit { Label = "nickel", Cents = 5 },
                    new Unit { Label = "penny", Cents = 1 },
                ]
            }
        },
    };

    struct Unit
    {
        public string Label;
        public int Cents;
    }

    struct ChangeCurrency
    {
        public List<Unit> Units;
        public string Label;
    }

    struct UnitAmount
    {
        public Unit Unit;
        public int Amount;
    }

    private void UpdateChange()
    {
        if (!double.TryParse(_inputField.Contents, out var input))
        {
            _outputLabel.Label = "N/A";
            return;
        }

        var currencyCode = _currencySelector.Options[_currencySelector.Selection];
        var currency = _currencies[currencyCode];
        var change = CalculateChange(input, currency);

        var output = change.Select(it => $"{it.Amount} x {it.Unit.Label}").Aggregate((a, b) => a + "\n" + b);
        _outputLabel.Label = output;
    }

    List<UnitAmount> CalculateChange(double amount, ChangeCurrency currency)
    {
        int cents = (int)(amount * 100);
        List<UnitAmount> output = [];
        foreach (var unit in currency.Units)
        {
            var amountOfUnit = cents / unit.Cents;
            if (amountOfUnit > 0)
                output.Add(new UnitAmount { Amount = amountOfUnit, Unit = unit });

            cents %= unit.Cents;
            if (cents == 0) break;
        }

        return output;
    }

    public Task Initialize(Screen screen)
    {
        _inputField = new InputFieldWidget(c => char.IsDigit(c) || c == ',', contents: "139,50");
        _outputLabel = new LabelWidget("", 0.0);
        _currencySelector = new SelectorWidget(_currencies.Keys.ToArray());
        _exitBtn = new ButtonWidget { Label = "\u2190" };

        screen.Elements =
        [
            _exitBtn,
            new FrameWidget(
                new LayoutWidget([
                    _inputField,
                    new SpaceWidget(2),
                    _currencySelector,
                    new SpaceWidget(5),
                    _outputLabel
                ], Vec2.Right, 0.5),
                Vec2.One,
                new Vec2(0.5, 0.5)
            )
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_exitBtn, _inputField, _currencySelector]);
        screen.FocusElement(_inputField);

        UpdateChange();
        
        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_exitBtn.Pressed) return null;

            UpdateChange();
            screen.RefreshScreen();
        }
    }
}