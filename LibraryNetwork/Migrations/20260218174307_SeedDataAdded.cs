using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryNetwork.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Category", "Description", "ISBN", "Title" },
                values: new object[,]
                {
                    { 1, "Robert C. Martin", "Software", "Writing readable, maintainable code.", "9780132350884", "Clean Code" },
                    { 2, "Andrew Hunt, David Thomas", "Software", "Classic tips for better development.", "9780201616224", "The Pragmatic Programmer" },
                    { 3, "George Orwell", "Fiction", "Dystopian novel.", "9780451524935", "1984" },
                    { 4, "J.R.R. Tolkien", "Fantasy", "Adventure story.", "9780547928227", "The Hobbit" },
                    { 5, "Yuval Noah Harari", "History", "A brief history of humankind.", "9780062316097", "Sapiens" }
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "Address", "City", "Name" },
                values: new object[,]
                {
                    { 1, "Ul. Makedonija 12", "Skopje", "Brakja Miladinovci" },
                    { 2, "Bul. Goce Delcev 5", "Skopje", "Nacionalna Biblioteka" },
                    { 3, "Ul. Ilinden 45", "Prilep", "Borka Taleski" },
                    { 4, "Ul. Partizanska 8", "Stip", "Goce Delcev" },
                    { 5, "Ul. 7-mi Septemvri 22", "Kavadarci", "Strasho Pindzur" }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "MembershipNumber", "phoneNumber" },
                values: new object[,]
                {
                    { 1, null, "Risto", "Kizov", "MBR-0001", null },
                    { 2, null, "Sandra", "Shandarovska", "MBR-0002", null },
                    { 3, null, "Tea", "Domazetovikj", "MBR-0003", null },
                    { 4, null, "Borjan", "Petrevski", "MBR-0004", null }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "Id", "BookId", "InventoryCode", "IsAvailable", "LibraryId" },
                values: new object[,]
                {
                    { 1, 1, "BM-CC-001", true, 1 },
                    { 2, 3, "BM-1984-001", false, 1 },
                    { 3, 2, "NB-PP-001", true, 2 },
                    { 4, 4, "NB-HOB-001", true, 2 },
                    { 5, 5, "BT-SAP-001", true, 3 },
                    { 6, 3, "BT-1984-002", true, 3 },
                    { 7, 1, "GD-CC-002", true, 4 },
                    { 8, 4, "SP-HOB-002", false, 5 }
                });

            migrationBuilder.InsertData(
                table: "Librarians",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "LibraryId" },
                values: new object[,]
                {
                    { 1, null, "Kristina", "Kitrozoska", 1 },
                    { 2, null, "Angela", "Nastovska", 2 },
                    { 3, null, "Monika", "Stoilkovska", 3 },
                    { 4, null, "Ivan", "Perchuklieski", 4 },
                    { 5, null, "Andrej", "Chochov", 5 }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "Id", "BookCopyId", "BorrowDate", "DueDate", "FineAmount", "FinePaidDate", "IsFinePaid", "MemberId", "ReturnDate", "Semester", "Status" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, 1, null, 0, 1 },
                    { 2, 8, new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, 2, null, 0, 1 },
                    { 3, 3, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 0f, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 3, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Librarians",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Librarians",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Librarians",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Librarians",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Librarians",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
