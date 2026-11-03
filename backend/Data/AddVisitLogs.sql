-- Idempotent schema addition for visitor tracking.
-- Safe to run against the existing production PostgreSQL database (does not touch
-- existing tables/data). Mirrors the VisitLog EF Core entity in Models/VisitLog.cs.
--
-- Usage:
--   psql "$DATABASE_URL" -f backend/Data/AddVisitLogs.sql
--
-- Note: the backend also runs this same statement automatically on startup
-- (see Program.cs), so manual execution is optional/for ops visibility only.

CREATE TABLE IF NOT EXISTS visit_logs (
    "Id" BIGSERIAL PRIMARY KEY,
    "TimestampUtc" timestamp without time zone NOT NULL,
    "Path" varchar(512) NOT NULL,
    "QueryString" varchar(1024) NULL,
    "IpAddress" varchar(64) NULL,
    "UserAgent" varchar(512) NULL,
    "Referrer" varchar(1024) NULL,
    "VisitorId" varchar(64) NOT NULL,
    "ResponseTimeMs" bigint NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_visit_logs_TimestampUtc" ON visit_logs ("TimestampUtc");
CREATE INDEX IF NOT EXISTS "IX_visit_logs_VisitorId" ON visit_logs ("VisitorId");
CREATE INDEX IF NOT EXISTS "IX_visit_logs_Path" ON visit_logs ("Path");
