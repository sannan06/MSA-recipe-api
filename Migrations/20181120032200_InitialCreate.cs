using Microsoft.EntityFrameworkCore.Migrations;

namespace RecipeBank.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    publisher = table.Column<string>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    image_url = table.Column<string>(nullable: true),
                    steps = table.Column<string>(nullable: true),
                    tag = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeItem");
        }
    }
}
