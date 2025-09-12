````md
# 💹 BTC/EUR Order Book — Fullstack Demo

This project demonstrates a **real-time BTC/EUR order book** with:  

- ⚡ **Backend (API)** → .NET 8 + SignalR + PostgreSQL  
- 🎨 **Frontend (Client)** → Vue 3 + Vite + ECharts  
- 🐳 **Deployment** → Docker & Docker Compose  

---

## ✨ Features

### API
- 📡 Real-time order book updates via **SignalR Hub**
- 💰 Quotes calculation endpoint: `/api/quotes`
- 🗄️ Audit log of snapshots in PostgreSQL

### Client
- 📊 Interactive **depth chart** (bids/asks)
- 🔄 Real-time updates via SignalR
- 💵 Quotes calculator for BTC amount

---

## 🚀 Quick Start (Docker Compose)

### 1. Clone repository
```bash
git clone https://github.com/your-org/orderbook-demo.git
cd orderbook-demo
````

### 2. Build & start all services

```bash
docker compose up --build -d
```

### 3. Access services

* **Frontend (Vue)** → [http://localhost:5173](http://localhost:5173)
* **API** → [http://localhost:5000](http://localhost:5000)
* **SignalR Hub** → [http://localhost:5000/hubs/orderbook](http://localhost:5000/hubs/orderbook)
* **PostgreSQL** → `localhost:5434`

  * user: `postgres`
  * password: `postgres`
  * database: `orderbookdb`

### 4. Apply DB migrations

Run inside API container:

```bash
docker compose exec api dotnet ef database update
```

### 5. Stop everything

```bash
docker compose down
```

---

## 📂 Project Structure

```
.
├── api/        # .NET 8 backend (SignalR, EF Core, PostgreSQL)
├── client/     # Vue 3 frontend (Vite + ECharts)
├── docker-compose.yml
└── README.md   # this file
```

---

## 🔧 Development Notes

* Ports & credentials are configurable via `.env`
* Default CORS allows `http://localhost:5173`
* Snapshots table: `OrderBookSnapshots`
* Client auto-reconnects to SignalR hub

---

## 🛠️ Tech Stack

* **Backend** → .NET 8, SignalR, EF Core, PostgreSQL
* **Frontend** → Vue 3, Vite, vue-echarts
* **Infra** → Docker, Docker Compose
