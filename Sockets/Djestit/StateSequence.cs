using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.Djestit
{
    public class StateSequence
    {
        /* Attributi */
        public int capacity {set; get;}
        public List<Token> tokens = new List<Token>();
        public int index {set;get;}

        /* Metodi */
        public StateSequence(int capacity)
        {
            this.capacity = capacity != 0 ? capacity : 2;
	        this.index = -1;
        }

        public StateSequence()
        {
            
        }

        public void _push(Token token)
        {
            if (this.tokens.Count > this.capacity)
            {
                this.tokens.Add(token);
                this.index++;
            }
            else
            {
                this.index = (this.index + 1) % this.capacity;
                this.tokens.Insert(index, token);
            }
        }

        public virtual void push(Token token)
        {
            this._push(token);
        }

        public Token get(int delay)
        { 
            int pos = Math.Abs(this.index - delay) % this.capacity;
            return this.tokens[pos];
        }
    }
}
