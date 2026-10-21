using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SIGEP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloPersonal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funciones",
                columns: table => new
                {
                    id_funcion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funciones", x => x.id_funcion);
                });

            migrationBuilder.CreateTable(
                name: "regimenes",
                columns: table => new
                {
                    id_regimen = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regimenes", x => x.id_regimen);
                });

            migrationBuilder.CreateTable(
                name: "turnos",
                columns: table => new
                {
                    id_turno = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    id_regimen = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos", x => x.id_turno);
                    table.ForeignKey(
                        name: "FK_turnos_regimenes_id_regimen",
                        column: x => x.id_regimen,
                        principalTable: "regimenes",
                        principalColumn: "id_regimen",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleados",
                columns: table => new
                {
                    id_empleado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    id_funcion = table.Column<int>(type: "integer", nullable: false),
                    id_regimen = table.Column<int>(type: "integer", nullable: false),
                    id_turno_habitual = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleados", x => x.id_empleado);
                    table.ForeignKey(
                        name: "FK_empleados_funciones_id_funcion",
                        column: x => x.id_funcion,
                        principalTable: "funciones",
                        principalColumn: "id_funcion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_empleados_regimenes_id_regimen",
                        column: x => x.id_regimen,
                        principalTable: "regimenes",
                        principalColumn: "id_regimen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_empleados_turnos_id_turno_habitual",
                        column: x => x.id_turno_habitual,
                        principalTable: "turnos",
                        principalColumn: "id_turno",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "funciones",
                columns: new[] { "id_funcion", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Mantenimiento, limpieza y servicios urbanos", "Servicios" },
                    { 2, "Control vial y seguridad vehicular", "Tránsito" },
                    { 3, "Gestión documental y atención administrativa", "Administración" },
                    { 4, "Eventos culturales y espacios públicos", "Cultura" },
                    { 5, "Asistencia comunitaria y programas sociales", "Desarrollo Social" }
                });

            migrationBuilder.InsertData(
                table: "regimenes",
                columns: new[] { "id_regimen", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Jornada ordinaria lunes a viernes y sábados rotativos por mitades", "Incorporado" },
                    { 2, "Turnos rotativos con descanso obligatorio de 48 hs tras turno noche", "Histórico" }
                });

            migrationBuilder.InsertData(
                table: "turnos",
                columns: new[] { "id_turno", "descripcion", "hora_fin", "hora_inicio", "nombre", "id_regimen" },
                values: new object[,]
                {
                    { 1, "Turno mañana Incorporado (06:00 - 12:00)", new TimeOnly(12, 0, 0), new TimeOnly(6, 0, 0), "Mañana", 1 },
                    { 2, "Turno tarde Incorporado (12:00 - 18:00)", new TimeOnly(18, 0, 0), new TimeOnly(12, 0, 0), "Tarde", 1 },
                    { 3, "Turno noche Incorporado (18:00 - 24:00)", new TimeOnly(23, 59, 59), new TimeOnly(18, 0, 0), "Noche", 1 },
                    { 4, "Turno mañana Histórico (07:00 - 13:00)", new TimeOnly(13, 0, 0), new TimeOnly(7, 0, 0), "Mañana", 2 },
                    { 5, "Turno siesta Histórico (13:00 - 19:00)", new TimeOnly(19, 0, 0), new TimeOnly(13, 0, 0), "Siesta", 2 },
                    { 6, "Turno noche Histórico (19:00 - 07:00) con 48 hs de descanso posterior", new TimeOnly(7, 0, 0), new TimeOnly(19, 0, 0), "Noche", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_empleados_documento",
                table: "empleados",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleados_estado",
                table: "empleados",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_id_funcion",
                table: "empleados",
                column: "id_funcion");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_id_regimen",
                table: "empleados",
                column: "id_regimen");

            migrationBuilder.CreateIndex(
                name: "IX_empleados_id_turno_habitual",
                table: "empleados",
                column: "id_turno_habitual");

            migrationBuilder.CreateIndex(
                name: "IX_funciones_nombre",
                table: "funciones",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regimenes_nombre",
                table: "regimenes",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_id_regimen",
                table: "turnos",
                column: "id_regimen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empleados");

            migrationBuilder.DropTable(
                name: "funciones");

            migrationBuilder.DropTable(
                name: "turnos");

            migrationBuilder.DropTable(
                name: "regimenes");
        }
    }
}
