using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIKnowledgeAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentIngestionMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ingestion_attempt_count",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ingestion_completed_at_utc",
                table: "documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ingestion_failure_reason",
                table: "documents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ingestion_started_at_utc",
                table: "documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ingestion_attempt_count",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "ingestion_completed_at_utc",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "ingestion_failure_reason",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "ingestion_started_at_utc",
                table: "documents");
        }
    }
}
