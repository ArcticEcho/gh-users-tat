using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using GhUsersTat.Models;
using GhUsersTat.Services;

namespace GhUsersTat.Controllers
{
    public class UserSearchController : Controller
    {
        private readonly IGithubQueryService _githubQueryService;

        public UserSearchController(IGithubQueryService githubQueryService)
        {
            _githubQueryService = githubQueryService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> StartSearch(UserSearch userSearch)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", userSearch);
            }

            var user = await _githubQueryService.GetUser(userSearch.Username);

            return PartialView("UserSearchResultsPartial", user);
        }
    }
}