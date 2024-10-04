using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Models;

namespace Sanssoussi.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqliteConnection _dbConnection;

        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<SanssoussiUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<SanssoussiUser> userManager, IConfiguration configuration)
        {
            this._logger = logger;
            this._userManager = userManager;
            this._dbConnection = new SqliteConnection(configuration.GetConnectionString("SanssoussiContextConnection"));
        }

        // Affiche la page d'acceuil du site
        public IActionResult Index()
        {
            this.ViewData["Message"] = "Parce que marcher devrait se faire SansSoussi";
            return this.View();
        }

        // Affiche les commentaires du user connecté
        [HttpGet]
        public async Task<IActionResult> Comments()
        {
            var comments = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.View(comments);
            }

            var query = "Select Comment from Comments where UserId =@userId";
            var cmd = new SqliteCommand(query, this._dbConnection);
            cmd.Parameters.AddWithValue("@userId", user.Id);
            this._dbConnection.Open();
            var rd = await cmd.ExecuteReaderAsync();

            while (rd.Read())
            {
                comments.Add(rd.GetString(0));
            }

            rd.Close();
            this._dbConnection.Close();

            return this.View(comments);
        }

        // Permet de sauvegarder un commentaire
        [HttpPost]
        public async Task<IActionResult> Comments(string comment)
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                throw new InvalidOperationException("Vous devez vous connecter");
            }

            comment = Utils.SanitizeInput(comment); // sécurité pour éviter les injections
            var query = "insert into Comments (UserId, CommentId, Comment) Values (@userId, @guid, @comment)";

            var cmd = new SqliteCommand(query, this._dbConnection);
            cmd.Parameters.AddWithValue("@userId", user.Id);
            cmd.Parameters.AddWithValue("@guid", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@comment", comment);
            this._dbConnection.Open();
            await cmd.ExecuteNonQueryAsync();

            return this.Ok("Commentaire ajouté");
        }

        public async Task<IActionResult> Search(string searchData)
        {
            var searchResults = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null || string.IsNullOrEmpty(searchData))
            {
                return this.View(searchResults);
            }

            var query = "Select Comment from Comments where UserId=@userId and Comment like @searchData";
            var cmd = new SqliteCommand(query, this._dbConnection);
            cmd.Parameters.AddWithValue("@userId", user.Id);
            cmd.Parameters.AddWithValue("@searchData", "%" + searchData + "%");

            this._dbConnection.Open();
            var rd = await cmd.ExecuteReaderAsync();
            while (rd.Read())
            {
                searchResults.Add(rd.GetString(0));
            }

            rd.Close();
            this._dbConnection.Close();
          
            return this.View(searchResults);
        }

        public IActionResult About()
        {
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }
    }
}