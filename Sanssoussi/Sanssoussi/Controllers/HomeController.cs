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

        private readonly UserManager<SanssoussiUser> _userManager; // Gestion de l'authentification, cookie?

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

            var user = await this._userManager.GetUserAsync(this.User); // Cookie pour aller chercher le user id
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
            var user = await this._userManager.GetUserAsync(this.User); // Cookie
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

            var user = await this._userManager.GetUserAsync(this.User); // Cookie
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

        // Affiche une page avec un identifiant unique de la requête qui a échoué
        // Mauvaise configuration de sécurité?
        // Est-ce qu'on donne trop d'information?
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Emails()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Emails(object form)
        {
            var searchResults = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User); // Cookie
            var roles = await this._userManager.GetRolesAsync(user); // Cookie
            if (roles.Contains("admin"))
            {
                var cmd = new SqliteCommand("select Email from AspNetUsers", this._dbConnection);
                this._dbConnection.Open();
                var rd = await cmd.ExecuteReaderAsync();
                while (rd.Read())
                {
                    searchResults.Add(rd.GetString(0));
                }

                rd.Close();

                this._dbConnection.Close();
            }

            return this.Json(searchResults);
        }
    }
}