using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGEP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloAsistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asistencias",
                columns: table => new
                {
                    id_asistencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_ingreso = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    hora_salida = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo_jornada = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    id_turno = table.Column<int>(type: "integer", nullable: false),
                    id_evento = table.Column<int>(type: "integer", nullable: true),
                    observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asistencias", x => x.id_asistencia);
                    table.ForeignKey(
                        name: "FK_asistencias_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asistencias_turnos_id_turno",
                        column: x => x.id_turno,
                        principalTable: "turnos",
                        principalColumn: "id_turno",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_fecha",
                table: "asistencias",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_fecha_id_turno",
                table: "asistencias",
                columns: new[] { "fecha", "id_turno" });

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_id_empleado_fecha_id_turno",
                table: "asistencias",
                columns: new[] { "id_empleado", "fecha", "id_turno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_id_turno",
                table: "asistencias",
                column: "id_turno");

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_tipo_jornada",
                table: "asistencias",
                column: "tipo_jornada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencias");
        }
    }
}
