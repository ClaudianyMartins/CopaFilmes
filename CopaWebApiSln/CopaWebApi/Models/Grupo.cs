using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CopaWebApi.Models
{
    public class Grupo
    {
        public string Id { get; set; }
        public List<Filme> Filmes { get; set; }
        public string IdPrimeiroPodio { get; set; }
        public string IdSegundoPodio { get; set; }

        public Grupo()
        {
            this.Filmes = new List<Filme>();
        }
    }
}
