using System;
using Xunit;
using Unica.TemporalExpressionSimulator;
using System.Collections.Generic;

namespace ExpressionSimulatorTest
{
    public class UnitTest1
    {
        [Fact]
        public void Composition()
        {
            TestTerm term1 = new TestTerm();
            TestTerm term2 = new TestTerm();
            TestTerm term3 = new TestTerm();
            TestTerm term4 = new TestTerm();

            CompositeTerm sequence = new Sequence(new List<Term>(new Term[] { term1, term2}));
            CompositeTerm parallel = new Parallel(new List<Term>(new Term[] { term3, term4 }));

            Assert.True(sequence.Children != parallel.Children);

        }

        [Fact]
        public void IterativeOperator()
        {
            var term1 = new TestTerm
            {
                Id = "dummy"
            };
            var iterative = new Iterative(term1);
            var token1 = new TestToken
            {
                Id = "dummy"
            };

            iterative.OnComplete += Iterative_OnComplete;

            iterative.Fire(token1);
            Assert.True(iterative.State == ExpressionState.Complete);

            // execute the term more than once
            iterative.Fire(token1);
            Assert.True(iterative.State == ExpressionState.Complete);
        }

        void Iterative_OnComplete(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            Iterative iterative = sender.term as Iterative;

            Object.Equals(token.Id, "dummy");
        }


        [Fact]
        public void SequenceOperator()
        {
            var term1 = new TestTerm
            {
                Id = "seq1"
            };
            var term2 = new TestTerm
            {
                Id = "seq2"
            };

            var sequence = new Sequence(new List<Term>(new Term[] { term1, term2 }));

            sequence.OnComplete += Sequence_OnComplete;
            var token1 = new TestToken
            {
                Id = "seq1"
            };
            var token2 = new TestToken
            {
                Id = "seq2"
            };

            Assert.False(sequence.LookAhead(token2));

            sequence.Fire(token1);

            Assert.True(sequence.State == ExpressionState.Default);
            Assert.True(sequence.LookAhead(token2));

            sequence.Fire(token2);
            Assert.True(sequence.State == ExpressionState.Complete);


        }

        void Sequence_OnComplete(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            Sequence sequence = sender.term as Sequence;

            Object.Equals(token.Id, "seq2");
        }


        [Fact]
        public void ParallelOperator1()
        {
            var term1 = new TestTerm()
            {
                Id = "par"
            };
            var term2 = new TestTerm()
            {
                Id = "par"
            };
            var parallel = new Parallel(new List<Term>(new Term[] { term1, term2}));

            term1.OnComplete += Term1_OnComplete;
            term2.OnComplete += Term2_OnComplete;


            TestToken token = new TestToken()
            {
                Id = "par"
            };

            parallel.Fire(token);

            Assert.True(parallel.State == ExpressionState.Complete);

        }

        void Term2_OnComplete(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            TestTerm term = sender.term as TestTerm;

            Object.Equals(token.Id, "par");
        }


        void Term1_OnComplete(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            TestTerm term = sender.term as TestTerm;

            Object.Equals(token.Id, "par");
        }

        [Fact]
        public void ParallelOperator2()
        {
            var term1 = new TestTerm()
            {
                Id = "A"
            };
            var term2 = new TestTerm()
            {
                Id = "B"
            };

            var iterative1 = new Iterative(term1);
            var iterative2 = new Iterative(term2);
            var parallel = new Parallel(new List<Term>(new Term[] { iterative1, iterative2 }));

            var tokenA = new TestToken()
            {
                Id = "A"
            };
            var tokenB = new TestToken()
            {
                Id = "B"
            };

            term1.OnComplete += Term1_OnComplete1;
            term2.OnComplete += Term2_OnComplete1;

            parallel.Fire(tokenA);
            parallel.Fire(tokenA);
            parallel.Fire(tokenB);

            Assert.True(parallel.State == ExpressionState.Complete);

        }

        void Term2_OnComplete1(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            TestTerm term = sender.term as TestTerm;

            Object.Equals(token.Id, "B");
        }


        void Term1_OnComplete1(ExpressionEventArgs sender)
        {
            TestToken token = sender.token as TestToken;
            TestTerm term = sender.term as TestTerm;

            Object.Equals(token.Id, "B");
        }


