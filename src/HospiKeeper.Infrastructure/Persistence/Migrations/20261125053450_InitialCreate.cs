using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace HospiKeeper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "doc_sequence",
                columns: table => new
                {
                    prefix = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    doc_date = table.Column<DateTime>(type: "date", nullable: false),
                    seq = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doc_sequence", x => new { x.prefix, x.doc_date });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "room_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    base_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_type", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    password = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    real_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    role = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "room",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    room_no = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    floor = table.Column<int>(type: "int", nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false),
                    type_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    beds = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    min_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    deposit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    has_window = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    has_bath = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_room_type_type_id",
                        column: x => x.type_id,
                        principalTable: "room_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "op_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    user_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    action = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    target = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    detail = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    biz_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_op_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_op_log_sys_user_user_id",
                        column: x => x.user_id,
                        principalTable: "sys_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkin_record",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    checkin_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    room_no = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    room_type_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    guest_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    guest_phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    guest_idcard = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: false),
                    checkin_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    plan_out_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actual_out_time = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    origin_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    deposit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    extend_hours = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    op_user_id = table.Column<int>(type: "int", nullable: true),
                    operator_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkin_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_checkin_record_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checkin_record_sys_user_op_user_id",
                        column: x => x.op_user_id,
                        principalTable: "sys_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkin_companion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    checkin_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    idcard = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkin_companion", x => x.id);
                    table.ForeignKey(
                        name: "FK_checkin_companion_checkin_record_checkin_id",
                        column: x => x.checkin_id,
                        principalTable: "checkin_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkout_bill",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    billing_mode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    bill_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    checkin_id = table.Column<int>(type: "int", nullable: false),
                    checkin_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    room_no = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    room_type_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    guest_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    checkin_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    checkout_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nights = table.Column<int>(type: "int", nullable: false),
                    room_fee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    extra_total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    damage = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    other_fee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    deposit_received = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    deposit_used = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    diff = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    op_user_id = table.Column<int>(type: "int", nullable: true),
                    operator_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_bill", x => x.id);
                    table.ForeignKey(
                        name: "FK_checkout_bill_checkin_record_checkin_id",
                        column: x => x.checkin_id,
                        principalTable: "checkin_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checkout_bill_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checkout_bill_sys_user_op_user_id",
                        column: x => x.op_user_id,
                        principalTable: "sys_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "extra_charge",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    checkin_id = table.Column<int>(type: "int", nullable: false),
                    item = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    charge_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    op_user_id = table.Column<int>(type: "int", nullable: true),
                    operator_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_charge", x => x.id);
                    table.ForeignKey(
                        name: "FK_extra_charge_checkin_record_checkin_id",
                        column: x => x.checkin_id,
                        principalTable: "checkin_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extra_charge_sys_user_op_user_id",
                        column: x => x.op_user_id,
                        principalTable: "sys_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reservation",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    res_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    guest_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    guest_phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    room_no = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    room_type_name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    arrive_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    checkin_id = table.Column<int>(type: "int", nullable: true),
                    checkin_no = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    op_user_id = table.Column<int>(type: "int", nullable: true),
                    operator_name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservation_checkin_record_checkin_id",
                        column: x => x.checkin_id,
                        principalTable: "checkin_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reservation_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservation_sys_user_op_user_id",
                        column: x => x.op_user_id,
                        principalTable: "sys_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkout_bill_line",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    checkout_bill_id = table.Column<int>(type: "int", nullable: false),
                    item = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_bill_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_checkout_bill_line_checkout_bill_checkout_bill_id",
                        column: x => x.checkout_bill_id,
                        principalTable: "checkout_bill",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_companion_checkin_id",
                table: "checkin_companion",
                column: "checkin_id");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_checkin_no",
                table: "checkin_record",
                column: "checkin_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_checkin_time",
                table: "checkin_record",
                column: "checkin_time");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_guest_idcard",
                table: "checkin_record",
                column: "guest_idcard");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_guest_name",
                table: "checkin_record",
                column: "guest_name");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_guest_phone",
                table: "checkin_record",
                column: "guest_phone");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_op_user_id",
                table: "checkin_record",
                column: "op_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_room_id",
                table: "checkin_record",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_room_no",
                table: "checkin_record",
                column: "room_no");

            migrationBuilder.CreateIndex(
                name: "IX_checkin_record_status",
                table: "checkin_record",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_bill_no",
                table: "checkout_bill",
                column: "bill_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_checkin_id",
                table: "checkout_bill",
                column: "checkin_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_checkout_time",
                table: "checkout_bill",
                column: "checkout_time");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_guest_name",
                table: "checkout_bill",
                column: "guest_name");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_op_user_id",
                table: "checkout_bill",
                column: "op_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_room_id",
                table: "checkout_bill",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_room_no",
                table: "checkout_bill",
                column: "room_no");

            migrationBuilder.CreateIndex(
                name: "IX_checkout_bill_line_checkout_bill_id",
                table: "checkout_bill_line",
                column: "checkout_bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_charge_checkin_id",
                table: "extra_charge",
                column: "checkin_id");

            migrationBuilder.CreateIndex(
                name: "IX_extra_charge_op_user_id",
                table: "extra_charge",
                column: "op_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_op_log_action",
                table: "op_log",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_op_log_created_at",
                table: "op_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_op_log_user_id",
                table: "op_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_arrive_time",
                table: "reservation",
                column: "arrive_time");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_checkin_id",
                table: "reservation",
                column: "checkin_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_guest_phone",
                table: "reservation",
                column: "guest_phone");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_op_user_id",
                table: "reservation",
                column: "op_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_res_no",
                table: "reservation",
                column: "res_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_room_id",
                table: "reservation",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_status",
                table: "reservation",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_room_floor",
                table: "room",
                column: "floor");

            migrationBuilder.CreateIndex(
                name: "IX_room_room_no",
                table: "room",
                column: "room_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_status",
                table: "room",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_room_type_id",
                table: "room",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_type_name",
                table: "room_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_username",
                table: "sys_user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkin_companion");

            migrationBuilder.DropTable(
                name: "checkout_bill_line");

            migrationBuilder.DropTable(
                name: "doc_sequence");

            migrationBuilder.DropTable(
                name: "extra_charge");

            migrationBuilder.DropTable(
                name: "op_log");

            migrationBuilder.DropTable(
                name: "reservation");

            migrationBuilder.DropTable(
                name: "checkout_bill");

            migrationBuilder.DropTable(
                name: "checkin_record");

            migrationBuilder.DropTable(
                name: "room");

            migrationBuilder.DropTable(
                name: "sys_user");

            migrationBuilder.DropTable(
                name: "room_type");
        }
    }
}
