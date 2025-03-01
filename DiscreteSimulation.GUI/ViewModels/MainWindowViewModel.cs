using System;

namespace DiscreteSimulation.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isStartSimulationButtonEnabled = true;

    public bool IsStartSimulationButtonEnabled
    {
        get => _isStartSimulationButtonEnabled;
        set
        {
            _isStartSimulationButtonEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStopSimulationButtonEnabled));
        }
    }
    
    public bool IsStopSimulationButtonEnabled => !IsStartSimulationButtonEnabled;
    
    public bool IsCheckedStrategyA { get; set; } = true;

    private bool _isEnabledStrategyA = true;
    
    public bool IsEnabledStrategyA
    {
        get => _isEnabledStrategyA;
        set
        {
            _isEnabledStrategyA = value;
            OnPropertyChanged();
        }
    }

    public bool IsCheckedStrategyB { get; set; }
    
    private bool _isEnabledStrategyB = true;

    public bool IsEnabledStrategyB
    {
        get => _isEnabledStrategyB;
        set
        {
            _isEnabledStrategyB = value;
            OnPropertyChanged();
        }
    }

    public bool IsCheckedStrategyC { get; set; }
    
    private bool _isEnabledStrategyC = true;

    public bool IsEnabledStrategyC
    {
        get => _isEnabledStrategyC;
        set
        {
            _isEnabledStrategyC = value;
            OnPropertyChanged();
        }
    }

    public bool IsCheckedStrategyD { get; set; }
    
    private bool _isEnabledStrategyD = true;

    public bool IsEnabledStrategyD
    {
        get => _isEnabledStrategyD;
        set
        {
            _isEnabledStrategyD = value;
            OnPropertyChanged();
        }
    }

    public bool IsCheckedCustomStrategy { get; set; }

    public bool IsEnabledCustomStrategy
    {
        get => _isEnabledCustomStrategy;
        set
        {
            _isEnabledCustomStrategy = value;
            OnPropertyChanged();
        }
    }
    
    public string GetSelectedStrategy()
    {
        if (IsCheckedStrategyA)
        {
            return "A";
        }
        
        if (IsCheckedStrategyB)
        {
            return "B";
        }
        
        if (IsCheckedStrategyC)
        {
            return "C";
        }
        
        if (IsCheckedStrategyD)
        {
            return "D";
        }

        return "Custom";
    }

    public void DisableButtonsForSimulationStart()
    {
        IsStartSimulationButtonEnabled = false;
        
        if (!IsCheckedStrategyA)
        {
            IsEnabledStrategyA = false;
        }
        
        if (!IsCheckedStrategyB)
        {
            IsEnabledStrategyB = false;
        }
        
        if (!IsCheckedStrategyC)
        {
            IsEnabledStrategyC = false;
        }
        
        if (!IsCheckedStrategyD)
        {
            IsEnabledStrategyD = false;
        }
        
        if (!IsCheckedCustomStrategy)
        {
            IsEnabledCustomStrategy = false;
        }
    }

    public void EnableButtonsForSimulationEnd()
    {
        IsStartSimulationButtonEnabled = true;
        IsEnabledStrategyA = true;
        IsEnabledStrategyB = true;
        IsEnabledStrategyC = true;
        IsEnabledStrategyD = true;
        IsEnabledCustomStrategy = true;
    }

    private long _replications = 2_000_000;
    
    public long Replications
    {
        get => _replications;
        set
        {
            if (value == _replications) return;
            _replications = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    private long _skipFirstNReplicationsInPercent = 0;
    
    public long SkipFirstNReplicationsInPercent
    {
        get => _skipFirstNReplicationsInPercent;
        set
        {
            if (value == _skipFirstNReplicationsInPercent) return;
            _skipFirstNReplicationsInPercent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    private long _renderPoints = 1000;
    
    public long RenderPoints
    {
        get => _renderPoints;
        set
        {
            _renderPoints = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RenderOffset));
        }
    }

    public long RenderOffset
    {
        get
        {
            var skipFirst = Math.Floor(Replications * (SkipFirstNReplicationsInPercent / 100.0));
            var renderOffset = (Replications - skipFirst) / RenderPoints;
            return Convert.ToInt64(Math.Floor(renderOffset)) - 1;
        }
    }
    
    private bool _isEnabledCustomStrategy = true;
    
    private string _currentCosts = "?";

    public string CurrentCosts
    {
        get => $"Náklady = {_currentCosts} €";
        set
        {
            _currentCosts = value;
            OnPropertyChanged();
        }
    }
    
    private string _currentReplication = "?";

    public string CurrentReplication
    {
        get => $"Replikácia = {_currentReplication}";
        set
        {
            _currentReplication = value;
            OnPropertyChanged();
        }
    }
}