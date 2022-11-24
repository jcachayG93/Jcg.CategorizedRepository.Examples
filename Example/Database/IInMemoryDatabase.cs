namespace Example.Database;

public interface IInMemoryDatabase
{
    void UpsertAndCommit(UpsertOperation[] operations);
}