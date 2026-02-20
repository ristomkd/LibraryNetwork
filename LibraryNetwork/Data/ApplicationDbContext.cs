using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LibraryNetwork.Models; 

namespace LibraryNetwork.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Member> Members { get; set; }
        public DbSet<Librarian> Librarians { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Library (1) -> (Many) BookCopy
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Library)
                .WithMany(l => l.BookCopies)
                .HasForeignKey(bc => bc.LibraryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Book (1) -> (Many) BookCopy
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Member (1) -> (Many) Loan
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookCopy (1) -> (Many) Loan
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.BookCopy)
                .WithMany(bc => bc.Loans)
                .HasForeignKey(l => l.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Library (1) -> (Many) Librarian
            modelBuilder.Entity<Librarian>()
                .HasOne(lb => lb.Library)
                .WithMany(l => l.Librarians)
                .HasForeignKey(lb => lb.LibraryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints (добра пракса)

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasIndex(m => m.MembershipNumber)
                .IsUnique();

            modelBuilder.Entity<BookCopy>()
                .HasIndex(bc => bc.InventoryCode)
                .IsUnique();

            // SEED DATA

            modelBuilder.Entity<Library>().HasData(
                new Library { Id = 1, Name = "Brakja Miladinovci", City = "Skopje", Address = "Ul. Makedonija 12" },
                new Library { Id = 2, Name = "Nacionalna Biblioteka", City = "Skopje", Address = "Bul. Goce Delcev 5" },
                new Library { Id = 3, Name = "Borka Taleski", City = "Prilep", Address = "Ul. Ilinden 45" },
                new Library { Id = 4, Name = "Goce Delcev", City = "Stip", Address = "Ul. Partizanska 8" },
                new Library { Id = 5, Name = "Strasho Pindzur", City = "Kavadarci", Address = "Ul. 7-mi Septemvri 22" }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Clean Code", ISBN = "9780132350884", Author = "Robert C. Martin", Category = "Software", Description = "Writing readable, maintainable code." },
                new Book { Id = 2, Title = "The Pragmatic Programmer", ISBN = "9780201616224", Author = "Andrew Hunt, David Thomas", Category = "Software", Description = "Classic tips for better development." },
                new Book { Id = 3, Title = "1984", ISBN = "9780451524935", Author = "George Orwell", Category = "Fiction", Description = "Dystopian novel." },
                new Book { Id = 4, Title = "The Hobbit", ISBN = "9780547928227", Author = "J.R.R. Tolkien", Category = "Fantasy", Description = "Adventure story." },
                new Book { Id = 5, Title = "Sapiens", ISBN = "9780062316097", Author = "Yuval Noah Harari", Category = "History", Description = "A brief history of humankind." }
            );

            modelBuilder.Entity<Member>().HasData(
                new Member { Id = 1, FirstName="Risto", LastName="Kizov", MembershipNumber = "MBR-0001" },
                new Member { Id = 2, FirstName = "Sandra", LastName = "Shandarovska", MembershipNumber = "MBR-0002" },
                new Member { Id = 3, FirstName = "Tea", LastName = "Domazetovikj", MembershipNumber = "MBR-0003" },
                new Member { Id = 4, FirstName = "Borjan", LastName = "Petrevski", MembershipNumber = "MBR-0004" }
            );

            modelBuilder.Entity<Librarian>().HasData(
                new Librarian { Id = 1, FirstName = "Kristina", LastName = "Kitrozoska", LibraryId = 1 },
                new Librarian { Id = 2, FirstName = "Angela", LastName = "Nastovska", LibraryId = 2 },
                new Librarian { Id = 3, FirstName = "Monika", LastName = "Stoilkovska", LibraryId = 3 },
                new Librarian { Id = 4, FirstName = "Ivan", LastName = "Perchuklieski", LibraryId = 4 },
                new Librarian { Id = 5, FirstName = "Andrej", LastName = "Chochov", LibraryId = 5 }
            );

            modelBuilder.Entity<BookCopy>().HasData(
                // Library 1
                new BookCopy { Id = 1, InventoryCode = "BM-CC-001", IsAvailable = true, BookId = 1, LibraryId = 1 },
                new BookCopy { Id = 2, InventoryCode = "BM-1984-001", IsAvailable = false, BookId = 3, LibraryId = 1 },

                // Library 2
                new BookCopy { Id = 3, InventoryCode = "NB-PP-001", IsAvailable = true, BookId = 2, LibraryId = 2 },
                new BookCopy { Id = 4, InventoryCode = "NB-HOB-001", IsAvailable = true, BookId = 4, LibraryId = 2 },

                // Library 3
                new BookCopy { Id = 5, InventoryCode = "BT-SAP-001", IsAvailable = true, BookId = 5, LibraryId = 3 },
                new BookCopy { Id = 6, InventoryCode = "BT-1984-002", IsAvailable = true, BookId = 3, LibraryId = 3 },

                // Library 4
                new BookCopy { Id = 7, InventoryCode = "GD-CC-002", IsAvailable = true, BookId = 1, LibraryId = 4 },

                // Library 5
                new BookCopy { Id = 8, InventoryCode = "SP-HOB-002", IsAvailable = false, BookId = 4, LibraryId = 5 }
            );

            modelBuilder.Entity<Loan>().HasData(
               // Active (не е overdue ако DueDate е во иднина)
               new Loan
               {
                   Id = 1,
                   MemberId = 1,
                   BookCopyId = 2,
                   BorrowDate = new DateTime(2026, 2, 10),
                   DueDate = new DateTime(2026, 2, 25),
                   ReturnDate = null,
                   FineAmount = null,
                   IsFinePaid = false,
                   FinePaidDate = null,
                   Semester = Enums.SemesterType.Winter,
                   Status = Enums.LoanStatus.Active
               },

               // Overdue пример (DueDate помината, ReturnDate null) -> казната ќе ти ја пресмета твојата функција во controller
               new Loan
               {
                   Id = 2,
                   MemberId = 2,
                   BookCopyId = 8,
                   BorrowDate = new DateTime(2026, 1, 10),
                   DueDate = new DateTime(2026, 1, 20),
                   ReturnDate = null,
                   FineAmount = null,
                   IsFinePaid = false,
                   FinePaidDate = null,
                   Semester = Enums.SemesterType.Winter,
                   Status = Enums.LoanStatus.Active
               },

               // Returned пример
               new Loan
               {
                   Id = 3,
                   MemberId = 3,
                   BookCopyId = 3,
                   BorrowDate = new DateTime(2026, 2, 1),
                   DueDate = new DateTime(2026, 2, 10),
                   ReturnDate = new DateTime(2026, 2, 8),
                   FineAmount = 0f,
                   IsFinePaid = true,
                   FinePaidDate = new DateTime(2026, 2, 8),
                   Semester = Enums.SemesterType.Winter,
                   Status = Enums.LoanStatus.Returned
               }
           );

        }
    }
}
