using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuizletCloneAPI.Models;

namespace BackendQuizletclone.Data;

public partial class QuizletCloneDbContext : DbContext
{
    public QuizletCloneDbContext()
    {
    }

    public QuizletCloneDbContext(DbContextOptions<QuizletCloneDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<StudyProgress> StudyProgresses { get; set; }

    public virtual DbSet<StudySet> StudySets { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-Q4MLP930\\SQLEXPRESS;Initial Catalog=QuizletCloneDB;Integrated Security=True;Trust Server Certificate=True");
*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Flashcar__3214EC071539970A");

            entity.Property(e => e.IsStarred).HasDefaultValue(false);

            entity.HasOne(d => d.StudySet).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.StudySetId)
                .HasConstraintName("FK_Flashcards_StudySets");
        });

        modelBuilder.Entity<StudyProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudyPro__3214EC073EE2B5A4");

            entity.Property(e => e.EaseFactor).HasDefaultValue(2.5);
            entity.Property(e => e.Interval).HasDefaultValue(0);
            entity.Property(e => e.LastReviewedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NextReviewDate).HasColumnType("datetime");
            entity.Property(e => e.Repetitions).HasDefaultValue(0);

            entity.HasOne(d => d.Flashcard).WithMany(p => p.StudyProgresses)
                .HasForeignKey(d => d.FlashcardId)
                .HasConstraintName("FK_Progress_Flashcards");

            entity.HasOne(d => d.User).WithMany(p => p.StudyProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Progress_Users");
        });

        modelBuilder.Entity<StudySet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudySet__3214EC07C3A4FD02");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.StudySets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_StudySets_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC078F2F0EEB");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E40DBCD2A0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534D3BD4728").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
