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
            // init dei socket TCP e gestione dell'arrivo dei messaggi
        }

        public void LoadTaskModel(string path) 
        {
            // creo la rappresentazione a del modello dei task 
            // con oggetti di tipo Term
            // da sostituire con il parsing dell'XML che si trova 
            // in path
            this.Root = ReadXmlFile(path);

            // aggiungo gli handler per ricevere notifiche quando ho completato
            // qualche task
            this.AddCompleteHandlers(this.Root);


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
            // implementazione idiota, da aggiungere un messaggio TCP verso 
            // gli Hololens. Attenzione alle notifiche circolari:
            // se hai eseguito un task perché ti è arrivato il comando via 
            // TCP non è necessario generare il messaggio per gli Hololens
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
        public void ExecuteTaskById(string id)
        {
            TaskToken token = new TaskToken()
            {
                Id = id
            };

            if(this.Root != null)
            {
                this.Root.Fire(token);
            }
        }

        public void SendMessage(string id)
        {
            // genera il messaggio da inviare agli hololens in base
            // all'ID del task e lo manda via TCP
        }
    }
}
