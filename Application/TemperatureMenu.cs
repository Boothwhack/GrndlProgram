using ConsoleUI;

namespace Application;

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

    public Task Initialize(Screen screen)
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

        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_backBtn.Pressed) return null;

            UpdateOutputLabel();
            screen.RefreshScreen();
        }
    }
}