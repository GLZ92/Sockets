using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.Djestit
{
    public class TokenFireArgs : EventArgs
    {
        /* Attributi */
        Token token;
        Term term;

        /* Costruttore */
        public TokenFireArgs(Token token, Term term)
        {
            this.token = token;
            this.term = term;
        }
    }
}
