using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CopaWebApi.Models;
using CopaWebApi.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Caching;
using CopaWebApi.Utils;
using Newtonsoft.Json;

namespace CopaWebApi.Controllers
{
    public class HomeController : Controller
    {
        public static FilmesParticipantes filmesParticipantes;
        
        private readonly IFilmeRepository _filmeRepository;

        public HomeController(IFilmeRepository filmeRepository)
        {
            _filmeRepository = filmeRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                filmesParticipantes = new FilmesParticipantes();
                
                List<Filme> listaFilmes = _filmeRepository.ListarFilmes();                   

                if (listaFilmes != null && listaFilmes.Any())
                {
                    int index = 0;

                    listaFilmes = RemoverDuplicados(listaFilmes);

                    filmesParticipantes.TotalGeralFilmes = listaFilmes.Count;

                    filmesParticipantes.FilmesA = ObterFilmesPorGrupo(listaFilmes, index);
                    index += filmesParticipantes.FilmesA.Count;
                    filmesParticipantes.FilmesB = ObterFilmesPorGrupo(listaFilmes, index);
                    index += filmesParticipantes.FilmesB.Count;
                    filmesParticipantes.FilmesC = ObterFilmesPorGrupo(listaFilmes, index);
                    index += filmesParticipantes.FilmesC.Count;
                    filmesParticipantes.FilmesD = ObterFilmesPorGrupo(listaFilmes, index);
                }

                return View(filmesParticipantes);                
               
            }
            catch (Exception)
            {
                filmesParticipantes.Message = "Não foi possível exibir a lista de filmes. Por favor, tente mais tarde!";
                return View(filmesParticipantes);
            }
        }

        [HttpPost]
        public IActionResult Index(FilmesParticipantes filmes)
        {
            try
            {
                filmesParticipantes.Message = null;

                if (filmes != null && filmes.FilmesA.Any())
                {
                    List<Filme> filmesSelecionados = ObterFilmesSelecionados(filmes.FilmesA, filmes.FilmesB, filmes.FilmesC, filmes.FilmesD);

                    filmes.TotalGeralFilmes = filmesParticipantes.TotalGeralFilmes;
                    filmesParticipantes = filmes;
                    filmesParticipantes.TotalFilmeSelecionados = filmesSelecionados.Count;

                    if (filmesSelecionados.Count == Constantes.QTD_FILMES_CAMPEONATO)
                    {   
                        Resultado resultado = ObterResultadoCampeonato(filmesSelecionados);

                        if (resultado != null && resultado.IdPodioPrimeiro != null)
                        {
                            ViewBag.resultado = JsonConvert.SerializeObject(resultado);                            
                        }
                        else
                        {
                            filmesParticipantes.Message = "Não foi possível exibir gerar o campeonato.Por favor, tente mais tarde!";
                        }
                    }
                    else
                    {
                        filmesParticipantes.Message = "Para gerar campeonato é necessário selecionar 16 filmes.";
                    }
                }
                else
                {
                    filmesParticipantes.Message = "Para gerar campeonato é necessário selecionar 16 filmes.";
                }               

                return View(filmesParticipantes);
            }
            catch (Exception)
            {
                filmesParticipantes.Message = "Não foi possível exibir gerar o campeonato. Por favor, tente mais tarde!";

                return View(filmesParticipantes);
            }
        }
       
        #region Métodos da Copa dos filmes

        private List<Filme> RemoverDuplicados(List<Filme> listaFilmes)
        {
            List<Filme> filmes = new List<Filme>();
            foreach (var filme in listaFilmes)
            {
                var filmeDuplicado = filmes.Any(f => f.Id == filme.Id);
                if (!filmeDuplicado)
                {
                    filmes.Add(filme);
                }
            }
            return filmes;
        }

        private List<Filme> ObterFilmesPorGrupo(List<Filme> listaFilmes, int index)
        {
            List<Filme> filmesAgrupados = new List<Filme>();
            int qtdPorGrupo = listaFilmes.Count / 4;

            filmesAgrupados = listaFilmes.GetRange(index, qtdPorGrupo).ToList<Filme>();

            return filmesAgrupados;
        }

