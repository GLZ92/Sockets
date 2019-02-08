using System;
using Unica.TemporalExpressionSimulator;

namespace Unica.TaskSimulator
{
    public class TaskToken : Token
    {
        public TaskToken()
        {
        }

        public string Id { get; set; }

        // TODO qui puoi aggiungere tutti gli attributi di 
        // un task che servono (se servono)
    }

    // questa classe dovrebbe essere già ok così
    public class TaskTerm : GroundTerm<TaskToken>
    {
        public string Id { get; set; }

        public TaskTerm()
        {
            this.Id = "";
            this.AcceptToken = CheckToken;
        }

        private bool CheckToken<T>(T t)
        {
            TaskToken test = t as TaskToken;
            return this.Id.Equals(test.Id);
        }
    }

    public class TcpSensor : Sensor
    {
        // TODO aggiungere la gestione del socket TPC

        public TcpSensor(int capacity)
            : base(capacity)
        {
            // init dei socket
        }

        public void LoadTaskModel(string path) 
        {
            // creo la rappresentazione a del modello dei task 
            // con oggetti di tipo Term
            // da sostituire con il parsing dell'XML che si trova 
            // in path
            this.Root = ReadXmlFile(path);

            // 


        }

        /// <summary>
        /// Aggiungiamo a tutti i ground term un handler per il loro completamento
        /// </summary>
        /// <param name="t">T.</param>
        private void AddCompleteHandlers(Term t)
        {
            if(t != null && t is GroundTerm<TaskToken>)
            {
                t.OnComplete += T_OnComplete;
            }
        }

        void T_OnComplete(ExpressionEventArgs sender)
        {
            TaskTerm t = sender.term as TaskTerm;
            // implementazione idiota
            Console.Write("Espressione {0} completata", t.Id);
        }




        private Term ReadXmlFile(string path)
        {
            // stub
            return null; 
        }

        // esegui questo metodo per eseguire un task foglia a partire dal suo ID
        // questo id lo puoi ricevere sia dagli Hololens via TCP
        // oppure lo puoi ricevere come input utente (tastiera in un main da
        // console o via interfaccia grafica)
        public void exec()
    }
}
