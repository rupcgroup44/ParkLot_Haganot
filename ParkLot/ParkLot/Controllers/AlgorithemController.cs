using ParkLot.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParkLot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlgorithemController : ControllerBase
    {
        // GET api/<AlgorithemController>/5
        [HttpGet("{idBorrow}")]  //מחזיר את השיבוץ החכם
        public List<object> Get(int idBorrow)
        {
            SmartAlgorithem SA = new SmartAlgorithem();

            try
            {
                return SA.getBorrowDetails(idBorrow);
            }
            catch (Exception ex)//במידה ולא מצליח מחזיר רשימה ריקה
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                // Return an empty list
                return new List<object>();
            }
        }



        // POST api/<AlgorithemController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AlgorithemController>/5
        [HttpPut("ChooseSmartAlgorithm")]   //במידה ולחץ על כפתור שיבוץ חכם
        public int Put( [FromBody] int idBorrow)
        {
            SmartAlgorithem SA = new SmartAlgorithem();
            return SA.UpdateToSmartAlgorithem(idBorrow);
        }


        // DELETE api/<AlgorithemController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
