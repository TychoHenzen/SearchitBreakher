namespace SearchitLibrary.Abstractions;

public interface IConstantProvider
{
    Constants Get();
    void Save(Constants constants);
}