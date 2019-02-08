using System;


namespace Unica.TemporalExpressionSimulator
{
    // Delegate funzione di Accept
    public delegate bool CheckTokenType<T>(T token) where T : Token;

    public abstract class GroundTerm<T> : Term
    {
        /* Attributi */
        public String Type { get; set; }// Tipo di ground term
        public CheckTokenType<Token> AcceptToken { get; protected set; }


        public virtual bool Accepts(Token t)
        {
            if (this.AcceptToken(t))
            {
                return true;
            }

            return false;
        }

        public override bool LookAhead(Token token)
        {
            if (this.Accepts(token))
            {
                return true;
            }

            return false;
        }
    }
    
}
