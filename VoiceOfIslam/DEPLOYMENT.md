# VoiceOfIslam Render/Fly.io Deployment Guide

## 1. Prerequisites
- Push your code to GitHub/GitLab (private or public repo)
- Set up your database (Azure SQL, Render PostgreSQL, etc.)
- Get your Azure Blob Storage connection string and set CORS for your domain

## 2. Deploy to Render.com
1. Go to https://dashboard.render.com/new/web-service
2. Connect your repo and select the `VoiceOfIslam` folder as the root directory
3. Set the Dockerfile path to `VoiceOfIslam/Dockerfile`
4. Set the build and start commands to default (Dockerfile handles it)
5. Set environment variables:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection=...` (your DB connection string)
   - `BlobStorage__ConnectionString=...` (your Azure Blob connection string)
6. Expose port 8080 (Render auto-detects from Dockerfile)
7. Deploy!

## 3. Deploy to Fly.io
1. Install Fly CLI: https://fly.io/docs/hands-on/install-flyctl/
2. Run `fly launch` in the `VoiceOfIslam` folder
3. Set the internal port to 8080
4. Set environment variables as above
5. Deploy with `fly deploy`

## 4. Custom Domain & SSL
- Both Render and Fly.io support custom domains and free SSL in their dashboard

## 5. Troubleshooting
- Check logs in Render/Fly.io dashboard
- Make sure your database and blob storage are accessible from the host
- For migrations, use Render/Fly.io shell or CI/CD to run `dotnet ef database update`

---

For more help, see the official docs:
- Render: https://render.com/docs/deploy-dotnet
- Fly.io: https://fly.io/docs/dotnet/getting-started/
- Blazor: https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/
