﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmallRecordBook.Web.Repositories;

#nullable disable

namespace SmallRecordBook.Web.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    partial class SqliteDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("SmallRecordBook.Web.Models.RecordEntry", b =>
                {
                    b.Property<int>("RecordEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("EntryDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("LinkReference")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("ReminderDate")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("ReminderDone")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RecordEntryId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("RecordEntries");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.RecordEntryTag", b =>
                {
                    b.Property<int>("RecordEntryTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("RecordEntryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("RecordEntryTagId");

                    b.HasIndex("RecordEntryId");

                    b.ToTable("RecordEntryTags");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserAccountCredential", b =>
                {
                    b.Property<int>("UserAccountCredentialId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("CredentialId")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("PublicKey")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<uint>("SignatureCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("UserHandle")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("UserAccountCredentialId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserAccountCredentials");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserAccountSetting", b =>
                {
                    b.Property<int>("UserAccountSettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SettingValue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserAccountSettingId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserAccountSettings");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserFeed", b =>
                {
                    b.Property<int>("UserFeedId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemHash")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserFeedIdentifier")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserFeedId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserFeeds");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.RecordEntry", b =>
                {
                    b.HasOne("SmallRecordBook.Web.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.RecordEntryTag", b =>
                {
                    b.HasOne("SmallRecordBook.Web.Models.RecordEntry", "RecordEntry")
                        .WithMany("RecordEntryTags")
                        .HasForeignKey("RecordEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RecordEntry");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserAccountCredential", b =>
                {
                    b.HasOne("SmallRecordBook.Web.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserAccountSetting", b =>
                {
                    b.HasOne("SmallRecordBook.Web.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.UserFeed", b =>
                {
                    b.HasOne("SmallRecordBook.Web.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallRecordBook.Web.Models.RecordEntry", b =>
                {
                    b.Navigation("RecordEntryTags");
                });
#pragma warning restore 612, 618
        }
    }
}
