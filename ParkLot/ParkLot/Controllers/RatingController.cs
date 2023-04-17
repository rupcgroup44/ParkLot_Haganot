using ParkLot.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParkLot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        // GET: api/<RatingController>
        [HttpGet]
        public List<object> Get(string mail)// הוצאת הארכיון של המשתמש
        {
            Rating R= new Rating();
            return R.GetArchiveData(mail);
        }

        // POST api/<RatingController>
        [HttpPost]
        public int Post([FromBody] Rating R)  //חישוב והכנסה דירוג מחדש
        {
            int num=R.InsertRating();
            return num;
        }

        // PUT api/<RatingController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RatingController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
