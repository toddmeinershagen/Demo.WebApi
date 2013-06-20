using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Demo.WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            ThrowRawException();
            return new string[] { "value1", "value2" };
        }

        private void ThrowRawException()
        {
            throw new NotImplementedException();
        }

        private void ThrowOnlyStatusCode()
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        private void ThrowStatusCodeWithContent()
        {
            var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content =
                        new StringContent(
                            "An error was encountered on the server.  If you continue to experience these issues please contact your administrator."),
                    //ReasonPhrase = "This is the reason you had an issue" //NOTE:  This overrides the default status code description.
                };

            throw new HttpResponseException(message);
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}