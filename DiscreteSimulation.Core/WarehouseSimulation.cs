using DiscreteSimulation.Core.Generators;
using DiscreteSimulation.Core.MonteCarlo;

namespace DiscreteSimulation.Core;

public class WarehouseSimulation : MonteCarloSimulationCore
{
    private double _replicationsCostsSum;

    public double CurrentCosts => _replicationsCostsSum / CurrentReplication;

    private DiscreteUniformGenerator _requiredCountOfTlmiceGenerator;
    private DiscreteUniformGenerator _requiredCountOfBrzdoveDostickyGenerator;
    private EmpiricalProbabilityGenerator _requiredCountOfSvetlometyGenerator;
    
    private ContinuousUniformGenerator _deliveryProbabilityBySupplier1First10WeeksGenerator;
    private ContinuousUniformGenerator _deliveryProbabilityBySupplier1After10WeeksGenerator;
    
    private EmpiricalProbabilityGenerator _deliveryProbabilityBySupplier2First15WeeksGenerator;
    private EmpiricalProbabilityGenerator _deliveryProbabilityBySupplier2After15WeeksGenerator;
    
    private ContinuousUniformGenerator _decisionOnDeliveryGenerator;
    
    public string SelectedStrategy
    {
        get => _selectedStrategy;
        set {
            if (IsSimulationRunning)
            {
                throw new Exception("Simulation is running");
            }
            
            _selectedStrategy = value;
        }
    }

    private string _selectedStrategy = "A";
    
    private Action<int> _deliverComponentsBySelectedStrategy;

    private int[] _deliveredComponents;

    public override void BeforeSimulation(int? seedForSeedGenerator = null)
    {
        base.BeforeSimulation(seedForSeedGenerator);
        
        _replicationsCostsSum = 0;
        
        _deliveredComponents = new int[3];
        
        if (SelectedStrategy == "A")
        {
            _deliverComponentsBySelectedStrategy = StrategyA;
        } 
        else if (SelectedStrategy == "B")
        {
            _deliverComponentsBySelectedStrategy = StrategyB;
        }
        else if (SelectedStrategy == "C")
        {
            _deliverComponentsBySelectedStrategy = StrategyC;
        }
        else if (SelectedStrategy == "D")
        {
            _deliverComponentsBySelectedStrategy = StrategyD;
        }
        else if (SelectedStrategy == "Custom")
        {
            _deliverComponentsBySelectedStrategy = CustomStrategy;
        }
        else
        {
            throw new Exception("Unknown strategy");
        }
        
        _requiredCountOfTlmiceGenerator = new DiscreteUniformGenerator(50, 101, SeedGenerator.Next());
        _requiredCountOfBrzdoveDostickyGenerator = new DiscreteUniformGenerator(60, 251, SeedGenerator.Next());
        _requiredCountOfSvetlometyGenerator = new EmpiricalProbabilityGenerator(
            isDiscrete: true, 
            [
                new EmpiricalProbabilityTableItem(30, 60, 0.2),
                new EmpiricalProbabilityTableItem(60, 100, 0.4),
                new EmpiricalProbabilityTableItem(100, 140, 0.3),
                new EmpiricalProbabilityTableItem(140, 160, 0.1)
            ],
            SeedGenerator
        );

        _deliveryProbabilityBySupplier1First10WeeksGenerator = new ContinuousUniformGenerator(10, 70, SeedGenerator.Next());
        _deliveryProbabilityBySupplier1After10WeeksGenerator = new ContinuousUniformGenerator(30, 95, SeedGenerator.Next());
        
        _deliveryProbabilityBySupplier2First15WeeksGenerator = new EmpiricalProbabilityGenerator(
            isDiscrete: false,
            [
                new EmpiricalProbabilityTableItem(5, 10, 0.4),
                new EmpiricalProbabilityTableItem(10, 50, 0.3),
                new EmpiricalProbabilityTableItem(50, 70, 0.2),
                new EmpiricalProbabilityTableItem(70, 80, 0.06),
                new EmpiricalProbabilityTableItem(80, 95, 0.04),
            ],
            SeedGenerator
        );
        _deliveryProbabilityBySupplier2After15WeeksGenerator = new EmpiricalProbabilityGenerator(
            isDiscrete: false,
            [
                new EmpiricalProbabilityTableItem(5, 10, 0.2),
                new EmpiricalProbabilityTableItem(10, 50, 0.4),
                new EmpiricalProbabilityTableItem(50, 70, 0.3),
                new EmpiricalProbabilityTableItem(70, 80, 0.06),
                new EmpiricalProbabilityTableItem(80, 95, 0.04),
            ],
            SeedGenerator
        );
        
        _decisionOnDeliveryGenerator = new ContinuousUniformGenerator(0, 100, SeedGenerator.Next());
    }

