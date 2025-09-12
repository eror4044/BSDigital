<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import * as signalR from "@microsoft/signalr";
import VChart from "vue-echarts";

const bids = ref<[number, number][]>([]);
const asks = ref<[number, number][]>([]);
const chartOptions = ref({});
const status = ref("disconnected");
const errorMessage = ref<string | null>(null);
const amountBtc = ref<number>(0.1);
const quote = ref<{ total: number; avg: number; filled: number; ok: boolean; ts?: string } | null>(null);
let quoteTimer: number | undefined;

async function fetchQuote() {
  try {
    const resp = await fetch(`http://localhost:5000/api/quotes?amount=${amountBtc.value}&type=buy`, {
      credentials: "omit"
    });
    const data = await resp.json();
    quote.value = {
      total: data.totalCostEur,
      avg: data.averagePriceEur,
      filled: data.filledAmountBtc,
      ok: data.sufficientLiquidity,
      ts: data.snapshotTimestampUtc
    };
  } catch (e) {
    console.error("quote error", e);
  }
}

watch(amountBtc, (newVal, oldVal) => {
  if (newVal === oldVal) return;
  if (quoteTimer) window.clearTimeout(quoteTimer);
  quoteTimer = window.setTimeout(fetchQuote, 400);
});

function buildChart() {
  if (!bids.value.length || !asks.value.length) return;

  const allPrices = [...bids.value.map(b => b[0]), ...asks.value.map(a => a[0])];
  const minPrice = Math.min(...allPrices);
  const maxPrice = Math.max(...allPrices);

  chartOptions.value = {
    tooltip: { trigger: "axis" },
    grid: { left: 60, right: 30, top: 40, bottom: 50 },
    xAxis: { type: "value", name: "Price (EUR)", nameLocation: "middle", nameGap: 35, scale: true, min: minPrice * 0.995, max: maxPrice * 1.005 },
    yAxis: { type: "value", name: "Cumulative BTC", nameLocation: "middle", nameGap: 45, scale: true },
    series: [
      { name: "Bids", type: "line", step: "end", showSymbol: false, areaStyle: {}, data: bids.value },
      { name: "Asks", type: "line", step: "end", showSymbol: false, areaStyle: {}, data: asks.value }
    ]
  };
}

onMounted(async () => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/orderbook", { transport: signalR.HttpTransportType.WebSockets })
    .withAutomaticReconnect()
    .build();
  connection.on("orderbook:update", (data: any) => {
    if (!data?.bids || !data?.asks) return;

    const parsedBids: [number, number][] = data.bids
      .slice(0, 50)
      .map(([p, v]: [string, string]) => [parseFloat(p), parseFloat(v)])
      .sort((a: number[], b: number[]) => b[0] - a[0]);

    const parsedAsks: [number, number][] = data.asks
      .slice(0, 50)
      .map(([p, v]: [string, string]) => [parseFloat(p), parseFloat(v)])
      .sort((a: number[], b: number[]) => a[0] - b[0]);

    let cum = 0;
    bids.value = parsedBids.map(([p, v]) => { cum += v; return [p, cum]; });
    cum = 0;
    asks.value = parsedAsks.map(([p, v]) => { cum += v; return [p, cum]; });

    buildChart();
  });

  try {
    await connection.start();
    status.value = "connected";
    fetchQuote();
  } catch (err) {
    console.error("SignalR connection error:", err);
    status.value = "failed";
    errorMessage.value = (err as Error).message;
  }
});
</script>

<template>
  <div>
    <h2>BTC/EUR Order Book Depth</h2>
    <p>Status: {{ status }}</p>
    <p v-if="errorMessage" style="color:red">Error: {{ errorMessage }}</p>
    <div style="margin: 12px 0;">
      <label>Amount (BTC): </label>
      <input type="number" v-model.number="amountBtc" step="0.0001" min="0" />
      <span v-if="quote" style="margin-left:12px">
        <strong>Quote:</strong>
        <template v-if="quote.ok">
          {{ quote.filled?.toFixed(4) ?? 0 }} BTC ≈ {{ quote.total?.toFixed(2) ?? 0 }} EUR
          (avg {{ quote.avg?.toFixed(2) ?? 0 }} EUR)
        </template>
        <template v-else>
          Not sufficient liquidity (filled {{ quote.filled?.toFixed(4) ?? 0 }} BTC)
        </template>
      </span>
    </div>

    <v-chart class="chart" :option="chartOptions" autoresize />
  </div>
</template>

<style scoped>
.chart {
  width: 100%;
  height: 500px;
}
</style>