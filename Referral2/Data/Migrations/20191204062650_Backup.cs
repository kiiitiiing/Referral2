using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Referral2.Data.Migrations
{
    public partial class Backup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Province",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Province", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transportation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Transportation = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transportation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Muncity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muncity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muncity_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Barangay",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceId = table.Column<int>(nullable: false),
                    MuncityId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    OldTarget = table.Column<int>(nullable: false),
                    Target = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barangay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barangay_Muncity",
                        column: x => x.MuncityId,
                        principalTable: "Muncity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Barangay_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facility",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Abbrevation = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Address = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    BarangayId = table.Column<int>(nullable: true),
                    MuncityId = table.Column<int>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: true),
                    Contact = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Email = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Picture = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    ChiefHospital = table.Column<string>(maxLength: 100, nullable: true),
                    HospitalLevel = table.Column<int>(nullable: false),
                    HospitalType = table.Column<string>(maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facility_Barangay",
                        column: x => x.BarangayId,
                        principalTable: "Barangay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facility_Muncity",
                        column: x => x.MuncityId,
                        principalTable: "Muncity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facility_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    LastName = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Sex = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    CivilStatus = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    PhicId = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    PhicStatus = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Address = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    BarangayId = table.Column<int>(nullable: true),
                    MuncityId = table.Column<int>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: true),
                    TsekapPatient = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Barangay",
                        column: x => x.BarangayId,
                        principalTable: "Barangay",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patients_Muncity",
                        column: x => x.MuncityId,
                        principalTable: "Muncity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patients_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    Level = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    FacilityId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    Firstname = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Middlename = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Title = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Contact = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    MuncityId = table.Column<int>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: false),
                    AccreditationNo = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    AccreditationValidity = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    LicenseNo = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Prefix = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Picture = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Designation = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Status = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    LastLogin = table.Column<DateTime>(nullable: false),
                    LoginStatus = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    RemeberToken = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Facility",
                        column: x => x.FacilityId,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Muncity",
                        column: x => x.MuncityId,
                        principalTable: "Muncity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Baby",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BabyId = table.Column<int>(nullable: false),
                    MotherId = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false),
                    GestationalAge = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baby", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patient_BabyId",
                        column: x => x.BabyId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patient_MotherId",
                        column: x => x.MotherId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    DateReferred = table.Column<DateTime>(nullable: false),
                    DateSeen = table.Column<DateTime>(nullable: false),
                    ReferredFrom = table.Column<int>(nullable: true),
                    ReferredTo = table.Column<int>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    ReferringMd = table.Column<int>(nullable: true),
                    ActionMd = table.Column<int>(nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activity_User_To",
                        column: x => x.ActionMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activity_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activity_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activity_Facility_From",
                        column: x => x.ReferredFrom,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activity_Facilit_To",
                        column: x => x.ReferredTo,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activity_User_From",
                        column: x => x.ReferringMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    SenderId = table.Column<int>(nullable: true),
                    RecieverId = table.Column<int>(nullable: true),
                    Message = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_MDReciever",
                        column: x => x.RecieverId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_MDSender",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incoming",
                columns: table => new
                {
                    Code = table.Column<string>(unicode: false, nullable: true),
                    PatientId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(unicode: false, nullable: true),
                    ReferringMd = table.Column<int>(unicode: false, nullable: false),
                    ActionMd = table.Column<int>(unicode: false, nullable: false),
                    Type = table.Column<string>(unicode: false, nullable: true),
                    DateAction = table.Column<DateTime>(nullable: false),
                    ReferredFrom = table.Column<int>(nullable: false),
                    ReferredFromNavigationId = table.Column<int>(nullable: true),
                    ReferringMdNavigationId = table.Column<int>(nullable: true),
                    ActionMdNavigationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Incoming_User_ActionMdNavigationId",
                        column: x => x.ActionMdNavigationId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incoming_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incoming_Facility_ReferredFromNavigationId",
                        column: x => x.ReferredFromNavigationId,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incoming_User_ReferringMdNavigationId",
                        column: x => x.ReferringMdNavigationId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Login",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Login = table.Column<DateTime>(nullable: false),
                    Logout = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Login", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Login_User_Id",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientForm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    ReferringFacilityId = table.Column<int>(nullable: true),
                    ReferredTo = table.Column<int>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    TimeReferred = table.Column<DateTime>(nullable: false),
                    TimeTransferred = table.Column<DateTime>(nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    CaseSummary = table.Column<string>(type: "text", nullable: true),
                    RecommendSummary = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    Reason = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    ReferringMd = table.Column<int>(nullable: true),
                    ReferredMd = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientForm_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientForm_Patient_Id",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientForm_User_ReferredMd",
                        column: x => x.ReferredMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientForm_Facility_To",
                        column: x => x.ReferredTo,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientForm_Facility_From",
                        column: x => x.ReferringFacilityId,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientForm_User_ReferringMd",
                        column: x => x.ReferringMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PregnantForm",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    ReferringFacility = table.Column<int>(nullable: false),
                    ReferredBy = table.Column<int>(nullable: false),
                    RecordNo = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    ReferredDate = table.Column<DateTime>(nullable: false),
                    ReferredTo = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    ArrivalDate = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    HealthWorker = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    PatientWomanId = table.Column<int>(nullable: false),
                    WomanReason = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    WomanMajorFindings = table.Column<string>(type: "text", nullable: true),
                    WomanBeforeTreatment = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    WomanBeforeGivenTime = table.Column<DateTime>(nullable: true),
                    WomanDuringTransport = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    WomanTransportGivenTime = table.Column<DateTime>(nullable: true),
                    WomanInformationGiven = table.Column<string>(type: "text", nullable: true),
                    PatientBabyId = table.Column<int>(nullable: true),
                    BabyReason = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    BabyMajorFindings = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    BabyLastFeed = table.Column<DateTime>(nullable: true),
                    BabyBeforeTreatment = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    BabyBeforeGivenTime = table.Column<DateTime>(nullable: true),
                    BabyDuringTransport = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    BabyTransportGivenTime = table.Column<DateTime>(nullable: true),
                    BabyInformationGiven = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PregnantForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PregnantForm_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PregnantForm_BabyId",
                        column: x => x.PatientBabyId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PregnantForm_MotherId",
                        column: x => x.PatientWomanId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PregnantForm_User_From",
                        column: x => x.ReferredBy,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PregnantForm_Facility_To",
                        column: x => x.ReferredTo,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PregnantForm_Facility_From",
                        column: x => x.ReferringFacility,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tracking",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    DateReferred = table.Column<DateTime>(nullable: false),
                    DateTransferred = table.Column<DateTime>(nullable: false),
                    DateAccepted = table.Column<DateTime>(nullable: false),
                    DateArrived = table.Column<DateTime>(nullable: false),
                    DateSeen = table.Column<DateTime>(nullable: false),
                    Transportation = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    ReferredFrom = table.Column<int>(nullable: true),
                    ReferredTo = table.Column<int>(nullable: true),
                    DepartmentId = table.Column<int>(nullable: true),
                    ReferringMd = table.Column<int>(nullable: true),
                    ActionMd = table.Column<int>(nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Type = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    WalkIn = table.Column<string>(unicode: false, maxLength: 255, nullable: true),
                    FormId = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracking_User_ActionMd",
                        column: x => x.ActionMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracking_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracking_Patients",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracking_Facility_From",
                        column: x => x.ReferredFrom,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracking_Facility_To",
                        column: x => x.ReferredTo,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tracking_User_ReferringMd",
                        column: x => x.ReferringMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issue",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingId = table.Column<int>(nullable: false),
                    Issue = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    Status = table.Column<string>(unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issue_Tracking",
                        column: x => x.TrackingId,
                        principalTable: "Tracking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seen",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingId = table.Column<int>(nullable: false),
                    FacilityId = table.Column<int>(nullable: false),
                    UserMd = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seen_Facility",
                        column: x => x.FacilityId,
                        principalTable: "Facility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seen_Tracking",
                        column: x => x.TrackingId,
                        principalTable: "Tracking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seen_User_Id",
                        column: x => x.UserMd,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_ActionMd",
                table: "Activity",
                column: "ActionMd");

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_DepartmentId",
                table: "Activity",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_PatientId",
                table: "Activity",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_ReferredFrom",
                table: "Activity",
                column: "ReferredFrom");

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_ReferredTo",
                table: "Activity",
                column: "ReferredTo");

            migrationBuilder.CreateIndex(
                name: "IX_IIActivity_ReferringMd",
                table: "Activity",
                column: "ReferringMd");

            migrationBuilder.CreateIndex(
                name: "IX_Baby_BabyId",
                table: "Baby",
                column: "BabyId");

            migrationBuilder.CreateIndex(
                name: "AK_IIBaby_Id",
                table: "Baby",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Baby_MotherId",
                table: "Baby",
                column: "MotherId");

            migrationBuilder.CreateIndex(
                name: "IX_IIBarangay_MuncityId",
                table: "Barangay",
                column: "MuncityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIBarangay_ProvinceId",
                table: "Barangay",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_IIFacility_BarangayId",
                table: "Facility",
                column: "BarangayId");

            migrationBuilder.CreateIndex(
                name: "IX_IIFacility_MuncityId",
                table: "Facility",
                column: "MuncityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIFacility_ProvinceId",
                table: "Facility",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_RecieverId",
                table: "Feedback",
                column: "RecieverId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_SenderId",
                table: "Feedback",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_ActionMdNavigationId",
                table: "Incoming",
                column: "ActionMdNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_PatientId",
                table: "Incoming",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_ReferredFrom",
                table: "Incoming",
                column: "ReferredFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_ReferredFromNavigationId",
                table: "Incoming",
                column: "ReferredFromNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Incoming_ReferringMdNavigationId",
                table: "Incoming",
                column: "ReferringMdNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_IIIssue_TrackingId",
                table: "Issue",
                column: "TrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_IILogin_UserId",
                table: "Login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IIMuncity_ProvinceId",
                table: "Muncity",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatients_BarangayId",
                table: "Patient",
                column: "BarangayId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatients_MuncityId",
                table: "Patient",
                column: "MuncityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatients_ProvinceId",
                table: "Patient",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_DepartmentId",
                table: "PatientForm",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_PatientId",
                table: "PatientForm",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_ReferredMd",
                table: "PatientForm",
                column: "ReferredMd");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_ReferredTo",
                table: "PatientForm",
                column: "ReferredTo");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_ReferringFacilityId",
                table: "PatientForm",
                column: "ReferringFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPatientForm_ReferringMd",
                table: "PatientForm",
                column: "ReferringMd");

            migrationBuilder.CreateIndex(
                name: "IX_IIPregnantForm_DepartmentId",
                table: "PregnantForm",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PregnantForm_PatientBabyId",
                table: "PregnantForm",
                column: "PatientBabyId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPregnantForm_PatientWomanId",
                table: "PregnantForm",
                column: "PatientWomanId");

            migrationBuilder.CreateIndex(
                name: "IX_IIPregnantForm_ReferredBy",
                table: "PregnantForm",
                column: "ReferredBy");

            migrationBuilder.CreateIndex(
                name: "IX_IIPregnantForm_ReferredTo",
                table: "PregnantForm",
                column: "ReferredTo");

            migrationBuilder.CreateIndex(
                name: "IX_IIPregnantForm_ReferringFacility",
                table: "PregnantForm",
                column: "ReferringFacility");

            migrationBuilder.CreateIndex(
                name: "IX_IISeen_FacilityId",
                table: "Seen",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_IISeen_TrackingId",
                table: "Seen",
                column: "TrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_IISeen_UserMd",
                table: "Seen",
                column: "UserMd");

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_ActionMd",
                table: "Tracking",
                column: "ActionMd");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_DepartmentId",
                table: "Tracking",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_PatientId",
                table: "Tracking",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_ReferredFrom",
                table: "Tracking",
                column: "ReferredFrom");

            migrationBuilder.CreateIndex(
                name: "IX_IITracking_ReferredTo",
                table: "Tracking",
                column: "ReferredTo");

            migrationBuilder.CreateIndex(
                name: "IX_Tracking_ReferringMd",
                table: "Tracking",
                column: "ReferringMd");

            migrationBuilder.CreateIndex(
                name: "IX_IIUser_BarangayId",
                table: "User",
                column: "BarangayId");

            migrationBuilder.CreateIndex(
                name: "IX_IIUser_DepartmentId",
                table: "User",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IIUser_FacilityId",
                table: "User",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIUser_MuncityId",
                table: "User",
                column: "MuncityId");

            migrationBuilder.CreateIndex(
                name: "IX_IIUser_ProvinceId",
                table: "User",
                column: "ProvinceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activity");

            migrationBuilder.DropTable(
                name: "Baby");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Incoming");

            migrationBuilder.DropTable(
                name: "Issue");

            migrationBuilder.DropTable(
                name: "Login");

            migrationBuilder.DropTable(
                name: "PatientForm");

            migrationBuilder.DropTable(
                name: "PregnantForm");

            migrationBuilder.DropTable(
                name: "Seen");

            migrationBuilder.DropTable(
                name: "Transportation");

            migrationBuilder.DropTable(
                name: "Tracking");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Facility");

            migrationBuilder.DropTable(
                name: "Barangay");

            migrationBuilder.DropTable(
                name: "Muncity");

            migrationBuilder.DropTable(
                name: "Province");
        }
    }
}