    public override void ExecuteReplication()
    {
        var availableComponents = new int[3];
        var costs = 0.0;
        
        for (int week = 1; week <= 30; week++)
        {
            _deliverComponentsBySelectedStrategy(week);
            
            availableComponents[0] += _deliveredComponents[0];
            availableComponents[1] += _deliveredComponents[1];
            availableComponents[2] += _deliveredComponents[2];
            
            // Náklady na súčiastky v dňoch pondelok až štvrtok
            costs += 0.2 * 4 * availableComponents[0];
            costs += 0.3 * 4 * availableComponents[1];
            costs += 0.25 * 4 * availableComponents[2];

            // TODO: Horna hranica U_max = 100 je vratane 100 alebo menej ako 100?
            var requiredCountOfTlmice = _requiredCountOfTlmiceGenerator.Next();
            // TODO: Rovnaka otazka
            var requiredCountOfBrzdoveDosticky = _requiredCountOfBrzdoveDostickyGenerator.Next();
            var requiredCountOfSvetlomety = _requiredCountOfSvetlometyGenerator.Next();
            
            availableComponents[0] -= requiredCountOfTlmice;
            
            if (availableComponents[0] < 0)
            {
                costs += 0.3 * 4 * -availableComponents[0];
                availableComponents[0] = 0;
            }
            
            availableComponents[1] -= requiredCountOfBrzdoveDosticky;
            
            if (availableComponents[1] < 0)
            {
                costs += 0.3 * 4 * -availableComponents[1];
                availableComponents[1] = 0;
            }
            
            availableComponents[2] -= Convert.ToInt32(requiredCountOfSvetlomety);
            
            if (availableComponents[2] < 0)
            {
                costs += 0.3 * 4 * -availableComponents[2];
                availableComponents[2] = 0;
            }
            
            // Náklady na súčiastky v piatok až nedeľu
            costs += 0.2 * 3 * availableComponents[0];
            costs += 0.3 * 3 * availableComponents[1];
            costs += 0.25 * 3 * availableComponents[2];
        }
        
        _replicationsCostsSum += costs;
    }
    
    // Stratégia A: Dodáva iba dodávatel 1
    private void StrategyA(int week)
    {
        double probabilityOfDelivery;
            
        if (week <= 10)
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier1First10WeeksGenerator.Next();
        }
        else
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier1After10WeeksGenerator.Next();
        }

        var decision = _decisionOnDeliveryGenerator.Next();
                
        if (decision < probabilityOfDelivery)
        {
            _deliveredComponents[0] = 100;
            _deliveredComponents[1] = 200;
            _deliveredComponents[2] = 150;
        }
        else
        {
            _deliveredComponents[0] = 0;
            _deliveredComponents[1] = 0;
            _deliveredComponents[2] = 0;
        }
    }
    
    // Stratégia B: Dodáva iba dodávatel 2
    private void StrategyB(int week)
    {
        double probabilityOfDelivery;
            
        if (week <= 15)
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier2First15WeeksGenerator.Next();
        }
        else
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier2After15WeeksGenerator.Next();
        }

        var decision = _decisionOnDeliveryGenerator.Next();
                
        if (decision < probabilityOfDelivery)
        {
            _deliveredComponents[0] = 100;
            _deliveredComponents[1] = 200;
            _deliveredComponents[2] = 150;
        }
        else
        {
            _deliveredComponents[0] = 0;
            _deliveredComponents[1] = 0;
            _deliveredComponents[2] = 0;
        }
    }
    
    // Stratégia C: Kazdy neparny tyzden dodava dodavatel 1, kazdy parny dodavatel 2
    private void StrategyC(int week)
    {
        if (week % 2 == 1)
        {
            StrategyA(week);
        }
        else
        {
            StrategyB(week);
        }
    }
    
    // Stratégia D: Kazdy neparny tyzden dodava dodavatel 2, kazdy parny dodavatel 1
    private void StrategyD(int week)
    {
        if (week % 2 == 1)
        {
            StrategyB(week);
        }
        else
        {
            StrategyA(week);
        }
    }
    
    // Vlastná stratégia: Objednavame iba od dodavatela 1, ale mnozstvo objednaneho tovaru
    // je zmenené na stredné hodnoty jednotlivych distribucii odoberaneho tovaru
    private void CustomStrategy(int week)
    {
        double probabilityOfDelivery;
            
        if (week <= 10)
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier1First10WeeksGenerator.Next();
        }
        else
        {
            probabilityOfDelivery = _deliveryProbabilityBySupplier1After10WeeksGenerator.Next();
        }

        var decision = _decisionOnDeliveryGenerator.Next();
                
        if (decision < probabilityOfDelivery)
        {
            _deliveredComponents[0] = 75;
            _deliveredComponents[1] = 155;
            _deliveredComponents[2] = 91;
        }
        else
        {
            _deliveredComponents[0] = 0;
            _deliveredComponents[1] = 0;
            _deliveredComponents[2] = 0;
        }
    }
}
