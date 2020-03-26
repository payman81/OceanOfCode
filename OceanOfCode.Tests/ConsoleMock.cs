using System;
using System.Collections.Generic;

namespace OceanOfCode.Tests
{
    public class ConsoleMock : IConsole
    {
        private readonly Queue<string> _inputQueue = new Queue<string>();
        public readonly List<string> RecordedActions = new List<string>();

        public void Record(string instruction)
        {
            _inputQueue.Enqueue(instruction);
        }
        public string ReadLine()
        {
            return _inputQueue.Dequeue();
        }

        public void WriteLine(object output)
        {
            RecordedActions.Add(output.ToString());
        }

        public void Debug(object obj)
        {
            Console.WriteLine($"DEBUG: {obj}");
        }
    }
}