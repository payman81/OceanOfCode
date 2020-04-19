using System;

namespace OceanOfCode
{
    sealed class RealConsole : IConsole
    {
        private static readonly Lazy<RealConsole> Lazy = new Lazy<RealConsole>(() => new RealConsole());

        public static RealConsole Instance => Lazy.Value;

        private RealConsole()
        {
            
        }

        public string ReadLine()
        {
            return System.Console.ReadLine();
        }
        public void WriteLine(object output)
        {
            System.Console.WriteLine(output);
        }
        public void Debug(object obj)
        {
            System.Console.Error.WriteLine(obj);
        }
    }
}