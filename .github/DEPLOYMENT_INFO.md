# TailorBlend Frontend - Fly.io Deployment

## ✅ Deployment Configuration

Your Blazor Server frontend is configured for automatic deployment to Fly.io.

### 🌐 URLs

- **Frontend UI**: https://tailorblend-frontend-ui.fly.dev
- **Backend API**: https://tailorblend-backend-api.fly.dev
- **Health Check**: https://tailorblend-frontend-ui.fly.dev/health

### 💰 Cost Optimization

| Setting | Value | Monthly Cost |
|---------|-------|--------------|
| Memory | 256MB + 512MB swap | $2.02 (~R37 ZAR) |
| Always-On | Yes | Included |
| Region | Johannesburg (jnb) | - |
| **Total** | - | **$2.02/month** |

### 🔧 Configuration Summary

```toml
App Name: tailorblend-frontend-ui
Region: Johannesburg, South Africa (jnb)
Memory: 256MB RAM + 512MB swap
Always-On: auto_stop_machines = "off"
Auto-Deploy: Yes (on push to main)
Backend: https://tailorblend-backend-api.fly.dev
```

## 🚀 How It Works

1. **Push to main** → GitHub Actions triggered
2. **Build** → Docker builds .NET 8 Blazor Server app
3. **Deploy** → Deployed to Fly.io (Johannesburg)
4. **Health Check** → Verifies `/health` endpoint
5. **Live** → Frontend accessible at https://tailorblend-frontend-ui.fly.dev

## 📊 Monitoring

**View logs:**
```bash
flyctl logs -a tailorblend-frontend-ui
```

**Check status:**
```bash
flyctl status -a tailorblend-frontend-ui
```

**Watch deployment:**
- GitHub Actions: https://github.com/Adriaan11/tailorblend-frontend-ui/actions
- Or: `gh run watch -R Adriaan11/tailorblend-frontend-ui`

## 🔐 Secrets Configured

- ✅ `FLY_API_TOKEN` - GitHub secret for deployment
- ✅ `PythonApi__BaseUrl` - Backend API URL (https://tailorblend-backend-api.fly.dev)

## 🛠️ Troubleshooting

### Out of Memory
If you experience OOM errors:
```bash
# Edit fly.toml, change line 40:
memory = "512mb"  # was 256mb

# Push changes (auto-deploys)
git add fly.toml
git commit -m "Increase memory to 512MB"
git push origin main
```

### Backend Connection Issues
Verify backend is running:
```bash
curl https://tailorblend-backend-api.fly.dev/api/health
```

### Update Backend URL
```bash
flyctl secrets set PythonApi__BaseUrl=https://new-backend-url.com -a tailorblend-frontend-ui
```

## 📋 Complete System Architecture

```
User Browser
    ↓
https://tailorblend-frontend-ui.fly.dev (Blazor Server - 256MB)
    ↓ HTTP calls
https://tailorblend-backend-api.fly.dev (Python FastAPI - 256MB)
    ↓ API calls
OpenAI API (GPT-4 + Vector Store)
```

**Total Cost: $4.04/month** ($2.02 frontend + $2.02 backend)

---

**🎉 Your TailorBlend platform is now fully automated with CI/CD!**
