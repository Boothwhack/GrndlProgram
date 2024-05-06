using System.Diagnostics;
using ConsoleUI;

namespace Application;

struct PensionCutoff()
{
    public DateTime Date = default;
    public int AgeYears = 0;
    public int AgeMonths = 0;
}

class PensionMenu : IMenu
{
    private const string InvalidDateError = "Kunne ikke forstå den indtastede dato.\nPrøv igen.";

    private List<PensionCutoff> _cutoffs =
    [
        new() { Date = new(1955, 6, 30), AgeYears = 65 },
        new() { Date = new(1962, 12, 31), AgeYears = 67 },
        new() { Date = new(1966, 12, 31), AgeYears = 68 },
        new() { Date = new(1970, 12, 31), AgeYears = 69 },
        new() { Date = new(1974, 12, 31), AgeYears = 70 },
        new() { Date = new(1978, 12, 31), AgeYears = 71 },
        new() { Date = new(1982, 12, 31), AgeYears = 72 },
        new() { Date = new(1987, 6, 30), AgeYears = 72, AgeMonths = 6 },
        new() { Date = new(1991, 12, 31), AgeYears = 73 },
        new() { Date = new(1996, 6, 30), AgeYears = 73, AgeMonths = 6 },

        // final catch-all cutoff
        new() { Date = DateTime.MaxValue, AgeYears = 74 },
    ];

    private ButtonWidget _backBtn;
    private InputFieldWidget _dayField;
    private SelectorWidget _monthSelector;
    private InputFieldWidget _yearField;
    private LabelWidget _outputLabel;

    private PensionCutoff FindPensionCutoff(DateTime dateOfBirth) => _cutoffs.Find(it => dateOfBirth < it.Date);

    private void UpdateOutput()
    {
        if (!int.TryParse(_dayField.Contents, out var day))
        {
            _outputLabel.Label = InvalidDateError;
            return;
        }

        if (!int.TryParse(_yearField.Contents, out var year))
        {
            _outputLabel.Label = InvalidDateError;
            return;
        }

        var month = _monthSelector.Selection + 1;

        var date = new DateTime(year, month, day);
        var cutoff = FindPensionCutoff(date);

        var pensionDate = date.AddYears(cutoff.AgeYears).AddMonths(cutoff.AgeMonths);
        var applyDate = pensionDate.AddDays(-pensionDate.Day).AddMonths(-6);
        var firstPayment = pensionDate.AddDays(-pensionDate.Day + 1).AddMonths(1);

        _outputLabel.Label = $"Din pensionsalder er\n{cutoff.AgeYears} år";
        if (cutoff.AgeMonths > 0)
            _outputLabel.Label += $" og {cutoff.AgeMonths} måneder";
        _outputLabel.Label += $".\nDu kan ansøge d. {applyDate.ToShortDateString()}";
        _outputLabel.Label += $".\nDu får udbetalt d. {firstPayment.ToShortDateString()}";
    }

    public Task Initialize(Screen screen)
    {
        _backBtn = new ButtonWidget { Label = "\u2190" };
        _dayField = new InputFieldWidget(char.IsDigit, 2, "20");
        _monthSelector = new SelectorWidget([
            "Januar",
            "Februar",
            "Marts",
            "April",
            "Maj",
            "Juni",
            "Juli",
            "August",
            "September",
            "Oktober",
            "November",
            "December"
        ]) { Selection = 11 };
        _yearField = new InputFieldWidget(char.IsDigit, 4, "1999");
        _outputLabel = new LabelWidget("", 0.0);

        screen.Elements =
        [
            _backBtn,
            new FrameWidget(
                new LayoutWidget([
                    _dayField,
                    new SpaceWidget(),
                    _monthSelector,
                    new SpaceWidget(),
                    _yearField,
                    new SpaceWidget(4),
                    _outputLabel,
                ], Vec2.Right),
                Vec2.One, new Vec2(0.5, 0.5)),
        ];
        screen.SetFocusOrder(FocusDirection.Horizontal, [_backBtn, _dayField, _monthSelector, _yearField]);

        UpdateOutput();

        return Task.CompletedTask;
    }

    public async Task<IMenu?> Run(Screen screen)
    {
        while (true)
        {
            if (!await screen.HandleInput()) continue;

            if (_backBtn.Pressed) break;

            UpdateOutput();
            screen.RefreshScreen();
        }

        return null;
    }
}