using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace Unica.Djestit
{
    // Delegate funzione di Accept
    public delegate bool Accepts<T>(T token) where T : Token;

    public class GroundTerm : Term
    {
        /* Attributi */
        public String Type { get; set; }// Tipo di ground term
        public Accepts<Token> Accepts { get; set; }
        public Accepts<Token> _Accepts { get; set; }

        /* Metodi */
        public virtual bool _accepts(Token token)
        {
            return true;
        }

        public override bool lookahead(Token token)
        {
            if(Accepts != null && _Accepts != null)
            {
                return Accepts(token) && _Accepts(token);
            }
            else if(Accepts != null)
            {
                return Accepts(token);
            }
            else if(_Accepts != null)
            {
                return _Accepts(token);
            }
            
            return false;
        }
    }
    
}
