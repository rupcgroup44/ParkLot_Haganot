using ParkLot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParkLot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        // GET: api/<EmailController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<EmailController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<EmailController>
        [HttpPost]
        public int Post([FromBody] JsonElement mail ) //שליחת סיסמאת ביניין למשתמש שנרשם
        {
            string EmailAddress = mail.GetProperty("EmailAddress").GetString();
            string Bilding = mail.GetProperty("BuildingCode").GetString();
            Email E= new Email();
            return E.RegistrationEmail(EmailAddress, Bilding);
        }

        [HttpPost("/request")]
        public int PostNewRequest([FromBody] JsonElement objForMail) //שליחת מייל למשתמש שמחכה לו בקשה חדשה
        {
            string EmailAddress = objForMail.GetProperty("Email").GetString();
            int idBorrow = Convert.ToInt32(objForMail.GetProperty("IdBorrow").GetString()); //שליפת השדות של האוביקט שנשלח מצד לקוח
            Email E = new Email();
            return E.RequestEmail(EmailAddress, idBorrow);
        }


        // PUT api/<EmailController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EmailController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
