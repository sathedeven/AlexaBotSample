using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SimpleEchoBot.Controllers
{
    [Route("api/alexa")]
    public class AlexaController : ApiController
    {
       
        [HttpGet]
        [HttpHead]
        public IHttpActionResult root()
        {
            return this.Ok("Im Alive");
        }

        // POST api/values
        //[RequireHttps]
       
        [HttpPost]
        public HttpResponseMessage Post()
        {
            var speechlet = new SampleSessionSpeechlet();
            return speechlet.GetResponse(this.Request);
        }
    }
}