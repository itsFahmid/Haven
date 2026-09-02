# Render Deployment Guide for HAVEN

This guide explains how HAVEN's backend and database are deployed and operated on **Render**.

---

## 1. Architecture Overview

HAVEN is an integrated, full-stack **ASP.NET Core (.NET 10)** web platform:
- **Backend**: C# Controllers (`Controllers/`), Entity Framework Core Database Layer (`Data/HavenDbContext.cs`), SignalR WebSocket Hubs (`Hubs/HotlineHub.cs`), and Security/Authentication Services (`Services/AuthService.cs`).
- **Frontend**: Server-rendered Razor views (`Views/`), Tailwind CSS, and vanilla JS (`wwwroot/js/site.js`).
- **Container**: Packaged via root [`Dockerfile`](file:///d:/Haven/Dockerfile).

> When deployed as a **Docker Web Service** on Render, the backend API, real-time SignalR, and user interface run together inside the container.

---

## 2. Deployment Methods on Render

### Method A: 1-Click Blueprint Deployment (Recommended)
Because [`render.yaml`](file:///d:/Haven/render.yaml) is present in the repository root:
1. Log in to your [Render Dashboard](https://dashboard.render.com/).
2. Click **New +** > **Blueprint**.
3. Select your GitHub repository: `itsFahmid/Haven`.
4. Choose the `dev` branch.
5. Render will automatically detect [`render.yaml`](file:///d:/Haven/render.yaml) and configure:
   - **`haven-backend`** (Web Service running Docker)
   - **`haven-database`** (Managed PostgreSQL database, free tier)
   - Automatically link `DATABASE_URL` between them.
6. Click **Apply**.

---

### Method B: Manual Web Service Deployment
If you prefer configuring the Web Service manually:
1. Go to [Render Dashboard](https://dashboard.render.com/) > **New +** > **Web Service**.
2. Connect your repository: `itsFahmid/Haven` (Branch: `dev`).
3. Select **Docker** as the Runtime.
4. Set the following configuration:
   - **Name**: `haven-backend`
   - **Region**: Oregon (or nearest)
   - **Branch**: `dev`
   - **Root Directory**: (Leave blank)
   - **Dockerfile Path**: `./Dockerfile`
   - **Instance Type**: Free
5. Under **Environment Variables**, add:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `PORT` = `8080`
   - *(Optional for PostgreSQL)* `DATABASE_URL` = `<Your PostgreSQL Connection String>`
6. Click **Create Web Service**.

---

## 3. Database Compatibility

The backend automatically detects and connects to your database:
1. **Render PostgreSQL**: If `DATABASE_URL` is set (e.g. `postgres://user:pass@host:port/dbname`), HAVEN automatically parses it and connects using **Npgsql**. All tables and schemas are created automatically on first boot.
2. **SQLite Cloud Fallback**: If no cloud database connection string is provided, HAVEN falls back to local SQLite (`haven.db`) inside the container with automated schema provisioning.
3. **Microsoft SQL Server**: Supported if a standard SQL Server connection string is provided in `ConnectionStrings__DefaultConnection`.

---

## 4. Default Seed Accounts

Upon initial launch and database creation, HAVEN automatically provisions default test accounts:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Chief Admin** | `admin@haven.org` | `Admin123!` |
| **Therapist** | `therapist@haven.org` | `Therapist123!` |
| **User** | `user@haven.org` | `User123!` |
