using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.TemporalExpressionSimulator
{
    /// <summary>
    /// Mantiene una cronologia degli ultimi n token ricevuti
    /// </summary>
    public class TokenSequence
    {
        /* Attributi */
        public int Capacity {set; get;}
        public List<Token> tokens = new List<Token>();
        public int Index {set;get;}

        /* Metodi */
        public TokenSequence(int capacity)
        {
            this.Capacity = capacity != 0 ? capacity : 2;
	        this.Index = -1;
        }

        public TokenSequence()
        {
            
        }

        public virtual void Push(Token token)
        {
            if (this.tokens.Count > this.Capacity)
            {
                this.tokens.Add(token);
                this.Index++;
            }
            else
            {
                this.Index = (this.Index + 1) % this.Capacity;
                this.tokens.Insert(Index, token);
            }
        }

        public Token Get(int delay)
        { 
            int pos = Math.Abs(this.Index - delay) % this.Capacity;
            return this.tokens[pos];
        }
    }
}
