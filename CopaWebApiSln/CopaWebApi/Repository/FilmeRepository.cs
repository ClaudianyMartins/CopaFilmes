using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CopaWebApi.Models;

namespace CopaWebApi.Repository
{
    public class FilmeRepository : IFilmeRepository
    {       
        public static List<Filme> filmes;

        public List<Filme> ListarFilmes()
        {
            try
            {
                filmes = new List<Filme>();

                RunAsync().Wait();

                return filmes;
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possível exibir os filmes para votação: " + ex.Message);
            }
        }

        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new System.Uri("https://copa-filmes.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/filmes");

                if (response.IsSuccessStatusCode)
                {  //GET
                    List<Filme> filmeAux = await response.Content.ReadAsAsync<List<Filme>>();

                    foreach (var item in filmeAux)
                    {
                        filmes.Add(new Filme(item.Id, item.PrimaryTitle, item.Year, item.AverageRating));
                    }
                }
            }
        }

    }
}
