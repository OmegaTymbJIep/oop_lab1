namespace Lab1.Core.Services;

public interface IFileSavePicker
{
    public Task<string> PickAsync(string defaultFileName);
}