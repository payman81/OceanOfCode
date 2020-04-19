namespace OceanOfCode
{
    public interface INavigator
    {
        NavigationResult Next((int, int) currentPosition);
        void Reset();
        (int, int) First();
    }
}