namespace DiscreteSimulation.Core.Generators;

public class EmpiricalProbabilityGenerator
{
    private readonly bool _isDiscrete;

    private readonly List<EmpiricalProbabilityTableItem> _probabilityTable;

    private readonly Random[] _randoms;
    
    public EmpiricalProbabilityGenerator(bool isDiscrete, List<EmpiricalProbabilityTableItem> probabilityTable, SeedGenerator seedGenerator)
    {
        _isDiscrete = isDiscrete;
        _probabilityTable = probabilityTable;
        
        _randoms = new Random[probabilityTable.Count + 1];
        _randoms[0] = new Random(seedGenerator.Next());
        
        var probabilitySumCheck = 0.0;

        for (var i = 0; i < _probabilityTable.Count; i++)
        {
            probabilitySumCheck += _probabilityTable[i].Probability;
            _randoms[i + 1] = new Random(seedGenerator.Next());
        }
        
        var difference = Math.Abs(probabilitySumCheck - 1.0);
        
        if (difference > 0.00000001)
        {
            throw new ArgumentException("Sum of all probabilities in table must by 1");
        }
    }
    
    public double Next()
    {
        var probabilityForSelectingTableItem = _randoms[0].NextDouble();

        var cumulativeProbabilities = 0.0;
        
        for (var i = 0; i < _probabilityTable.Count; i++)
        {
            cumulativeProbabilities += _probabilityTable[i].Probability;
            
            if (probabilityForSelectingTableItem < cumulativeProbabilities)
            {
                return NextFromTableItem(i);
            }
        }
        
        throw new Exception("This exception should never be thrown");
    }
    
    private double NextFromTableItem(int tableItemIndex)
    {
        var random = _randoms[tableItemIndex + 1];
        var intervalWidth = _probabilityTable[tableItemIndex].UpperBound - _probabilityTable[tableItemIndex].LowerBound;

        if (!_isDiscrete)
        {
            return random.NextDouble() * intervalWidth + _probabilityTable[tableItemIndex].LowerBound;
        }
        
        var lowerBound = (int)_probabilityTable[tableItemIndex].LowerBound;
        var upperBound = (int)_probabilityTable[tableItemIndex].UpperBound;

        return random.Next(lowerBound, upperBound);
    }
}
