using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestuKurimoSistema.Constants
{
    public class Secret
    {
        private const string secret = "veryveryverysecret";
        private static readonly object key = new object();
        public static string getSecret()
        {
            string scr = "";
            lock (key)
            {
                scr = secret;
            }
            return scr;
        }
    }
}
