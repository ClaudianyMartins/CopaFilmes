using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CopaWebApi.Models
{
    public class Partida
    {
        public Filme Filme { get; set; }
        public Filme FilmeAdversario { get; set; }
        public string IdFilmeVencedor { get; set; }
        public int Podio { get; set; }

        public Partida()
        {
            this.Filme = new Filme();
            this.FilmeAdversario = new Filme();
        }
    }
}
