using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdPhotoManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [EmployeeId] = '02486033')
BEGIN
    INSERT INTO [Users]
        ([Id], [AdObjectId], [DisplayName], [EmployeeId], [Title], [Organization], [Department], [Email], [HasPhoto], [LastSyncedAt], [PhotoUpdatedAt], [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        ('f4c2f5d0-7d8d-4cd8-a0bf-e450bf4c7024', 'f4c2f5d0-7d8d-4cd8-a0bf-e450bf4c7024', N'ŞENOL DİNÇ', '02486033', N'Sistem Uzmanı', N'KoçSistem', N'Bilgi Teknolojileri', 'senol.dinc@kocsistem.com.tr', 0, GETUTCDATE(), NULL, GETUTCDATE(), GETUTCDATE(), 0);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [Users]
WHERE [EmployeeId] = '02486033'
  AND [Email] = 'senol.dinc@kocsistem.com.tr';
");
        }
    }
}
