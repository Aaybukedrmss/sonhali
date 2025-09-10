IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [UrunId] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_CartItems_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_ShopCo_UrunId] FOREIGN KEY ([UrunId]) REFERENCES [dbo].[ShopCo]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_CartItems_UserId_UrunId] ON [dbo].[CartItems]([UserId],[UrunId]);
END

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderNumber] NVARCHAR(32) NOT NULL,
        [UserId] INT NOT NULL,
        [ShippingProvider] NVARCHAR(20) NOT NULL,
        [AddressId] INT NULL,
        [Subtotal] DECIMAL(18,2) NOT NULL,
        [ShippingCost] DECIMAL(18,2) NOT NULL,
        [Total] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Orders_UserAddresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[UserAddresses]([Id])
    );
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [dbo].[Orders]([OrderNumber]);
END

IF OBJECT_ID(N'[dbo].[OrderItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItems]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [UrunId] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_ShopCo_UrunId] FOREIGN KEY ([UrunId]) REFERENCES [dbo].[ShopCo]([Id]) ON DELETE NO ACTION
    );
END

-- Ensure Status column exists on Orders
IF COL_LENGTH(N'dbo.Orders', N'Status') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT 'Pending' WITH VALUES;
END


-- Ensure Iyzipay tracking columns exist on Orders
IF COL_LENGTH(N'dbo.Orders', N'PaymentId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentId] NVARCHAR(64) NULL;
END

IF COL_LENGTH(N'dbo.Orders', N'ConversationId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [ConversationId] NVARCHAR(64) NULL;
END

IF COL_LENGTH(N'dbo.Orders', N'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentStatus] NVARCHAR(32) NULL;
END

IF COL_LENGTH(N'dbo.Orders', N'PaymentLastError') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentLastError] NVARCHAR(256) NULL;
END

-- Baseline EF migration history for existing objects to avoid re-creation
IF OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory](
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Helper to insert history row if missing
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250829112054_InitialCreate')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250829112054_InitialCreate', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250901074233_AddLoginLog')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250901074233_AddLoginLog', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250901093727_AddFavorites')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250901093727_AddFavorites', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250901101450_AddSupportRequests')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250901101450_AddSupportRequests', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250901113316_AddUserAddresses')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250901113316_AddUserAddresses', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250901191929_AddCartItems')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250901191929_AddCartItems', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250910073506_AddIyzipayFieldsToOrder')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250910073506_AddIyzipayFieldsToOrder', N'9.0.9');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250910075129_AddPaymentFieldsToOrder')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250910075129_AddPaymentFieldsToOrder', N'9.0.9');


