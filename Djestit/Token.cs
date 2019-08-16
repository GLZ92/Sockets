using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.TemporalExpressionSimulator
{
    public enum TypeToken
    {
        Start,
        Move,
        End
    }

    public class Token
    {
        public Token()
        {

        }

        public String ID = "";
    }
}
