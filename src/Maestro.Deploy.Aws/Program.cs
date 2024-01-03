using System;
using Constructs;
using HashiCorp.Cdktf;

namespace Maestro.MyApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            App app = new App();

            new MainStack(app, "maestro-dotnet");
            
            
            app.Synth();
            Console.WriteLine("App synth complete");
        }
    }
}