        private Resultado ObterResultadoCampeonato(List<Filme> filmes)
        {
            Resultado resultado = new Resultado();

            if (filmes != null && filmes.Any())
            {
                List<Grupo> agrupamentoFilmes = ObterFilmesAgrupados(filmes);

                if (agrupamentoFilmes.Any())
                {
                    List<Grupo> filmesFaseGrupo = new List<Grupo>();
                    filmesFaseGrupo = ObterFaseGrupo(agrupamentoFilmes);
                    if (filmesFaseGrupo.Any())
                    {
                        List<Partida> filmesFaseEliminatoria = new List<Partida>();
                        filmesFaseEliminatoria = ObterFaseEliminatoria(filmesFaseGrupo);
                        if (filmesFaseEliminatoria.Any())
                        {
                            ViewBag.resultadoEliminatoria = JsonConvert.SerializeObject(filmesFaseEliminatoria);
                            List<Partida> filmesSemiFinal = new List<Partida>();
                            filmesSemiFinal = ObterFaseSemiFinal(filmesFaseEliminatoria);
                            if (filmesSemiFinal.Any())
                            {
                                ViewBag.resultadoSemiFinal = JsonConvert.SerializeObject(filmesSemiFinal);
                                List<Partida> partidaFinal = ObterFinalCopa(filmesSemiFinal);

                                if (partidaFinal != null)
                                {
                                    var partida = partidaFinal.Select(n => n).Where(n => n.Podio == Constantes.POSICAO_PODIO_1).FirstOrDefault<Partida>();
                                    if (partida != null)
                                    {
                                        resultado.IdPodioPrimeiro = partida.IdFilmeVencedor;
                                        resultado.TituloPodioPrimeiro = partida.Filme.PrimaryTitle;

                                        resultado.IdPodioTerceiro = partida.FilmeAdversario.Id;
                                        resultado.TituloPodioTerceiro = partida.FilmeAdversario.PrimaryTitle;
                                    }                                   

                                    partida = partidaFinal.Select(n => n).Where(n => n.Podio == Constantes.POSICAO_PODIO_2).FirstOrDefault<Partida>();
                                    if (partida != null)
                                    {
                                        resultado.IdPodioSegundo = partida.IdFilmeVencedor;
                                        resultado.TituloPodioSegundo = partida.Filme.PrimaryTitle;
                                    }
                                }

                                return resultado;
                            }
                        }
                    }
                }
            }
            return resultado;
        }

        private List<Filme> ObterFilmesSelecionados(List<Filme> filmesA, List<Filme> filmesB, List<Filme> filmesC, List<Filme> filmesD)
        {
            List<Filme> filmes = new List<Filme>();

            foreach (var item in filmesA)
            {
                if (item.Selected)
                    filmes.Add(item);
            }

            foreach (var item in filmesB)
            {
                if (item.Selected)
                    filmes.Add(item);
            }

            foreach (var item in filmesC)
            {
                if (item.Selected)
                    filmes.Add(item);
            }

            foreach (var item in filmesD)
            {
                if (item.Selected)
                    filmes.Add(item);
            }

            return filmes;
        }

        private List<Partida> ObterFinalCopa(List<Partida> filmesSemiFinal)
        {
            List<Partida> partidas = new List<Partida>();

            //Final copa
            Grupo grupoUM = new Grupo();
            Grupo grupoDois = new Grupo();

            var filmeUm = filmesSemiFinal.Select(n => n.Filme).FirstOrDefault<Filme>();
            var filmeDois = filmesSemiFinal.Select(n => n.Filme).LastOrDefault<Filme>();

            grupoUM.Filmes.Add(filmeUm);
            grupoUM.Id = filmeUm.Group;
            grupoUM.IdPrimeiroPodio = filmeUm.Id;
            grupoDois.Filmes.Add(filmeDois);
            grupoDois.Id = filmeDois.Group;
            grupoDois.IdSegundoPodio = filmeDois.Id;

            Partida partida = new Partida();
            partida = PartidaEliminatoria(grupoUM, grupoDois);
            if (partida != null)
            {
                partida.Podio = Constantes.POSICAO_PODIO_1;
                if (partida.IdFilmeVencedor == filmeUm.Id)
                {
                    var partidaAux = filmesSemiFinal.Select(n => n).Where(n => n.IdFilmeVencedor == filmeUm.Id).FirstOrDefault<Partida>();
                    partida.FilmeAdversario = partidaAux.FilmeAdversario;
                }
                else
                {
                    var partidaAux = filmesSemiFinal.Select(n => n).Where(n => n.IdFilmeVencedor == filmeDois.Id).FirstOrDefault<Partida>();
                    partida.FilmeAdversario = partidaAux.FilmeAdversario;
                }

                partidas.Add(partida);

                Partida partidaSegundoPodio = new Partida();
                if (partida.IdFilmeVencedor == filmeUm.Id)
                {
                    partidaSegundoPodio.IdFilmeVencedor = filmeDois.Id;
                    partidaSegundoPodio.Filme = filmeDois;
                }
                else
                {
                    partidaSegundoPodio.IdFilmeVencedor = filmeUm.Id;
                    partidaSegundoPodio.Filme = filmeUm;
                }
                partidaSegundoPodio.Podio = Constantes.POSICAO_PODIO_2;
                partidas.Add(partidaSegundoPodio);
            }

            return partidas;
        }

