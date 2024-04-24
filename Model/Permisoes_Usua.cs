using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Permisoes_Usua
    {
        public int idUsuario { get; set; }
        public bool gerarCiot { get; set; }
        public bool finalizarCiot { get; set; }
        public bool excluirCiot { get; set; }
        public bool cancelarCiot { get; set; }
    }
}