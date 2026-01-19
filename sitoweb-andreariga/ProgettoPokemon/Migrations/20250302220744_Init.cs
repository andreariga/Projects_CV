using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgettoPokemon.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    ImagesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Symbol = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Logo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Small = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Large = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.ImagesId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "legalities",
                columns: table => new
                {
                    LegalitiesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Unlimited = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expanded = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legalities", x => x.LegalitiesId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prices",
                columns: table => new
                {
                    PricesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => x.PricesId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "utenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utenti", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sets",
                columns: table => new
                {
                    SetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Identificatore = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Series = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrintedTotal = table.Column<int>(type: "int", nullable: true),
                    Total = table.Column<int>(type: "int", nullable: true),
                    PtcgoCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReleaseDate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImagesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sets", x => x.SetId);
                    table.ForeignKey(
                        name: "FK_sets_images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "images",
                        principalColumn: "ImagesId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "holofoils",
                columns: table => new
                {
                    HolofoilId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Low = table.Column<double>(type: "double", nullable: true),
                    Mid = table.Column<double>(type: "double", nullable: true),
                    High = table.Column<double>(type: "double", nullable: true),
                    Market = table.Column<double>(type: "double", nullable: true),
                    DirectLow = table.Column<double>(type: "double", nullable: true),
                    PricesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holofoils", x => x.HolofoilId);
                    table.ForeignKey(
                        name: "FK_holofoils_prices_PricesId",
                        column: x => x.PricesId,
                        principalTable: "prices",
                        principalColumn: "PricesId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tcgplayers",
                columns: table => new
                {
                    TcgplayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PricesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcgplayers", x => x.TcgplayerId);
                    table.ForeignKey(
                        name: "FK_tcgplayers_prices_PricesId",
                        column: x => x.PricesId,
                        principalTable: "prices",
                        principalColumn: "PricesId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "datas",
                columns: table => new
                {
                    DatasId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Supertype = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Artist = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rarity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvertedRetreatCost = table.Column<int>(type: "int", nullable: true),
                    SetId = table.Column<int>(type: "int", nullable: true),
                    LegalitiesId = table.Column<int>(type: "int", nullable: true),
                    ImagesId = table.Column<int>(type: "int", nullable: true),
                    TcgplayerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_datas", x => x.DatasId);
                    table.ForeignKey(
                        name: "FK_datas_images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "images",
                        principalColumn: "ImagesId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_datas_legalities_LegalitiesId",
                        column: x => x.LegalitiesId,
                        principalTable: "legalities",
                        principalColumn: "LegalitiesId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_datas_sets_SetId",
                        column: x => x.SetId,
                        principalTable: "sets",
                        principalColumn: "SetId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_datas_tcgplayers_TcgplayerId",
                        column: x => x.TcgplayerId,
                        principalTable: "tcgplayers",
                        principalColumn: "TcgplayerId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "attacks",
                columns: table => new
                {
                    AttackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvertedEnergyCost = table.Column<int>(type: "int", nullable: true),
                    Damage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attacks", x => x.AttackId);
                    table.ForeignKey(
                        name: "FK_attacks_datas_DatasId",
                        column: x => x.DatasId,
                        principalTable: "datas",
                        principalColumn: "DatasId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "weaknesses",
                columns: table => new
                {
                    WeaknessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DatasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weaknesses", x => x.WeaknessId);
                    table.ForeignKey(
                        name: "FK_weaknesses_datas_DatasId",
                        column: x => x.DatasId,
                        principalTable: "datas",
                        principalColumn: "DatasId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_attacks_DatasId",
                table: "attacks",
                column: "DatasId");

            migrationBuilder.CreateIndex(
                name: "IX_datas_ImagesId",
                table: "datas",
                column: "ImagesId");

            migrationBuilder.CreateIndex(
                name: "IX_datas_LegalitiesId",
                table: "datas",
                column: "LegalitiesId");

            migrationBuilder.CreateIndex(
                name: "IX_datas_SetId",
                table: "datas",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_datas_TcgplayerId",
                table: "datas",
                column: "TcgplayerId");

            migrationBuilder.CreateIndex(
                name: "IX_holofoils_PricesId",
                table: "holofoils",
                column: "PricesId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sets_ImagesId",
                table: "sets",
                column: "ImagesId");

            migrationBuilder.CreateIndex(
                name: "IX_tcgplayers_PricesId",
                table: "tcgplayers",
                column: "PricesId");

            migrationBuilder.CreateIndex(
                name: "IX_weaknesses_DatasId",
                table: "weaknesses",
                column: "DatasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attacks");

            migrationBuilder.DropTable(
                name: "holofoils");

            migrationBuilder.DropTable(
                name: "utenti");

            migrationBuilder.DropTable(
                name: "weaknesses");

            migrationBuilder.DropTable(
                name: "datas");

            migrationBuilder.DropTable(
                name: "legalities");

            migrationBuilder.DropTable(
                name: "sets");

            migrationBuilder.DropTable(
                name: "tcgplayers");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "prices");
        }
    }
}
