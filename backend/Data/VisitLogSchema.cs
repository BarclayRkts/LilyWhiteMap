using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Data;

/// <summary>
/// The backend bootstraps its schema via <see cref="DatabaseFacade.EnsureCreatedAsync"/> rather
/// than EF Core migrations. That call only builds the full schema for a brand-new database, so on
/// an already-existing database it won't add tables introduced later (like visit_logs). This
/// idempotent statement keeps visit_logs in sync without requiring a full migration/history setup.
/// See backend/Data/AddVisitLogs.sql for the equivalent script to run manually via psql.
/// </summary>
public static class VisitLogSchema
{
    private const string EnsureTableSql = """
        CREATE TABLE IF NOT EXISTS visit_logs (
            "Id" BIGSERIAL PRIMARY KEY,
            "TimestampUtc" timestamp without time zone NOT NULL,
            "Path" varchar(512) NOT NULL,
            "QueryString" varchar(1024) NULL,
            "IpAddress" varchar(64) NULL,
            "UserAgent" varchar(512) NULL,
            "Referrer" varchar(1024) NULL,
            "VisitorId" varchar(64) NOT NULL,
            "ResponseTimeMs" bigint NOT NULL,
            "IsSuspicious" boolean NOT NULL DEFAULT FALSE
        );
        ALTER TABLE visit_logs ADD COLUMN IF NOT EXISTS "IsSuspicious" boolean NOT NULL DEFAULT FALSE;
        CREATE INDEX IF NOT EXISTS "IX_visit_logs_TimestampUtc" ON visit_logs ("TimestampUtc");
        CREATE INDEX IF NOT EXISTS "IX_visit_logs_VisitorId" ON visit_logs ("VisitorId");
        CREATE INDEX IF NOT EXISTS "IX_visit_logs_Path" ON visit_logs ("Path");
        CREATE INDEX IF NOT EXISTS "IX_visit_logs_IsSuspicious" ON visit_logs ("IsSuspicious");
        """;

    public static Task EnsureCreatedAsync(LilyWhiteMapDbContext db, CancellationToken cancellationToken = default) =>
        db.Database.ExecuteSqlRawAsync(EnsureTableSql, cancellationToken);
}
