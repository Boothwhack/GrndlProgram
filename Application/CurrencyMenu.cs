using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleUI;

namespace Application;

struct Currency
{
    public string Label;
    public double Rate;
}

class CurrencyRatesResponse
{
    [JsonPropertyName("rates")]
    public Dictionary<string, double> Rates { get; set; }
}

[JsonSerializable(typeof(CurrencyRatesResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class SourceGenerationContext : JsonSerializerContext;

class CurrencyMenu : IMenu
{
    private ButtonWidget _exitBtn;
    private InputFieldWidget _inputField;
    private SelectorWidget _currencySelector;
    private LabelWidget _outputLabel;
    private Dictionary<string, Currency> _currencies;

    private void UpdateConversion()
    {
        var currencyCode = _currencySelector.Options[_currencySelector.Selection];
        var currency = _currencies[currencyCode];
        if (!double.TryParse(_inputField.Contents, out var input))
        {
            _outputLabel.Label = "N/A";
            return;
        }

        var output = currency.Rate * input;
        _outputLabel.Label = output.ToString("0.##") + "\n" + currency.Label;
    }

    private void ErrorScreen(Screen screen)
    {
        _exitBtn = new ButtonWidget { Label = "Afslut" };

        screen.Elements =
        [
            new FrameWidget(new LayoutWidget([
                        new LabelWidget("""
                                        Det opstod en fejl under indlæsning
                                        af valuta kurser.
                                        Prøv igen senere.
                                        """,
                            0.5),
                        new SpaceWidget(),
                        _exitBtn,
                    ],
                    Vec2.Down
                ),
                Vec2.One, new Vec2(0.5, 0.5)),
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_exitBtn]);
    }

    public async Task Initialize(Screen screen)
    {
        screen.Elements = [new FrameWidget(new LabelWidget("Indlæser...", 0.5), Vec2.One, new Vec2(0.5, 0.5))];
        screen.SetFocusOrder(FocusDirection.Horizontal, []);
        screen.RefreshScreen();

        var jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = SourceGenerationContext.Default
        };

        var httpClient = new HttpClient();
        var ratesResponse = await httpClient.GetAsync("https://api.frankfurter.app/latest?from=DKK");
        if (ratesResponse.StatusCode != HttpStatusCode.OK)
        {
            ErrorScreen(screen);
            return;
        }

        var rates = await ratesResponse.Content.ReadFromJsonAsync<CurrencyRatesResponse>(jsonOptions);
        if (rates is null)
        {
            ErrorScreen(screen);
            return;
        }

        var currenciesResponse = await httpClient.GetAsync("https://api.frankfurter.app/currencies");
        if (currenciesResponse.StatusCode != HttpStatusCode.OK)
        {
            ErrorScreen(screen);
            return;
        }

        var currencyNames = await currenciesResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>(jsonOptions);
        if (currencyNames is null)
        {
            ErrorScreen(screen);
            return;
        }

        _currencies = new Dictionary<string, Currency>();
        foreach (var (key, rate) in rates.Rates)
        {
            var name = currencyNames[key];
            _currencies[key] = new Currency { Label = name, Rate = rate };
        }

        _inputField = new InputFieldWidget(char.IsDigit, 10, "100");
        _currencySelector = new SelectorWidget(_currencies.Keys.ToArray());
        _currencySelector.Selection = Array.IndexOf(_currencySelector.Options, "USD");
        _outputLabel = new LabelWidget("", 0.0) { MinWidth = 24 };
        _exitBtn = new ButtonWidget { Label = "\u2190" };

        screen.Elements =
        [
            _exitBtn,
            new FrameWidget(new LayoutWidget([
                _inputField,
                new SpaceWidget(),
                new LabelWidget("DKK", 0.0),
                new SpaceWidget(2),
                new LabelWidget("\u2192", 0.0),
                new SpaceWidget(2),
                _currencySelector,
                new SpaceWidget(3),
                _outputLabel
            ], Vec2.Right, 0.6), Vec2.One, new Vec2(0.5, 0.5))
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_exitBtn, _inputField, _currencySelector]);
        screen.FocusElement(_inputField);

        UpdateConversion();
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_exitBtn.Pressed) return null;

            UpdateConversion();
            screen.RefreshScreen();
        }
    }
}