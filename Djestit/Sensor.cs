using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.TemporalExpressionSimulator
{


    public abstract class Sensor
    {
        
        /* Attributi  */
        public int Capacity { get; private set; }
        public Term Root { get; internal set; }
        public TokenSequence tokens { get; internal set; }

        internal Sensor(int capacity)
        {
            this.Capacity = capacity;
            this.tokens = new TokenSequence(capacity);
        }

        public virtual void Emit(Token t) 
        {
            if(this.Root != null) 
            {
                this.Root.Fire(t);
                tokens.Push(t);
            }
        }



    }
}
