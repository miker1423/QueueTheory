using System;

namespace TcpServer.Models
{
    public class ServerStatus
    {
        public int ReceivedClients { get; set; }
        public int ServedClients { get; set; }
        public int PendingRequests { get; set; }
        public TimeSpan AvarageWaitTime { get; set; }
        public TimeSpan AvarageArrivalTime { get; set; }
        public double RPS { get; set; }
        public int Workers { get; set; }

        public override string ToString()
            =>  $@"TAQUERIA DON MIGUELITO 
                Han llegado {ReceivedClients} clientes a la taquería el día de hoy.
                En promedio, llegan clientes cada {AvarageArrivalTime} segundos.
                Hay {Workers} meseros disponibles para atender a los {PendingRequests} clientes que esperan en la fila.
                En promedio, cada cliente espera {AvarageWaitTime} segundos para ser atendido.
                De esta forma, la Taquería Don Miguelito puede procesar {RPS} pedidos por segundo (nada mal, eh).
                Ya se les ha dado una rica comida a {ServedClients} clientes el dia de hoy.";
    }
}