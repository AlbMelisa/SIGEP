using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGEP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    id_evento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    lugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_confirmacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_creacion = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_confirmacion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.id_evento);
                    table.ForeignKey(
                        name: "FK_eventos_usuarios_id_usuario_confirmacion",
                        column: x => x.id_usuario_confirmacion,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eventos_usuarios_id_usuario_creacion",
                        column: x => x.id_usuario_creacion,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "turnos_evento",
                columns: table => new
                {
                    id_turno_evento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_evento = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    cupo_requerido = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos_evento", x => x.id_turno_evento);
                    table.ForeignKey(
                        name: "FK_turnos_evento_eventos_id_evento",
                        column: x => x.id_evento,
                        principalTable: "eventos",
                        principalColumn: "id_evento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones_evento",
                columns: table => new
                {
                    id_asignacion_evento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_turno_evento = table.Column<int>(type: "integer", nullable: false),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    puntaje_calculado = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    es_asignacion_manual = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    motivo_modificacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones_evento", x => x.id_asignacion_evento);
                    table.ForeignKey(
                        name: "FK_asignaciones_evento_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignaciones_evento_turnos_evento_id_turno_evento",
                        column: x => x.id_turno_evento,
                        principalTable: "turnos_evento",
                        principalColumn: "id_turno_evento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_evento_estado",
                table: "asignaciones_evento",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_evento_id_empleado",
                table: "asignaciones_evento",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_evento_id_turno_evento_id_empleado",
                table: "asignaciones_evento",
                columns: new[] { "id_turno_evento", "id_empleado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventos_estado",
                table: "eventos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_fecha_inicio_fecha_fin",
                table: "eventos",
                columns: new[] { "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_eventos_id_usuario_confirmacion",
                table: "eventos",
                column: "id_usuario_confirmacion");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_id_usuario_creacion",
                table: "eventos",
                column: "id_usuario_creacion");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_evento_fecha",
                table: "turnos_evento",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_evento_id_evento",
                table: "turnos_evento",
                column: "id_evento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones_evento");

            migrationBuilder.DropTable(
                name: "turnos_evento");

            migrationBuilder.DropTable(
                name: "eventos");
        }
    }
}
