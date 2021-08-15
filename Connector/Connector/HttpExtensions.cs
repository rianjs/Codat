using System.Net;

namespace Connector
{
    public static class HttpExtensions
    {
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            var intVal = (int) statusCode;
            return intVal >= 200 && intVal <= 299;
        }
    }
}