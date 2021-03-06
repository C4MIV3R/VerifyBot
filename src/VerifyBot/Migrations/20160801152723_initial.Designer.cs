﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using VerifyBot.Models;

namespace VerifyBot.Migrations
{
    [DbContext(typeof(VerifyDatabase))]
    [Migration("20160801152723_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("VerifyBot.Models.User", b =>
                {
                    b.Property<string>("AccountID");

                    b.Property<string>("APIKey");

                    b.Property<ulong>("DiscordID");

                    b.HasKey("AccountID");

                    b.ToTable("Users");
                });
        }
    }
}