        [Fact]
        public void ChoiceOperator()
        {
            var term1 = new TestTerm()
            {
                Id = "A"
            };

            var term2 = new TestTerm()
            {
                Id = "A"
            };

            var term3 = new TestTerm()
            {
                Id = "B"
            };

            var term4 = new TestTerm()
            {
                Id = "B"
            };

            var sequence = new Sequence(new List<Term>(new Term[] { term1, term2 }));
            var parallel = new Parallel(new List<Term>(new Term[] { term3, term4 }));

            var choice = new Choice(new List<Term>(new Term[] { sequence, parallel }));

            var tokenA = new TestToken()
            {
                Id = "A"
            };
            var tokenB = new TestToken()
            {
                Id = "B"
            };

            choice.Fire(tokenA);
            choice.Fire(tokenA);
            Assert.True(sequence.State == ExpressionState.Complete);
            Assert.True(choice.State == ExpressionState.Complete);

            choice.Reset();

            choice.Fire(tokenB);
            Assert.True(parallel.State == ExpressionState.Complete);
            Assert.True(choice.State == ExpressionState.Complete);

        }

        [Fact]
        public void OrderIndependenceOperator()
        {
            var term1 = new TestTerm()
            {
                Id = "A"
            };

            var term2 = new TestTerm()
            {
                Id = "A"
            };

            var term3 = new TestTerm()
            {
                Id = "B"
            };

            var term4 = new TestTerm()
            {
                Id = "A"
            };

            var sequence = new Sequence(new List<Term>(new Term[] { term1, term2 }));
            var parallel = new Parallel(new List<Term>(new Term[] { term3, term4 }));

            var order = new OrderIndependece(new List<Term>(new Term[] { sequence, parallel }));

            var tokenA = new TestToken()
            {
                Id = "A"
            };
            var tokenB = new TestToken()
            {
                Id = "B"
            };

            order.Fire(tokenA);
            order.Fire(tokenA);

            Assert.True(sequence.State == ExpressionState.Complete);

            order.Fire(tokenB);
            order.Fire(tokenA);

            Assert.True(parallel.State == ExpressionState.Complete);
            Assert.True(order.State == ExpressionState.Complete);

            order.Reset();

            order.Fire(tokenB);
            order.Fire(tokenA);

            Assert.True(parallel.State == ExpressionState.Complete);


            order.Fire(tokenA);
            order.Fire(tokenA);

            Assert.True(sequence.State == ExpressionState.Complete);
            Assert.True(order.State == ExpressionState.Complete);

        }

        [Fact]
        public void DisablingOperator()
        {
            var term1 = new TestTerm()
            {
                Id = "A"
            };

            var iterative1 = new Iterative(term1);

            var term2 = new TestTerm()
            {
                Id = "B"
            };

            var iterative2 = new Iterative(term2);

            var term3 = new TestTerm()
            {
                Id = "C"
            };

            var disabling = new Disabling(new List<Term>(new Term[] { iterative1, iterative2, term3 }));

            var tokenA = new TestToken()
            {
                Id = "A"
            };

            var tokenB = new TestToken()
            {
                Id = "B"
            };

            var tokenC = new TestToken()
            {
                Id = "C"
            };

            // una sequenza di token A
            disabling.Fire(tokenA);
            disabling.Fire(tokenA);

            // invio un token C e completo l'espressione
            disabling.Fire(tokenC);
            Assert.True(disabling.State == ExpressionState.Complete);

            disabling.Reset();

            // dopo un token B, l'espressione non accetta più A
            disabling.Fire(tokenA);
            disabling.Fire(tokenB);
            disabling.Fire(tokenA);

            Assert.True(disabling.State == ExpressionState.Error);

            disabling.Reset();

            //una sequenza di token A
            disabling.Fire(tokenA);
            disabling.Fire(tokenA);
            disabling.Fire(tokenA);
            disabling.Fire(tokenA);

            // stop della prima iterazione con uno o più token B
            disabling.Fire(tokenB);
            disabling.Fire(tokenB);
            Assert.True(iterative2.State == ExpressionState.Complete);

            // stop della seconda iterazione con un token C
            // invio un token C e completo l'espressione
            disabling.Fire(tokenC);
            Assert.True(disabling.State == ExpressionState.Complete);




        }

    }
}
