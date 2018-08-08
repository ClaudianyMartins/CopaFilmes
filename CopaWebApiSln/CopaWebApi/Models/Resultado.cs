using CopaWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CopaWebApi.Models
{
    public class Resultado
    {
        public int PosicaoPodioPrimeiro => Constantes.POSICAO_PODIO_1;
        public string TituloPodioPrimeiro { get; set; }
        public string IdPodioPrimeiro { get; set; }

        public int PosicaoPodioSegundo => Constantes.POSICAO_PODIO_2;
        public string TituloPodioSegundo { get; set; }
        public string IdPodioSegundo { get; set; }

        public int PosicaoPodioTerceiro => Constantes.POSICAO_PODIO_3;
        public string TituloPodioTerceiro { get; set; }
        public string IdPodioTerceiro { get; set; }
    }
}
