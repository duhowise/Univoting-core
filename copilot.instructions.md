# Building Applications with Akka.NET & Akka.Persistence

This document provides comprehensive instructions for building applications using Akka.NET and Akka.Persistence based on the banking application example in this codebase.

## Table of Contents

1. [Project Structure](#project-structure)
2. [Dependencies and NuGet Packages](#dependencies-and-nuget-packages)
3. [Configuration Setup](#configuration-setup)
4. [Persistent Actors](#persistent-actors)
5. [Event Sourcing Pattern](#event-sourcing-pattern)
6. [Actor Hierarchy and Supervision](#actor-hierarchy-and-supervision)
7. [Message Design](#message-design)
8. [Testing Strategies](#testing-strategies)
9. [Hosting and Dependency Injection](#hosting-and-dependency-injection)
10. [Database Setup](#database-setup)
11. [Best Practices](#best-practices)

## Project Structure

```
src/
├── Akka.Bank.Console/
│   ├── Actors/
│   │   ├── AccountActor.cs          # Persistent actor implementation
│   │   └── BankSupervisorActor.cs   # Supervisor actor
│   ├── Messages/
│   │   ├── AccountCommands.cs       # Command messages
│   │   └── AccountEvents.cs         # Event messages
│   ├── Utility/
│   │   ├── EntityMessageExtractor.cs     # Message routing utilities
│   │   ├── AccountMessageExtractor.cs
│   │   ├── GenericChildPerEntityParent.cs
│   │   └── SqliteEventStore.cs      # Database initialization
│   ├── Program.cs                   # Application entry point
│   ├── appsettings.json            # Configuration
│   └── Akka.Bank.Console.csproj    # Project file
tests/
└── Akka.Bank.Tests/
    ├── AccountActorTests.cs         # Unit tests
    └── Akka.Bank.Tests.csproj      # Test project file
```

## Dependencies and NuGet Packages

### Core Akka.NET Packages

```xml
<PackageReference Include="Akka.Hosting" Version="1.5.44" />
<PackageReference Include="Akka.Persistence" Version="1.5.44" />
<PackageReference Include="Akka.Persistence.Hosting" Version="1.5.44" />
<PackageReference Include="Akka.Persistence.Sql.Hosting" Version="1.5.44" />
<PackageReference Include="Akka.Cluster.Sharding" Version="1.5.44" />
```

### Database Packages

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.119" />
```

### Hosting and Configuration

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.0-preview.5.25277.114" />
```

### Testing Packages

```xml
<PackageReference Include="Akka.TestKit.Xunit2" Version="1.5.44" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"/>
<PackageReference Include="xunit.abstractions" Version="2.0.3" />
```

## Configuration Setup

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=akka_bank.db;Mode=ReadWriteCreate;"
  },
  "Akka": {
    "loglevel": "INFO",
    "loggers": ["Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog"]
  }
}
```

### Hosting Configuration

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddAkka("BankSystem", (builder, provider) =>
        {
            builder
                .WithActors((system, registry) =>
                {
                    var supervisor = system.ActorOf(
                        Props.Create(() => new BankSupervisorActor()),
                        "bank-supervisor");
                    registry.Register<BankSupervisorActor>(supervisor);
                })
                .WithSqlPersistence(
                    journal =>
                    {
                        journal.AutoInitialize = true;
                        journal.ConnectionString = context.Configuration.GetConnectionString("DefaultConnection");
                        journal.DatabaseOptions = JournalDatabaseOptions.Sqlite;
                        journal.ProviderName = "Microsoft.Data.Sqlite";
                    },
                    snapshot =>
                    {
                        snapshot.AutoInitialize = true;
                        snapshot.ConnectionString = context.Configuration.GetConnectionString("DefaultConnection");
                        snapshot.DatabaseOptions = SnapshotDatabaseOptions.Sqlite;
                        snapshot.ProviderName = "Microsoft.Data.Sqlite";
                    });
        });
    })
    .Build();
```

## Persistent Actors

### Basic Structure

A persistent actor inherits from `ReceivePersistentActor` and implements:

```csharp
public class AccountActor : ReceivePersistentActor
{
    // State variables
    private decimal _balance;
    private readonly string _accountId;
    private string _accountName;
    private int _eventCount;
    private bool _initialized;

    // Required persistence ID
    public override string PersistenceId => _accountId;

    public AccountActor(string accountId)
    {
        _accountId = accountId;
        
        // Define command handlers
        Command<AccountCommand>(HandleCommand);
        
        // Define event recovery handlers
        Recover<AccountEvent>(ApplyEvent);
        
        // Handle snapshot recovery
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is AccountSnapshot snap)
            {
                _balance = snap.Balance;
                _accountName = snap.AccountName;
                _eventCount = snap.EventCount;
                _initialized = !string.IsNullOrEmpty(_accountName);
            }
        });
    }
}
```

### Key Components

1. **PersistenceId**: Unique identifier for the actor's journal
2. **Command Handlers**: Process incoming commands and validate business rules
3. **Event Handlers**: Apply events to update actor state
4. **Snapshot Recovery**: Restore actor state from snapshots

## Event Sourcing Pattern

### Commands vs Events

**Commands** represent intent to change state:

```csharp
public abstract record AccountCommand;
public record CreateAccount(string accountId, string AccountName) : AccountCommand;
public record Deposit(string IntoAccountId, decimal Amount) : AccountCommand;
public record Withdraw(string FromAccountId, decimal Amount) : AccountCommand;
```

**Events** represent what actually happened:

```csharp
public abstract record AccountEvent;
public record AccountCreated(string AccountId, string AccountName) : AccountEvent;
public record MoneyDeposited(decimal Amount) : AccountEvent;
public record MoneyWithdrawn(decimal Amount) : AccountEvent;
```

### Command Handling Pattern

```csharp
private void HandleCommand(AccountCommand cmd)
{
    switch (cmd)
    {
        case CreateAccount create:
            // Validate business rules
            if (_initialized)
            {
                Sender.Tell(new Status.Failure(new InvalidOperationException("Account already exists.")));
                return;
            }
            
            // Persist event and apply changes
            Persist(new AccountCreated(_accountId, create.AccountName), evt =>
            {
                ApplyEvent(evt);
                Sender.Tell(Status.Success.Instance);
            });
            break;
            
        case Deposit deposit:
            // Validation
            if (!_initialized)
            {
                Sender.Tell(new Status.Failure(new InvalidOperationException("Account not created.")));
                return;
            }
            if (deposit.Amount <= 0)
            {
                Sender.Tell(new Status.Failure(new ArgumentException("Deposit amount must be positive.")));
                return;
            }
            
            // Persist and apply
            Persist(new MoneyDeposited(deposit.Amount), evt =>
            {
                ApplyEvent(evt);
                Sender.Tell(Status.Success.Instance);
            });
            break;
    }
}
```

### Event Application

```csharp
private void ApplyEvent(AccountEvent evt)
{
    switch (evt)
    {
        case AccountCreated created:
            _accountName = created.AccountName;
            _initialized = true;
            break;
        case MoneyDeposited deposited:
            _balance += deposited.Amount;
            break;
        case MoneyWithdrawn withdrawn:
            _balance -= withdrawn.Amount;
            break;
    }
    
    _eventCount++;
    
    // Optional: Save snapshots periodically
    if (_eventCount % SnapshotInterval == 0)
    {
        SaveSnapshot(new AccountSnapshot(_balance, _accountName, _eventCount));
    }
}
```

## Actor Hierarchy and Supervision

### Supervisor Actor

```csharp
public class BankSupervisorActor : UntypedActor
{
    private readonly IActorRef _accountsParent;

    public BankSupervisorActor()
    {
        _accountsParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new AccountMessageExtractor(),
                id => Props.Create(() => new AccountActor(id))
            ),
            "accounts-parent");
    }

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case CreateAccount:
            case Deposit:
            case Withdraw:
            case Transfer:
            case GetBalance:
                _accountsParent.Forward(message);
                break;
            default:
                Unhandled(message);
                break;
        }
    }
}
```

### Message Routing

```csharp
public class AccountMessageExtractor : EntityMessageExtractor
{
    public AccountMessageExtractor() : base(
        ExtractEntityId,
        ExtractEntityMessage,
        "account-shard")
    {
    }

    private static string ExtractEntityId(object message)
    {
        return message switch
        {
            CreateAccount create => create.accountId,
            Deposit deposit => deposit.IntoAccountId,
            Withdraw withdraw => withdraw.FromAccountId,
            Transfer transfer => transfer.FromAccountId,
            GetBalance getBalance => getBalance.FromAccountId,
            _ => null
        };
    }

    private static object ExtractEntityMessage(object message)
    {
        return message;
    }
}
```

## Message Design

### Command Design Principles

1. **Include Entity ID**: Every command should contain the target entity identifier
2. **Immutable Records**: Use C# records for immutability
3. **Clear Intent**: Command names should clearly express the intended action
4. **Validation Data**: Include all data needed for business rule validation

### Event Design Principles

1. **Past Tense**: Events represent completed actions
2. **Minimal Data**: Include only essential information
3. **Immutable**: Events should never change once persisted
4. **Serializable**: Ensure events can be persisted and recovered

## Testing Strategies

### Unit Testing with TestKit

```csharp
public class AccountActorTests : TestKit.Xunit2.TestKit
{
    private readonly ITestOutputHelper _output;

    public AccountActorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private IActorRef CreateAccountsParent() => Sys.ActorOf(GenericChildPerEntityParent.Props(
        new AccountMessageExtractor(),
        id => Props.Create(() => new AccountActor(id))
    ));

    [Fact]
    public void AccountActor_Should_Create_Account_And_Deposit_Withdraw()
    {
        // Arrange
        const string accountId = "test-1";
        var parent = CreateAccountsParent();
        
        // Act & Assert - Create Account
        parent.Tell(new CreateAccount(accountId, "Test Account"));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Deposit
        parent.Tell(new Deposit(accountId, 100));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Withdraw
        parent.Tell(new Withdraw(accountId, 40));
        ExpectMsg<Status.Success>();
        
        // Act & Assert - Check Balance
        parent.Tell(new GetBalance(accountId));
        var balance = ExpectMsg<decimal>();
        Assert.Equal(60, balance);
    }
}
```

### Testing Best Practices

1. **Test Business Logic**: Focus on business rules and edge cases
2. **Use TestKit**: Leverage Akka.TestKit for actor testing
3. **Isolated Tests**: Each test should be independent
4. **Clear Assertions**: Use descriptive assertions and error messages

## Hosting and Dependency Injection

### Host Configuration

```csharp
using (host)
{
    await host.StartAsync();
    
    // Initialize database
    SqliteEventStore.InitializeDatabase();

    // Get actor references from DI
    var requiredActor = host.Services.GetRequiredService<IRequiredActor<BankSupervisorActor>>();
    var supervisor = requiredActor.ActorRef;

    // Use actors
    var accountId = "account-1";
    var balance = await supervisor.Ask<decimal>(new GetBalance(accountId), TimeSpan.FromSeconds(10));
    
    Console.WriteLine($"Account balance: {balance}");
    
    await host.StopAsync();
}
```

### Service Registration

- Use `AddAkka()` to register the actor system
- Register actors with `WithActors()`
- Configure persistence with `WithSqlPersistence()`

## Database Setup

### SQLite Configuration

```csharp
public static class SqliteEventStore
{
    public static void InitializeDatabase()
    {
        var dbPath = "akka_bank.db";
        var directory = Path.GetDirectoryName(Path.GetFullPath(dbPath));
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
```

### Persistence Configuration

- **AutoInitialize**: Automatically create database tables
- **ConnectionString**: Define database connection
- **DatabaseOptions**: Specify database type (SQLite, SQL Server, etc.)
- **ProviderName**: Database provider specification

## Best Practices

### Actor Design

1. **Single Responsibility**: Each actor should have one clear purpose
2. **Immutable State**: Keep actor state immutable where possible
3. **Fail Fast**: Validate early and provide clear error messages
4. **Idempotent Operations**: Design operations to be safely retryable

### Event Sourcing

1. **Never Modify Events**: Events are immutable historical facts
2. **Snapshot Strategy**: Use snapshots for performance on large event streams
3. **Event Versioning**: Plan for event schema evolution
4. **Business Logic in Commands**: Keep business rules in command handlers

### Error Handling

1. **Supervision Strategy**: Define how actors should handle failures
2. **Status Messages**: Use `Status.Success` and `Status.Failure` for responses
3. **Validation**: Validate all inputs before persisting events
4. **Graceful Degradation**: Handle partial system failures gracefully

### Performance

1. **Batching**: Consider batching operations where appropriate
2. **Snapshots**: Use snapshots to reduce recovery time
3. **Sharding**: Use Akka.Cluster.Sharding for scalability
4. **Connection Pooling**: Configure database connection pooling

### Security

1. **Input Validation**: Validate all external inputs
2. **Authorization**: Implement proper authorization checks
3. **Audit Trail**: Events provide natural audit capabilities
4. **Encryption**: Consider encrypting sensitive event data

This framework provides a solid foundation for building event-sourced applications with Akka.NET and Akka.Persistence. The banking example demonstrates key patterns that can be adapted to other domains.
