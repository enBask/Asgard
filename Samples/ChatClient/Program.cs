using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var chatClient = new ChatClient("127.0.0.1", 8899);
            chatClient.Run();
        }
    }
}
