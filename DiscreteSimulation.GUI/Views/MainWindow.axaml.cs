using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscreteSimulation.Core;
using DiscreteSimulation.GUI.ViewModels;
using ScottPlot;
using ScottPlot.AutoScalers;

namespace DiscreteSimulation.GUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly WarehouseSimulation _simulation = new();
    private readonly List<Coordinates> _chartData = new();
    private int _skipFirstNReplications = 0;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        var scatterLine = CostsPlot.Plot.Add.ScatterLine(_chartData, Colors.Red);
        scatterLine.PathStrategy = new ScottPlot.PathStrategies.CubicSpline();
        CostsPlot.Plot.Axes.AutoScaler = new FractionalAutoScaler(.005, .015);


        _simulation.ReplicationEnded += ReplicationEnded;

        _simulation.SimulationEnded += () =>
        {
            Dispatcher.UIThread.Post(() => _viewModel.EnableButtonsForSimulationEnd());
        };
    }

    private void ReplicationEnded()
    {
        var currentReplication = _simulation.CurrentReplication;
        
        // Berieme iba kazdu (RenderOffset + 1)-tu replikaciu
        if ((currentReplication - _skipFirstNReplications) % (_viewModel.RenderOffset + 1) != 0)
        {
            return;
        }
        
        var currentCosts = _simulation.CurrentCosts;
        
        Dispatcher.UIThread.Post(() => NewReplicationResult(currentReplication, currentCosts));
        
        // Preskocime prvych niekolko replikacii
        if (currentReplication < _skipFirstNReplications)
        {
            return;
        }
        
        Dispatcher.UIThread.Post(() => NewCostValue(currentReplication, currentCosts));
    }

    private async void StartSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.DisableButtonsForSimulationStart();
        
        _chartData.Clear();
        CostsPlot.Plot.Axes.AutoScale();
        CostsPlot.Refresh();
        
        _skipFirstNReplications = Convert.ToInt32(_viewModel.Replications * (_viewModel.SkipFirstNReplicationsInPercent / 100.0));
        
        _simulation.SelectedStrategy = _viewModel.GetSelectedStrategy();
        
        await Task.Run(() => _simulation.StartSimulation(_viewModel.Replications));
    }
    
    private void NewReplicationResult(long replication, double costs)
    {
        _viewModel.CurrentReplication = replication.ToString();
        _viewModel.CurrentCosts = costs.ToString();
    }
    
    private void NewCostValue(long replication, double costs)
    {
        _chartData.Add(new Coordinates(replication, costs));

        CostsPlot.Plot.Axes.AutoScale();
        CostsPlot.Refresh();
    }

    private void StopSimulationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _simulation.StopSimulation();
    }
}
