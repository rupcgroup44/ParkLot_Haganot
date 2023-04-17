using ParkLot.Models;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParkLot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        // GET: api/<BorrowController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<BorrowController>/5
        [HttpGet("parking/{email}")]
        public List<string> GetParkingName(string email) //שליפת שם חניה של משתמש לפי מייל
        {
            Borrow b = new Borrow();
            return b.BorrowParkingName(email);
        }


        // GET api/<BorrowController>/5
        [HttpGet("GetMatch")]
        public List<object> Getmach(int IdBorrow) //שליפת כל המאצים של ההשאלה הנתונה
        {
            Borrow b = new Borrow();
            return b.GetMatchFromDB(IdBorrow);

        }

        [HttpGet("/DesirableBorrow/OrderDate/{desiredDate}/startTime/{startTime}/endTime/{endTime}/email/{email}/idB/{idB}")]
        public List<object> DesirableBorrow(DateTime desiredDate, DateTime startTime, DateTime endTime, string email, int idB) //שליפת חניות מתאימות לפי בקשת המשתמש
        {
            Borrow b = new Borrow();
            List<object> result = b.DesirableB(desiredDate, startTime, endTime, email,idB);
            return result;   
        }

        // POST api/<BorrowController>
        [HttpPost]
        public IActionResult Post([FromBody] Borrow borrow) //הכנסת השאלה 
        {
            int ANS;
            try
            {
                ANS = borrow.insertBorrowU();
            }
            catch
            {
                return StatusCode(500, "could not insert your borrow");
            }
            if (ANS >= 1)  //במידה והצליח להכניס יום בודד או טווח
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("/Match")]
        public IActionResult Post([FromBody] JsonElement data) //הכנסת הבקשה לטבלת המאץ לפי ההשאלה שבחר 
        {
            Borrow borrow = new Borrow();
            int idBorrow = Convert.ToInt32(data.GetProperty("IdBorrow").GetString()); //שליפת השדות של האוביקט שנשלח מצד לקוח
            int idRequest = data.GetProperty("IdRequest").GetInt32();
            int ANS = borrow.insertToAskFor(idBorrow, idRequest);
            if (ANS >= 1)  //במידה והצליח להכניס לטבלה
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }

        // PUT api/<BorrowController>/5
        [HttpPut("UPDATE")]
        public IActionResult put ([FromBody] Borrow Borrow) //עדכון פרטי השאלה
        {
            Borrow B = new Borrow();
            int ANS;
            try
            {
                ANS = B.UpdateBorrow(Borrow);
            }
            catch
            {
                return StatusCode(500, "The Server could not do The Update The Borrow");
            }

            if (ANS == 1)  //במידה והצליח להכניס יום בודד או טווח
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }

        // DELETE api/<BorrowController>/5
        [HttpDelete("Id/{idBorrow}/Mail/{mail}")]
        public IActionResult Delete(int idBorrow, string mail) //המייל של מי שמשאיל כלומר מי שמחובר
        {
            Borrow B = new Borrow();
            int ANS = B.DeleteBorrow(idBorrow, mail);
            if (ANS == 1)  //במידה והצליח להכניס יום בודד או טווח
            {
                return Ok(ANS);
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<BorrowController>/5
        [HttpGet("User/{email}")]
        public List<Borrow> GetBorrows(string email)//שליפת ההשאלות של המשתמש לפי המייל
        {
            Borrow b = new Borrow();
            return b.UserBorrows(email);

        }
    }
}
