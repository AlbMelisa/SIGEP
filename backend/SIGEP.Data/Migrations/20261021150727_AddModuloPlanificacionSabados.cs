using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SIGEP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloPlanificacionSabados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "planificaciones_sabado",
                columns: table => new
                {
                    id_planificacion_sabado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_sabado = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_confirmacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_creacion = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_confirmacion = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planificaciones_sabado", x => x.id_planificacion_sabado);
                    table.ForeignKey(
                        name: "FK_planificaciones_sabado_usuarios_id_usuario_confirmacion",
                        column: x => x.id_usuario_confirmacion,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planificaciones_sabado_usuarios_id_usuario_creacion",
                        column: x => x.id_usuario_creacion,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalles_planificacion_sabado",
                columns: table => new
                {
                    id_detalle_planificacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_planificacion_sabado = table.Column<int>(type: "integer", nullable: false),
                    id_empleado = table.Column<int>(type: "integer", nullable: false),
                    id_turno = table.Column<int>(type: "integer", nullable: false),
                    es_modificacion_manual = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    motivo_modificacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalles_planificacion_sabado", x => x.id_detalle_planificacion);
                    table.ForeignKey(
                        name: "FK_detalles_planificacion_sabado_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "empleados",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalles_planificacion_sabado_planificaciones_sabado_id_pla~",
                        column: x => x.id_planificacion_sabado,
                        principalTable: "planificaciones_sabado",
                        principalColumn: "id_planificacion_sabado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalles_planificacion_sabado_turnos_id_turno",
                        column: x => x.id_turno,
                        principalTable: "turnos",
                        principalColumn: "id_turno",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_detalles_planificacion_sabado_id_empleado",
                table: "detalles_planificacion_sabado",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_planificacion_sabado_id_planificacion_sabado_id_em~",
                table: "detalles_planificacion_sabado",
                columns: new[] { "id_planificacion_sabado", "id_empleado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalles_planificacion_sabado_id_turno",
                table: "detalles_planificacion_sabado",
                column: "id_turno");

            migrationBuilder.CreateIndex(
                name: "IX_planificaciones_sabado_estado",
                table: "planificaciones_sabado",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_planificaciones_sabado_fecha_sabado",
                table: "planificaciones_sabado",
                column: "fecha_sabado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planificaciones_sabado_id_usuario_confirmacion",
                table: "planificaciones_sabado",
                column: "id_usuario_confirmacion");

            migrationBuilder.CreateIndex(
                name: "IX_planificaciones_sabado_id_usuario_creacion",
                table: "planificaciones_sabado",
                column: "id_usuario_creacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalles_planificacion_sabado");

            migrationBuilder.DropTable(
                name: "planificaciones_sabado");
        }
    }
}
