using System.Collections.Generic;
using CopaWebApi.Models;

namespace CopaWebApi.Repository
{
    public interface IFilmeRepository
    {
        List<Filme> ListarFilmes();
    }
}