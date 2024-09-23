using System;
using Craw;

namespace ConsoleEngine
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var app = new CrawController
            {
                ShouldRun = true
            };
            app.Run();
        }
    }
}