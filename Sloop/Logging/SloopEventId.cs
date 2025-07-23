namespace Sloop.Logging;

public enum SloopEventId
{
    // Common
    ExecutingSql = 1002,

    // Migration
    StartingMigration = 2001,

    SkippingMigration = 2000,

    MigrationDone = 2003,

    // GetItem
    GetItemStart = 3000,

    GetItemHit = 3002,

    GetItemMiss = 3003,

    // PurgeExpiredItems
    PurgeStart = 4000,

    PurgeBatchDone = 4001,

    PurgeFinished = 4002,

    PurgeCancelled = 4003,

    // RefreshItem
    RefreshStart = 5000,

    RefreshUpdated = 5001,

    RefreshNoop = 5002,

    // RemoveItem
    RemoveStart = 6000,

    RemoveDeleted = 6001,

    RemoveNoop = 6002,

    // SetItem
    SetStart = 7000,

    SetStored = 7001,

    SetNoop = 7002,

    // TryAcquireLock
    TryLockStart = 8000,

    TryLockAcquired = 8001,

    TryLockDenied = 8002
}