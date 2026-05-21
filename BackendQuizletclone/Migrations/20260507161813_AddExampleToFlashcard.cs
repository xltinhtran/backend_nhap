using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendQuizletclone.Migrations
{
    /// <inheritdoc />
    public partial class AddExampleToFlashcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC078F2F0EEB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudySets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudySet__3214EC07C3A4FD02", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySets_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flashcards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStarred = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    StudySetId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Example = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Flashcar__3214EC071539970A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flashcards_StudySets",
                        column: x => x.StudySetId,
                        principalTable: "StudySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FlashcardId = table.Column<int>(type: "int", nullable: false),
                    EaseFactor = table.Column<double>(type: "float", nullable: true, defaultValue: 2.5),
                    Interval = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Repetitions = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    NextReviewDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsStarred = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudyPro__3214EC073EE2B5A4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Progress_Flashcards",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Progress_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_StudySetId",
                table: "Flashcards",
                column: "StudySetId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyProgresses_FlashcardId",
                table: "StudyProgresses",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyProgresses_UserId",
                table: "StudyProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySets_UserId",
                table: "StudySets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E40DBCD2A0",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534D3BD4728",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyProgresses");

            migrationBuilder.DropTable(
                name: "Flashcards");

            migrationBuilder.DropTable(
                name: "StudySets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