        private List<Partida> ObterFaseSemiFinal(List<Partida> filmesFaseEliminatoria)
        {
            List<Partida> vencedoresPartidas = new List<Partida>();

            //Partida 1º - grupo A e 1º - grupo B
            Grupo grupoUM = new Grupo();
            Grupo grupoDois = new Grupo();
            Filme filmeDois;
            Filme filmeUm;

            filmeUm = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_A).FirstOrDefault<Filme>();
            if (filmeUm == null)
            {
                filmeUm = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_B).FirstOrDefault<Filme>();
                filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_B).LastOrDefault<Filme>();
            }
            else
            {
                filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_B).FirstOrDefault<Filme>();
                if (filmeDois == null)
                {
                    filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_A).LastOrDefault<Filme>();
                }
            }

            grupoUM.Filmes.Add(filmeUm);
            grupoUM.IdPrimeiroPodio = filmeUm.Id;
            grupoUM.Id = filmeUm.Group;
            grupoDois.Filmes.Add(filmeDois);
            grupoDois.IdSegundoPodio = filmeDois.Id;
            grupoDois.Id = filmeDois.Group;

            Partida partida = new Partida();
            partida = PartidaEliminatoria(grupoUM, grupoDois);
            partida.Podio = Constantes.POSICAO_PODIO_1;
            if (partida.IdFilmeVencedor == filmeUm.Id)
            {
                partida.FilmeAdversario = filmeDois;
            }
            else
            {
                partida.FilmeAdversario = filmeUm;
            }
            vencedoresPartidas.Add(partida);

            //Partida 1º - grupo C e 1º - grupo D
            grupoUM = new Grupo();
            grupoDois = new Grupo();

            filmeUm = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_C).FirstOrDefault<Filme>();           
            if (filmeUm == null)
            {
                filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_D).FirstOrDefault<Filme>();
                filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_D).LastOrDefault<Filme>();
            }
            else
            {
                filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_D).FirstOrDefault<Filme>();
                if (filmeDois == null)
                {
                    filmeDois = filmesFaseEliminatoria.Select(n => n.Filme).Where(n => n.Group == Constantes.GRUPO_C).LastOrDefault<Filme>();
                }
            }

            grupoUM.Filmes.Add(filmeUm);
            grupoUM.IdPrimeiroPodio = filmeUm.Id;
            grupoUM.Id = filmeUm.Group;
            grupoDois.Filmes.Add(filmeDois);
            grupoDois.IdSegundoPodio = filmeDois.Id;
            grupoDois.Id = filmeDois.Group;

            partida = PartidaEliminatoria(grupoUM, grupoDois);
            if (partida != null)
            {
                partida.Podio = Constantes.POSICAO_PODIO_2;
                if (partida.IdFilmeVencedor == filmeUm.Id)
                {
                    partida.FilmeAdversario = filmeDois;
                }
                else
                {
                    partida.FilmeAdversario = filmeUm;
                }
                vencedoresPartidas.Add(partida);
            }

            return vencedoresPartidas;
        }

        private List<Partida> ObterFaseEliminatoria(List<Grupo> filmesFaseGrupo)
        {
            List<Partida> partidas = new List<Partida>();

            var grupoA = filmesFaseGrupo.Select(n => n).Where(n => n.Id == Constantes.GRUPO_A).FirstOrDefault<Grupo>();
            var grupoB = filmesFaseGrupo.Select(n => n).Where(n => n.Id == Constantes.GRUPO_B).FirstOrDefault<Grupo>();
            var grupoC = filmesFaseGrupo.Select(n => n).Where(n => n.Id == Constantes.GRUPO_C).FirstOrDefault<Grupo>();
            var grupoD = filmesFaseGrupo.Select(n => n).Where(n => n.Id == Constantes.GRUPO_D).FirstOrDefault<Grupo>();

            if (grupoA != null && grupoB != null && grupoC != null && grupoD != null)
            {
                //Partida 1º - grupo A e 2º - grupo B
                Partida partida = new Partida();
                partida = PartidaEliminatoria(grupoA, grupoB);
                partidas.Add(partida);

                //Partida 1º - grupo B e 2º - grupo A
                partida = new Partida();
                partida = PartidaEliminatoria(grupoB, grupoA);
                partidas.Add(partida);

                //Partida 1º - grupo C e 2º - grupo D
                partida = new Partida();
                partida = PartidaEliminatoria(grupoC, grupoD);
                partidas.Add(partida);

                //Partida 1º - grupo D e 2º - grupo C
                partida = new Partida();
                partida = PartidaEliminatoria(grupoD, grupoC);
                partidas.Add(partida);
            }                      

            return partidas;
        }

        private Partida PartidaEliminatoria(Grupo grupoUm, Grupo grupoDois)
        {
            List<Partida> partidas = new List<Partida>();
            Partida partida = new Partida();
            Filme filmeUm;
            Filme filmeDois;

             filmeUm = grupoUm.Filmes.Select(n => n).Where(n => n.Id == grupoUm.IdPrimeiroPodio).FirstOrDefault<Filme>();
             filmeDois = grupoDois.Filmes.Select(n => n).Where(n => n.Id == grupoDois.IdSegundoPodio).FirstOrDefault<Filme>();                     

            if (filmeUm != null && filmeDois != null)
            {
                List<Filme> participantes = new List<Filme>();
                participantes.Add(filmeUm);
                participantes.Add(filmeDois);

                var filmes = (from item in participantes
                              where item.AverageRating == (from valor in participantes select valor.AverageRating).Max()
                              select item).ToList<Filme>();

                if (filmes != null && filmes.Any())
                {
                    Filme filme = new Filme();
                    if (filmes.Count() == 1)
                    {
                        filme = filmes.FirstOrDefault();
                        partida.IdFilmeVencedor = filme.Id;
                        partida.Filme = filme;

                    }
                    else if (filmes.Count() > 1)
                    {
                        filmes = filmes.OrderByDescending(x => x.PrimaryTitle).ToList<Filme>();
                        filme = filmes.LastOrDefault();
                        partida.IdFilmeVencedor = filme.Id;
                        partida.Filme = filme;
                    }
                }
            }

            return partida;
        }
       
        private List<Grupo> ObterFaseGrupo(List<Grupo> agrupamentoFilmes)
        {
            List<Grupo> resultadoFaseGrupo = new List<Grupo>();

            foreach (var itemGrupo in agrupamentoFilmes)
            {
                Grupo grupo = AtribuirFilmesFaseGrupo(itemGrupo);

                resultadoFaseGrupo.Add(grupo);
            }

            return resultadoFaseGrupo;
        }

        private Grupo AtribuirFilmesFaseGrupo(Grupo itemGrupo)
        {
            Grupo grupo = new Grupo();

            if (itemGrupo.Filmes.Any())
            {
                var filmes = (from item in itemGrupo.Filmes
                              where item.AverageRating == (from valor in itemGrupo.Filmes select valor.AverageRating).Max()
                              select item).ToList<Filme>();

                if (filmes != null && filmes.Any())
                {
                    Filme filme = new Filme();
                    if (filmes.Count() == 1)
                    {
                        filme = filmes.FirstOrDefault();
                        grupo.IdPrimeiroPodio = filme.Id;
                        grupo.Id = itemGrupo.Id;
                        grupo.Filmes.Add(filme);
                        itemGrupo.Filmes.Remove(filme);

                        filme = new Filme();
                        filme = ObterSegundoPodio(itemGrupo.Filmes);
                        grupo.IdSegundoPodio = filme.Id;
                        grupo.Filmes.Add(filme);
                    }
                    else if (filmes.Count() > 1)
                    {
                        filmes = filmes.OrderByDescending(x => x.PrimaryTitle).ToList<Filme>();
                        filme = filmes.LastOrDefault();
                        grupo.Id = itemGrupo.Id;
                        grupo.IdPrimeiroPodio = filme.Id;
                        grupo.Filmes.Add(filme);
                        itemGrupo.Filmes.Remove(filme);

                        filme = new Filme();
                        filme = ObterSegundoPodio(itemGrupo.Filmes);
                        grupo.Id = itemGrupo.Id;
                        grupo.IdSegundoPodio = filme.Id;
                        grupo.Filmes.Add(filme);
                    }
                }
            }

            return grupo;
        }

        private Filme ObterSegundoPodio(List<Filme> listaFilmes)
        {
            Filme filme = new Filme();

            var filmes = (from item in listaFilmes
                          where item.AverageRating == (from valor in listaFilmes select valor.AverageRating).Max()
                          select item).ToList<Filme>();

            if (filmes != null)
            {
                if (filmes.Count() > 1)
                {
                    filmes = filmes.OrderByDescending(x => x.PrimaryTitle).ToList<Filme>();
                }
                filme = filmes.LastOrDefault();
            }

            return filme;
        }      
        
        private List<Grupo> ObterFilmesAgrupados(List<Filme> filmes)
        {
            List<Grupo> gruposClassificados = new List<Grupo>();

            List<Filme> listaAuxFilmes = new List<Filme>();

            if (filmes != null && filmes.Any())
            {
                gruposClassificados = OrdenacaoGrupos(filmes);
            }

            return gruposClassificados;
        }

        private List<Grupo> OrdenacaoGrupos(List<Filme> listaFilmes)
        {
            List<Grupo> filmesAgrupados = new List<Grupo>();

            if (listaFilmes != null && listaFilmes.Any())
            {
                filmesAgrupados = FormacaoGrupos(listaFilmes);
            }

            return filmesAgrupados;
        }

        private List<Grupo> FormacaoGrupos(List<Filme> listaFilmes)
        {
            Random rnd = new Random();
            List<Grupo> grupos = new List<Grupo>();
            List<Filme> listaAux = new List<Filme>();
            int contGrupo = 0;                       
            while(listaAux.Count < 16)
            {
                int index = rnd.Next(0, listaFilmes.Count);
                Filme filme = listaFilmes[index];
                var filmeDuplicado = listaAux.Any(f => f.Id == filme.Id);
                if (!filmeDuplicado)
                {
                    if (contGrupo < 4)
                    {
                        filme.Group = Constantes.GRUPO_A;
                        contGrupo++;
                    }
                    else if (contGrupo >= 4 && contGrupo < 8)
                    {
                        filme.Group = Constantes.GRUPO_B;
                        contGrupo++;
                    }
                    else if (contGrupo >= 8 && contGrupo < 12)
                    {
                        filme.Group = Constantes.GRUPO_C;
                        contGrupo++;
                    }
                    else if (contGrupo >= 12 && contGrupo < 16)
                    {
                        filme.Group = Constantes.GRUPO_D;
                        contGrupo++;
                    }
                    if (filme.Group != null)
                    {
                        listaAux.Add(filme);
                    }
                }
            }
           

            if (listaAux.Count > 0)
            {
                grupos = PopularGrupos(listaAux);
            }

            return grupos;
        }

        private List<Grupo> PopularGrupos(List<Filme> listaFilmes)
        {
            List<Grupo> grupos = new List<Grupo>();
            Grupo grupo = new Grupo();

            //popular grupo A
            grupo.Id = Constantes.GRUPO_A;
            grupo.Filmes = AtribuirFilmeAgrupados(grupo, listaFilmes);
            grupos.Add(grupo);

            //popular grupo B
            grupo = new Grupo();
            grupo.Id = Constantes.GRUPO_B;
            grupo.Filmes = AtribuirFilmeAgrupados(grupo, listaFilmes);
            grupos.Add(grupo);

            //popular grupo C
            grupo = new Grupo();
            grupo.Id = Constantes.GRUPO_C;
            grupo.Filmes = AtribuirFilmeAgrupados(grupo, listaFilmes);
            grupos.Add(grupo);

            //popular grupo D
            grupo = new Grupo();
            grupo.Id = Constantes.GRUPO_D;
            grupo.Filmes = AtribuirFilmeAgrupados(grupo, listaFilmes);
            grupos.Add(grupo);

            return grupos;
        }

        private List<Filme> AtribuirFilmeAgrupados(Grupo grupo, List<Filme> listaFilmes)
        {
            List<Filme> filmes = new List<Filme>();
            var grupoFilmes = listaFilmes.Select(n => n).Where(n => n.Group == grupo.Id);

            foreach (Filme itemFilme in grupoFilmes)
            {
                filmes.Add(itemFilme);
            }

            return filmes;
        }

        #endregion


    }
}