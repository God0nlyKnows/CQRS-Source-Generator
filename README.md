# CQRS Source Generator

It's a Queries and Commands generator in CQRS pattern using [MediatR](https://github.com/jbogard/MediatR), [ErrorOr](https://github.com/amantinband/error-or) and [AutoMapper](https://github.com/AutoMapper/AutoMapper) 



## Goals

- [ ] Generating Queries and Commands
- [ ] Adjusting generated classes trough attributes  
- [ ] Making low-level improvments to make generator fast and lightweight
- [ ] Deploying package on NuGet
- [ ] Snapshot tests and benchmarks
- [ ] Enable caching and ignore unchanged files

## Example usage

```cs
    [GenerateQueries]
    [GenerateCommand(Method: "Create", DTO: typeof(AddClientCommand), MapperConfig: mapperConfig)]
    public interface IClientRepository
    {
        Task<Client> Create(ClientId clientId, string name, string surname, string email);
        Task<IReadOnlyList<Client>> GetAll();
        Task<Client> Get(ClientId id);
        Task Update(Client client);
        Task Delete(ClientId id);
        Task Add(Client client);

    }
}
```
