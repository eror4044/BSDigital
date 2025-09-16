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
- 🗄️ Audit log of snapshots in PostgreSQL (with cleanup)

### Client
- 📊 Interactive **depth chart** (bids/asks with cumulative volumes)
- 🔄 Real-time updates via SignalR
- 💵 Quotes calculator for BTC amount
- 📜 Snapshots history list (parsed on client)

---

## 🎥 Demo

<p align="center">
  <img src="assets/bsd_ui.gif" alt="BSD UI Demo" width="800"/>
</p>

---

## 🚀 Quick Start (Docker Compose)

### 1. Clone repository
```bash
git clone https://github.com/your-org/orderbook-demo.git
```

### 2. Build images
```bash
docker compose build
```

### 3. Start database
```bash
docker compose up -d db
```

### 4. Apply migrations
```bash
docker compose run --rm migrate
```

### 5. Start API and client
```bash
docker compose up -d api client
```

### 6. Access services
* **Frontend (Vue)** → [http://localhost:5173](http://localhost:5173)  
* **API** → [http://localhost:5000](http://localhost:5000)  
* **SignalR Hub** → [http://localhost:5000/hubs/orderbook](http://localhost:5000/hubs/orderbook)  
* **PostgreSQL** → `localhost:5434`
  ```
  user:     postgres
  password: postgres
  database: orderbookdb
  ```

### 7. Stop everything
```bash
docker compose down
```

---

## 📂 Project Structure

```
.
├── OrderBook.Application/   # .NET 8 backend (SignalR, EF Core, PostgreSQL)
├── OrderBook.Client/        # Vue 3 frontend (Vite + ECharts)
├── docker-compose.yml       # Orchestration
└── README.md                # this file
```

---

## 🔧 Development Notes

* Ports & credentials configurable via `.env`
* Default CORS allows `http://localhost:5173`
* Snapshots stored in `OrderBookSnapshots` table
* EF Core migrations applied via `docker compose run --rm migrate`
* Client auto-reconnects to SignalR hub
* Retention service cleans up old snapshots (>7 days)

---

## 🛠️ Tech Stack

* **Backend** → .NET 8, SignalR, EF Core, PostgreSQL  
* **Frontend** → Vue 3, Vite, vue-echarts  
* **Infra** → Docker, Docker Compose  
