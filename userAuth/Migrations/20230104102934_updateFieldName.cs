using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace userAuth.Migrations
{
    public partial class updateFieldName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Registers",
                newName: "PersonalEmail");

            migrationBuilder.AlterColumn<string>(
                name: "Father_Name",
                table: "Registers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonalEmail",
                table: "Registers",
                newName: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "Father_Name",
                table: "Registers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);
        }
    }
}
