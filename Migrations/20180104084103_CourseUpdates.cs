using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PeerReviewWeb.Migrations
{
    public partial class CourseUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseJoinTags_AspNetUsers_ApplicationUserId",
                table: "CourseJoinTags");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseJoinTags_Courses_CourseId",
                table: "CourseJoinTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseJoinTags",
                table: "CourseJoinTags");

            migrationBuilder.RenameTable(
                name: "CourseJoinTags",
                newName: "CourseJoinTag");

            migrationBuilder.RenameIndex(
                name: "IX_CourseJoinTags_ApplicationUserId",
                table: "CourseJoinTag",
                newName: "IX_CourseJoinTag_ApplicationUserId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Courses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Courses",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "CourseJoinTag",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseJoinTag",
                table: "CourseJoinTag",
                columns: new[] { "CourseId", "ApplicationUserId" });

            migrationBuilder.CreateTable(
                name: "AssignmentStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AssignmentID = table.Column<Guid>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentStatus_Assignments_AssignmentID",
                        column: x => x.AssignmentID,
                        principalTable: "Assignments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentStatus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStatus_AssignmentID",
                table: "AssignmentStatus",
                column: "AssignmentID");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentStatus_UserId",
                table: "AssignmentStatus",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseJoinTag_AspNetUsers_ApplicationUserId",
                table: "CourseJoinTag",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseJoinTag_Courses_CourseId",
                table: "CourseJoinTag",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseJoinTag_AspNetUsers_ApplicationUserId",
                table: "CourseJoinTag");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseJoinTag_Courses_CourseId",
                table: "CourseJoinTag");

            migrationBuilder.DropTable(
                name: "AssignmentStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseJoinTag",
                table: "CourseJoinTag");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "CourseJoinTag");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "CourseJoinTag",
                newName: "CourseJoinTags");

            migrationBuilder.RenameIndex(
                name: "IX_CourseJoinTag_ApplicationUserId",
                table: "CourseJoinTags",
                newName: "IX_CourseJoinTags_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseJoinTags",
                table: "CourseJoinTags",
                columns: new[] { "CourseId", "ApplicationUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CourseJoinTags_AspNetUsers_ApplicationUserId",
                table: "CourseJoinTags",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseJoinTags_Courses_CourseId",
                table: "CourseJoinTags",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
