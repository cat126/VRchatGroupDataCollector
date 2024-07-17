using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace VRchatDataCollector
{
    public class MemoryProtection
    {
        public static dataProtector Setup()
        {
            // add data protection services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            var services = serviceCollection.BuildServiceProvider();

            // create an instance of MyClass using the service provider
            var instance = ActivatorUtilities.CreateInstance<dataProtector>(services);
            //instance.RunSample();
            return instance;
        }

        public class dataProtector
        {
            IDataProtector _protector;

            // the 'provider' parameter is provided by DI
            public dataProtector(IDataProtectionProvider provider)
            {
                _protector = provider.CreateProtector("Contoso.dataProtector.v1");
            }

            public String Protect(string input)
            {
                // protect the payload
                string protectedPayload = _protector.Protect(input);
                //Console.WriteLine($"Protect returned: {protectedPayload}");
                return protectedPayload;

            }
            public String RemoveProtection(string protectedPayload)
            {

                // unprotect the payload
                string unprotectedPayload = _protector.Unprotect(protectedPayload);
                return unprotectedPayload;
            }
        }
    }
}
