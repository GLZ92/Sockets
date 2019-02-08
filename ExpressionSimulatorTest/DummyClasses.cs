using System;
using Unica.TemporalExpressionSimulator;

namespace ExpressionSimulatorTest
{
    public class TestToken : Token
    {
        public string Id { get; set; }
    }

    public class TestSensor: Sensor 
    { 
        public TestSensor(Term root, int capacity) : 
            base(root,capacity)
        { 
        
        }
    }

    public class TestTerm: GroundTerm<TestToken>
    {
        public string Id { get; set; }

        public TestTerm() 
        {
            this.Id = "";
            this.AcceptToken = CheckToken;
        }

        private bool CheckToken<T>(T t) 
        {
            TestToken test = t as TestToken;
            return this.Id.Equals(test.Id);
        }
    }
}
