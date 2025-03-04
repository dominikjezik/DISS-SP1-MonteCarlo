using System;
using System.Collections.Generic;

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
    
    public Dictionary<string, string> StrategiesOptions { get; set; } = new()
    {
        { "A", "Strategy A" },
        { "B", "Strategy B" },
        { "C", "Strategy C" },
        { "D", "Strategy D" },
        { "Custom1", "Custom Strategy 1" },
        { "Custom2", "Custom Strategy 2" },
        { "Custom3", "Custom Strategy 3" }
    };
    
    public string SelectedStrategy { get; set; } = "A";
    
    private bool _isEnabledStrategySelector = true;

    public bool IsEnabledStrategySelector
    {
        get => _isEnabledStrategySelector;
        set
        {
            _isEnabledStrategySelector = value;
            OnPropertyChanged();
        }
    }

    public void DisableButtonsForSimulationStart()
    {
        IsStartSimulationButtonEnabled = false;
        IsEnabledStrategySelector = false;
    }

    public void EnableButtonsForSimulationEnd()
    {
        IsStartSimulationButtonEnabled = true;
        IsEnabledStrategySelector = true;
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
    
    private string _currentCosts = "? ";

    public string CurrentCosts
    {
        get => $"{_currentCosts}€";
        set
        {
            _currentCosts = value;
            OnPropertyChanged();
        }
    }
    
    private string _currentReplication = "?";

    public string CurrentReplication
    {
        get => _currentReplication;
        set
        {
            _currentReplication = value;
            OnPropertyChanged();
        }
    }

    private bool _isSingleReplication = false;
    
    public bool IsSingleReplication
    {
        get => _isSingleReplication;
        set
        {
            _isSingleReplication = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMultipleReplications));
        }
    }
    
    public bool IsMultipleReplications => !IsSingleReplication;
}
