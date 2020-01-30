using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Univoting.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    Description = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    Logo = table.Column<byte[]>(nullable: true),
                    BrandColour = table.Column<string>(unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Moderators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    Badge = table.Column<int>(nullable: false),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moderators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moderators_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollingStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollingStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollingStations_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voters",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    IdentificationNumber = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    VotingStatus = table.Column<int>(nullable: false),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voters_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    RankId = table.Column<Guid>(nullable: false),
                    PriorityId = table.Column<Guid>(nullable: true),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_Priorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "Priorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    LastName = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    Picture = table.Column<byte[]>(nullable: true),
                    PositionId = table.Column<Guid>(nullable: false),
                    PriorityId = table.Column<Guid>(nullable: true),
                    RankId = table.Column<Guid>(nullable: false),
                    ElectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidates_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidates_Priorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "Priorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkippedVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VoterId = table.Column<Guid>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    PositionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkippedVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkippedVotes_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkippedVotes_Voters_VoterId",
                        column: x => x.VoterId,
                        principalTable: "Voters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VoterId = table.Column<Guid>(nullable: false),
                    CandidateId = table.Column<Guid>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    PositionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_Voters_VoterId",
                        column: x => x.VoterId,
                        principalTable: "Voters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PriorityId",
                table: "Candidates",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ElectionId",
                table: "Departments",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Moderators_ElectionId",
                table: "Moderators",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PollingStations_ElectionId",
                table: "PollingStations",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ElectionId",
                table: "Positions",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PriorityId",
                table: "Positions",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SkippedVotes_PositionId",
                table: "SkippedVotes",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SkippedVotes_VoterId",
                table: "SkippedVotes",
                column: "VoterId");

            migrationBuilder.CreateIndex(
                name: "IX_Voters_ElectionId",
                table: "Voters",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_CandidateId",
                table: "Votes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PositionId",
                table: "Votes",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VoterId",
                table: "Votes",
                column: "VoterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Moderators");

            migrationBuilder.DropTable(
                name: "PollingStations");

            migrationBuilder.DropTable(
                name: "SkippedVotes");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "Voters");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Elections");

            migrationBuilder.DropTable(
                name: "Priorities");
        }
    }
}
