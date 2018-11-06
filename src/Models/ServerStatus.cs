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
        public float RPS { get; set; }
        public int Workers { get; set; }

        public override string ToString()
            =>  "\nTAQUERIA DON MIGUELITO\n" +
                $"Han llegado {ReceivedClients} clientes a la taquería el día de hoy.\n" +
                $"En promedio, llegan clientes cada {AvarageArrivalTime.Seconds} segundos.\n" +
                $"Hay {Workers} meseros disponibles para atender a los {PendingRequests} clientes que esperan en la fila.\n" + 
                $"En promedio, cada cliente espera {AvarageWaitTime.Seconds} segundos para ser atendido.\n" + 
                $"De esta forma, la Taquería Don Miguelito puede procesar {RPS} pedidos por segundo (nada mal, eh).\n" + 
                $"Ya se les ha dado una rica comida a {ServedClients} clientes el dia de hoy.\n";
    }
}