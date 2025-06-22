using System.Net.Http; //Can use this or UnityWebRequest
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Privy
{
    internal interface IHttpRequestHandler
    {
        //Generic Response/Request Data values, to place responsibility of data validation on the delegators/repositories
        Task<string> SendRequestAsync(string path, string jsonData, Dictionary<string, string> customHeaders = null);
    }
}