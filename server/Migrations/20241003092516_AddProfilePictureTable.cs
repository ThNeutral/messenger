using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfilePicture",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Base64EncodedImage = table.Column<string>(type: "Text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePicture", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_ProfilePicture_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfilePicture");
        }
    }
}
