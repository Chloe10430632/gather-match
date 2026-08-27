using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_gather_match.Migrations
{
    /// <inheritdoc />
    public partial class EnableRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserClaims\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserLogins\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserTokens\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"ActivityTypes\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Cities\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Districts\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Activities\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"DateOptions\" ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"PlaceOptions\" ENABLE ROW LEVEL SECURITY;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserClaims\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserLogins\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUserTokens\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"ActivityTypes\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Cities\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Districts\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"Activities\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"DateOptions\" DISABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE \"PlaceOptions\" DISABLE ROW LEVEL SECURITY;");
        }
    }
}
