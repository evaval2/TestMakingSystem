using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace TestuKurimoSistema.OAuth2
{
    public class Error
    {
        private HttpResponse _response;
        private Stream _stream;
        public Error(HttpResponse response)
        {
            _response = response;
            _stream = _response.Body;
        }
        public Error()
        {
            _response = null;
            _stream = null;
        }
        public void Set(HttpResponse response)
        {
            _response = response;
            _stream = _response.Body;
        }
        public Dictionary<string, string> WriteError(int status_code, string error, string state, string error_description = "")
        {
            Dictionary<string, string> errorObject = new Dictionary<string, string>();
            if (checkCode(status_code))
            _response.StatusCode = status_code;
            if (!error.Equals(""))
                errorObject.Add("error", error);
            if (!error_description.Equals(""))
                errorObject.Add("error_description", error_description);
            if (!state.Equals(""))
                errorObject.Add("state", state);
            return errorObject;
        }
        private bool checkCode(int code)
        {
            var codes = new int[] { 100, 101, 102, 103,
                                    200, 201, 202, 203, 204, 205, 206, 207, 208, 226,
                                    300, 301, 302, 303, 304, 305, 306, 307, 308,
                                    400, 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417, 418, 421, 422, 423, 424, 425, 426, 428, 429, 431, 451,
                                    500, 501, 502, 503, 504, 505, 506, 507, 508, 510, 511 };
            if (codes.Contains(code))
                return true;
            return false;
        }
    }
}
