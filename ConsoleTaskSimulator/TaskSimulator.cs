using System;
using Unica.TemporalExpressionSimulator;
using System.Net.Sockets;
using System.Net;
using System.Text;

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
        public string name;
    }

    // questa classe dovrebbe essere già ok così
    public class TaskTerm : GroundTerm<TaskToken>
    {
        //public string Id { get; set; }

        public TaskTerm(String ID, String _name) //aggiunto parametro ingresso
        {
            this.Id = ID;
            this.Children = new System.Collections.Generic.List<Term>();
            this.AcceptToken = CheckToken;
            this.Name = _name;
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
        ///public IPAddress clientAddress;
        public IPEndPoint localEndPoint;
        public Socket listener;

        public TcpSensor(int capacity)
            : base(capacity)
        {
            // init dei socket TCP e gestione dell'arrivo dei messaggi
            IPHostEntry ipHostInfo = null;
            try
            {
                ipHostInfo = Dns.GetHostEntry("127.0.0.1");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace.ToString());
                Console.ReadLine();
            }
            IPAddress address = ipHostInfo.AddressList[0];
            localEndPoint = new IPEndPoint(address, 11000);
            listener = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
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
            //Console.Write("Espressione {0} completata", t.Id);
            
            //SendMessage(t.Id);
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

        public void SendMessage(string data)
        {
            // genera il messaggio da inviare agli hololens in base
            // all'ID del task e lo manda via TCP

            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                IPAddress serverAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEndPoint = new IPEndPoint(serverAddress, 12000);

                // Create a TCP/IP  socket.  
                Socket snd = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    snd.Connect(remoteEndPoint);

                    Console.WriteLine("Socket connesso con {0}", snd.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    // Send the data through the socket.  
                    int bytesSent = snd.Send(msg);
                    Console.WriteLine("Messaggio inviato a {0}", snd.RemoteEndPoint.ToString());

                    // Release the socket.  
                    snd.Shutdown(SocketShutdown.Both);
                    snd.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        //SOCKET SERVER
        public void waitInput(TaskTerm root)
        {
            byte[] bytes = new Byte[1024];
            try
            {
                listener.Listen(1);
                Console.WriteLine("\nPronto all'ascolto sulla porta " + localEndPoint.Port);

                //attendo connessioni 
                while (true)
                {
                    Socket handler = listener.Accept();
                    String data = null;
                    
                    //ricevo messaggio
                    data += Encoding.ASCII.GetString(bytes, 0, handler.Receive(bytes));
                    
                    //mostro a schermo il messaggio 
                    Console.WriteLine("Messaggio ricevuto da : {0}", localEndPoint.Address);
                    Console.WriteLine("<< {0} >>", data);
                    
                    //aggiorno l'albero
                    navigate(data, root);

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    handler.Send(msg);
                    Console.WriteLine("Messaggio inviato a : {0}", localEndPoint.Address);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void checkOperator(Term oper)
        {
            if (oper.kind.Split(" ")[0].Equals("operator:"))
            {
                String operatorType = oper.kind.Split(" ")[1];
                bool completed = true;
                if (operatorType.Equals("enable")) //tutti i task figli devono essere completati
                {
                    for (int i = 0; i < oper.Children.Count && completed; i++)
                        if (oper.Children[i].State != ExpressionState.Complete)
                            completed = false;
                }
                else if(operatorType.Equals("choice")) //basta che solo uno dei figli sia completato
                {
                    completed = false;
                    for (int i = 0; i < oper.Children.Count && !completed; i++)
                        if (oper.Children[i].State != ExpressionState.Complete)
                            completed = true;
                }

                if (completed)
                {
                    TaskToken t = new TaskToken();
                    t.Id = oper.Id;
                    AddCompleteHandlers(oper);
                    oper.Fire(t);
                }
            }
        }

        private void navigate(String data, Term root)
        {
            String[] d = data.Split(new Char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int x = Convert.ToInt32(d[0]);
            int y = Convert.ToInt32(d[1]);

            if (root.getX() == x && root.getY() == y)
            {
                //esegui il task
                TaskToken t = new TaskToken();
                t.Id = data;
                AddCompleteHandlers(root);
                root.Fire(t);
            }
            else
            {
                for(int i = 0; i < root.Children.Count; i++)
                {
                    Term child = root.Children[i];
                    if (child.State != ExpressionState.Complete)
                    {
                        navigate(data, child);

                        //criterio di uscita dal for
                        if (child.State == ExpressionState.Complete)
                            break;
                    }
                }

                //controllo se l'operator è completato
                //checkOperator(root);
            }
        }
    }
}
