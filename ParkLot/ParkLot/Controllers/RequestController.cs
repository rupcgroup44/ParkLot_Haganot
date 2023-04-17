using ParkLot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParkLot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        // GET: api/<RequestController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<RequestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // POST api/<RequestController>
        [HttpPost]
        public int Post([FromBody] Request request)//הכנסת בקשה
        {
            int ANS = request.InsertRequestU();
            return ANS;//מחזיר את מספר הבקשה
        }

        // PUT api/<RequestController>/5
        [HttpPut("updateRequestForBorrow")] //אישור בקשה להשאלה מטבלת מזמינים
        public IActionResult Put([FromBody] JsonElement data)
        {
            Request request = new Request();
            int idBorrow = Convert.ToInt32(data.GetProperty("IdBorrow").GetString()); //שליפת השדות של האוביקט שנשלח מצד לקוח
            int idRequest = Convert.ToInt32(data.GetProperty("IdRequest").GetString());
            int ANS;
            try
            {
                ANS = request.UpdateReqForBorrow(idBorrow, idRequest);
            }
            catch
            {

                return StatusCode(500, "The server could not Update the Request");
            }

            if (ANS >= 0)  //במידה והצליח להכניס לטבלה
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }

        // PUT api/<RequestController>/5
        [HttpPut("updateRequestcancealedForBorrow")]  //ביטול בקשה להשאלה מטבלת מזמינים
        public IActionResult PutCancealed([FromBody] JsonElement data)
        {
            Request request = new Request();
            int idBorrow = Convert.ToInt32(data.GetProperty("IdBorrow").GetString()); //שליפת השדות של האוביקט שנשלח מצד לקוח
            int idRequest = Convert.ToInt32(data.GetProperty("IdRequest").GetString());
            int ANS = request.UpdateCancealedForBorrow( idBorrow,idRequest,0);
            if (ANS >= 0)  //במידה והצליח להכניס לטבלה
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }


        // DELETE api/<RequestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
