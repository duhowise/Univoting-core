# Univoting Akka.NET Refactor

This project refactors the original Univoting application from a traditional Entity Framework-based approach to an event-sourced Akka.NET application using Akka.Persistence.

## Model Relationships Analysis

Based on the analysis of `UnivotingContext.cs`, the following relationships were identified:

### Core Entities and Relationships

1. **Election** (Root Aggregate)
   - Contains: Voters, Positions, Moderators, PollingStations, Departments
   - Acts as the boundary for consistency and transactions

2. **Voter**
   - Belongs to an Election
   - Can cast many Votes
   - Can skip many positions (SkippedVotes)
   - Has voting status (Pending, InProgress, Voted)

3. **Position**
   - Belongs to an Election
   - Has a Priority for ordering
   - Contains many Candidates
   - Receives many Votes and SkippedVotes

4. **Candidate**
   - Belongs to a Position
   - Has a Priority for ballot ordering
   - Can receive many Votes

5. **Vote**
   - Links Voter, Candidate, and Position
   - Immutable once cast
   - Timestamped

6. **SkippedVote**
   - Links Voter and Position (when voter chooses to skip)
   - Immutable once recorded
   - Timestamped

## Akka.NET Architecture

### Event Sourcing Pattern

The refactored application follows Event Sourcing principles:

- **Commands** represent intent to change state
- **Events** represent what actually happened
- **State** is rebuilt by replaying events
- **Snapshots** provide performance optimization

### Actor Hierarchy

```
VotingSupervisorActor
└── GenericChildPerEntityParent (Elections)
    └── ElectionActor (per election instance)
```

### Key Components

1. **ElectionActor** - Persistent actor managing a single election
2. **VotingSupervisorActor** - Routes commands to appropriate election actors
3. **GenericChildPerEntityParent** - Creates child actors on demand
4. **ElectionMessageExtractor** - Routes messages based on election ID

## Project Structure

```
Src/Univoting.Akka/
├── Actors/
│   ├── ElectionActor.cs          # Core persistent actor
│   └── VotingSupervisorActor.cs  # Supervisor actor
├── Messages/
│   ├── VotingCommands.cs         # Command messages
│   └── VotingEvents.cs           # Event messages
├── Utility/
│   ├── GenericChildPerEntityParent.cs
│   ├── ElectionMessageExtractor.cs
│   └── SqliteEventStore.cs
├── Program.cs                    # Application entry point
└── appsettings.json             # Configuration

Tests/Univoting.Akka.Tests/
└── ElectionActorTests.cs        # Unit tests
```

## Commands and Events

### Commands (Intent)
- `CreateElection` - Create a new election
- `AddPosition` - Add a position to an election
- `AddCandidate` - Add a candidate to a position
- `RegisterVoter` - Register a voter for an election
- `CastVote` - Cast a vote for a candidate
- `SkipVote` - Skip voting for a position

### Events (Facts)
- `ElectionCreated` - Election was created
- `PositionAdded` - Position was added
- `CandidateAdded` - Candidate was added
- `VoterRegistered` - Voter was registered
- `VoteCast` - Vote was cast
- `VoteSkipped` - Vote was skipped

## Key Features

### Business Logic Enforcement
- Prevents duplicate voting by same voter for same position
- Validates voter registration before allowing votes
- Ensures referential integrity (voter, candidate, position existence)
- Maintains audit trail through event sourcing

### Performance Optimizations
- Periodic snapshots to reduce recovery time
- In-memory state for fast query processing
- Child-per-entity pattern for scalability

### Persistence
- SQLite-based event journal and snapshot store
- Auto-initialization of database schema
- Event replay for state reconstruction

## Running the Application

### Prerequisites
- .NET 9.0 SDK
- SQLite

### Build and Run
```bash
# Build the solution
dotnet build

# Run the Akka.NET application
cd Src/Univoting.Akka
dotnet run

# Run tests
cd Tests/Univoting.Akka.Tests
dotnet test
```

### Demo Workflow

The application includes a comprehensive demo that:

1. Creates an election
2. Adds positions (President, Vice President, Secretary)
3. Adds candidates to positions
4. Registers voters
5. Casts votes and skips positions
6. Displays vote counts and election information

## Benefits of Akka.NET Refactor

### Scalability
- Actor model provides natural concurrency
- Child-per-entity pattern scales with data
- Message-driven architecture supports distributed systems

### Consistency
- Event sourcing provides strong consistency guarantees
- Commands are processed sequentially per election
- No lost updates or race conditions

### Auditability
- Complete audit trail through event log
- Events are immutable and timestamped
- Full history of all operations

### Reliability
- Actor supervision for fault tolerance
- Event replay for recovery
- Persistent state survives application restarts

### Performance
- In-memory state for fast queries
- Snapshots reduce recovery time
- Asynchronous message processing

## Comparison with Original Architecture

| Aspect | Original (EF) | Akka.NET Refactor |
|--------|---------------|-------------------|
| Data Model | Relational/CRUD | Event Sourced |
| Concurrency | Database locks | Actor isolation |
| Scalability | Vertical scaling | Horizontal scaling |
| Audit Trail | External logging | Built-in events |
| State Management | Database queries | In-memory state |
| Consistency | Database transactions | Actor message ordering |
| Recovery | Database backups | Event replay |

## Testing Strategy

The test suite covers:
- Election creation and management
- Position and candidate management
- Voter registration and status updates
- Vote casting with business rule validation
- Vote skipping functionality
- Duplicate vote prevention
- Data retrieval operations

Tests use Akka.TestKit for reliable actor testing with proper message expectations and timeouts.

## Future Enhancements

1. **Clustering** - Add Akka.Cluster.Sharding for true distributed voting
2. **Read Models** - Implement CQRS with specialized read models
3. **Real-time Updates** - Add SignalR for live vote count updates
4. **Security** - Add authentication and authorization
5. **Monitoring** - Add Akka.Monitoring for observability
6. **Database Options** - Support for PostgreSQL, SQL Server
7. **Event Versioning** - Handle event schema evolution
8. **Snapshots** - Advanced snapshot strategies and compaction
