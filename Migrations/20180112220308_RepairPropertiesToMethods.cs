using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PeerReviewWeb.Migrations
{
    public partial class RepairPropertiesToMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Courses_CourseID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Courses_CourseID1",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Courses_CourseID1",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_CourseID1",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CourseID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CourseID1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CourseID1",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CourseID1",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseID1",
                table: "Assignments",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseID",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseID1",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CourseID1",
                table: "Assignments",
                column: "CourseID1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CourseID",
                table: "AspNetUsers",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CourseID1",
                table: "AspNetUsers",
                column: "CourseID1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Courses_CourseID",
                table: "AspNetUsers",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Courses_CourseID1",
                table: "AspNetUsers",
                column: "CourseID1",
                principalTable: "Courses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Courses_CourseID1",
                table: "Assignments",
                column: "CourseID1",
                principalTable: "Courses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
