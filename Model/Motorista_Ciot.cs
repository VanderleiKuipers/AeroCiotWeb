using System;
using System.Collections.Generic;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Motorista_Ciot
    {
        public int Id { get; set; }
        public string Cpf_Motorista { get; set; }
        public DateTime Data_Geracao { get; set; }
        public string Ciot { get; set; }
        public DateTime ?Inicio_Viagem { get; set; }
        public DateTime ?Final_Viagem { get; set; }
        public int Id_Fopag_Ciot_Item { get; set; }
        public DateTime Dt_Inclusao { get; set; }
        public string Usua_Inclusao { get; set; }
        public string Cv { get; set; }
        public int Tp_Finalizado { get; set; }
        public string Usuario_Finalizacao { get; set; }
        public DateTime ?Dt_Finalizacao { get; set; }
        public string Placa { get; set; }
        public string Cpf_Proprietario { get; set; }
        public string Ciot_Protocolo_Encerramento { get; set; }
        public string Cnpj_Destino { get; set; }
        public string Msg_Erro { get; set; }
    }
}