using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dttbidsmxbb.Migrations
{
    /// <inheritdoc />
    public partial class InformationRelationAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Informations_MilitaryRanks_MilitaryRankId",
                table: "Informations");

            migrationBuilder.AddForeignKey(
                name: "FK_Informations_MilitaryRanks_MilitaryRankId",
                table: "Informations",
                column: "MilitaryRankId",
                principalTable: "MilitaryRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Informations_MilitaryRanks_MilitaryRankId",
                table: "Informations");

            migrationBuilder.AddForeignKey(
                name: "FK_Informations_MilitaryRanks_MilitaryRankId",
                table: "Informations",
                column: "MilitaryRankId",
                principalTable: "MilitaryRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
