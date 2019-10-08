using FsnApi.Models;
using Google.Cloud.Datastore.V1;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace FsnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {


        
        [HttpPost("register")]
        public ActionResult<object> Register([FromBody]User user)
        {
              
            Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fsndigital-cred.json"));

            DatastoreDb db = DatastoreDb.Create("fsndigital", "Users", DatastoreClient.Create());

            string kind = "user";

            KeyFactory keyFactory = db.CreateKeyFactory(kind);
            Key key = keyFactory.CreateKey(user.Email);

            var task = new Entity
            {
                Key = key,
                ["TypeId"] = user.TypeId,
                ["UserName"] = user.UserName,
                ["Email"] = user.Email,
                ["Password"] = user.Password,
            };

            Entity check = db.Lookup(task.Key);

            if (check != null)
                return Ok(new { succeeded = false, message = "An account with that email already exists." });



            using (DatastoreTransaction transaction = db.BeginTransaction())
            {
                transaction.Upsert(task);
                transaction.Commit();
            }

            return Ok(new { succeeded = true });


        }


        [HttpPost("login")]
        public IActionResult Login([FromBody]Credentials credentials)
        {

            Environment.SetEnvironmentVariable(
                    "GOOGLE_APPLICATION_CREDENTIALS",
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fsndigital-cred.json"));

            DatastoreDb db = DatastoreDb.Create("fsndigital", "Users", DatastoreClient.Create());

            string kind = "user";

            KeyFactory keyFactory = db.CreateKeyFactory(kind);
            Key key = keyFactory.CreateKey(credentials.Email);

            var task = new Entity
            {
                Key = key,
            };

            Entity check = db.Lookup(task.Key);


            if (check == null)
                return Ok(new { succeeded = false, message = "No such user." });

            if(credentials.Password == (string)check["Password"])
    
             return Ok(new { succeeded = true, typeid = (int)check["TypeId"], username = (string)check["UserName"] });

              else return Ok(new { succeeded = false, message = "Incorrect Password" });


        }


        [HttpPost("recipebyIngredients")]
        public IActionResult RecipebyIngredients([FromBody]SearchIngredients searchingredients)
        {

            var client = new RestClient("https://api.spoonacular.com");
            var request = new RestRequest("recipes/search", Method.GET);
            request.AddParameter("apiKey", "4e982b8d88284d728c7c546a14ceed39");
            request.AddParameter("query", searchingredients.query);
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            return Ok(content); 
            
        }

        
        [HttpPost("recipebyNutrition")]
        public IActionResult RecipebyNutrition([FromBody]searchingNutrition searchingnutrition)
        {
            var client = new RestClient("https://api.spoonacular.com");
            var request = new RestRequest("recipes/findByNutrients", Method.GET);
            request.AddParameter("apiKey", "4e982b8d88284d728c7c546a14ceed39");
            request.AddParameter("maxCalories", searchingnutrition.maxCalories);
            request.AddParameter("minCalories", searchingnutrition.minCalories);
            request.AddParameter("maxCarbs", searchingnutrition.maxCarbs);
            request.AddParameter("minCarbs", searchingnutrition.minCarbs);
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            return Ok(content);
        }

        // GET api/user/test
        [HttpGet("test")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

    }
}
