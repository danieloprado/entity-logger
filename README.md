Entity Logger
==============


Generate a log file with all entity changes

```csharp
var entityLogger = new EntityLogger(yourDbContext);
var log = entityLogger.GenerateLog();
```

Remeber: use it **before** SaveChanges:

```csharp
var entityLogger = new EntityLogger(yourDbContext);
var log = entityLogger.GenerateLog(); //All data

yourDbContext.SaveChanges();

var otherLog = entityLogger.GenerateLog(); //Empty
```

Also work with Transaction:

```csharp
using (var tran = yourDbContext.Database.BeginTransaction())
{
    var entityLogger = new EntityLogger(yourDbContext);
    var logBuilder = new StringBuilder();

    //Your code

    logBuilder.AppendLine(entityLogger.GenerateLog());
    yourDbContext.SaveChanges();

    //More code

    logBuilder.AppendLine(entityLogger.GenerateLog());
    yourDbContext.SaveChanges();

    tran.Commit();
    var fullLog = logBuilder.toString();
}
```

Output
-----


```
// For entity: {action}|Entity|{Entity Json}
// For property: {action}|{property}|{oldValue}|{newValue}

// Commom prop change:
Modified|Entity|Sale|{"Id":1,"CreatedDate": "2016-07-11T14:44:53.1692612Z","Price":30}
Modified|CreatedDate|2016-07-11T14:33:03.833|2016-07-11T14:44:53.1692612Z
Modified|Price|25|30

// Relationship:
Added|Entity|Sale_Gruops|{"KeyOne":"8","KeyTwo":"18"}

// Delete:
Deleted|Entity|Sale|{"Id":2,"CreatedDate": "2016-07-11T14:44:53.1692612Z","Price":30}

// Full report will be:
Modified|Entity|Sale|{"Id":1,"CreatedDate": "2016-07-11T14:44:53.1692612Z","Price":30}
Modified|CreatedDate|2016-07-11T14:33:03.833|2016-07-11T14:44:53.1692612Z
Modified|Price|25|30
Added|Entity|Sale_Gruops|{"KeyOne":"8","KeyTwo":"18"}
Deleted|Entity|Sale|{"Id":2,"CreatedDate": "2016-07-11T14:44:53.1692612Z","Price":30}
```
