-- Rode UMA vez em um banco "myfinance" cujas tabelas PlanoConta, Transacao e
-- TransacaoRecorrente foram criadas manualmente, antes das migrations do EF Core.
-- Registra a migration inicial como ja aplicada, para que Database.Migrate()
-- nao tente recriar as tabelas. Os dados existentes nao sao alterados.
-- Bancos novos (ex.: Azure) NAO precisam deste script.

USE myfinance;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261030124433_InicialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261030124433_InicialCreate', N'9.0.1');
END;
GO
