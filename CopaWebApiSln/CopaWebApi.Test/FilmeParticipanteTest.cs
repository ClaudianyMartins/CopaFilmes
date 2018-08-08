using CopaWebApi.Controllers;
using CopaWebApi.Models;
using CopaWebApi.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;

namespace CopaWebApi.Test
{
    public class FilmeParticipanteTest
    {       
        [Fact]        
        public void Index_ReturnsAViewResult_WithFilmesParticipantes()
        {
            // Act     
            var controller = new HomeController(new FilmeRepository());
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            // Assert
            var items = Assert.IsType<FilmesParticipantes>(viewResult.Model);

            Assert.Equal(64, items.TotalGeralFilmes);
        }

        [Fact]
        public void Index_ReturnsAViewResult_WithMessage()
        {
            // Act     
            var controller = new HomeController(new FilmeRepository());

            var result = controller.Index();            

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result); 
            // Assert
            var filmes = Assert.IsType<FilmesParticipantes>(viewResult.Model);
            filmes.TotalFilmeSelecionados = 10;
            for (int i = 0; i < 10; i++)
            {
                filmes.FilmesB[i].Selected = true;
            }

            result = controller.Index(filmes);
            viewResult = Assert.IsType<ViewResult>(result);

            // Assert
            var items = Assert.IsType<FilmesParticipantes>(viewResult.Model);
            Assert.NotNull(items.Message);
        }
    }
}
