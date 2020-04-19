namespace OceanOfCode
{
    public interface IConsole
    {
        string ReadLine();
        void WriteLine(object output);
        void Debug(object obj);
    }
}