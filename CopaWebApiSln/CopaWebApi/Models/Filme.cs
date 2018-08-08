using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CopaWebApi.Models
{
    public class Filme
    {
        public string Id { get; set; }
        public string PrimaryTitle { get; set; }
        public int Year { get; set; }
        public float AverageRating { get; set; }
        public bool Selected { get; set; }
        public string Group { get; set; }

        public Filme() { }

        public Filme(string id, string titulo, int ano, float nota)
        {
            this.Id = id;
            this.PrimaryTitle = titulo;
            this.Year = ano;
            this.AverageRating = nota;
            this.Selected = false;
            this.Group = null;
        }
    }

    public class FilmesParticipantes {

        public int TotalGeralFilmes { get; set; }
        public int TotalFilmeSelecionados { get; set; }       
        public List<Filme> FilmesA { get; set; }
        public List<Filme> FilmesB { get; set; }
        public List<Filme> FilmesC { get; set; }
        public List<Filme> FilmesD { get; set; }
        public string Message { get; set; }

        public FilmesParticipantes()
        {
            this.TotalGeralFilmes = 0;
            this.TotalFilmeSelecionados = 0;           
            this.FilmesA = new List<Filme>();
            this.FilmesB = new List<Filme>();
            this.FilmesC = new List<Filme>();
            this.FilmesD = new List<Filme>();
            this.Message = null;
        }
    }
}
