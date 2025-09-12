<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import * as signalR from "@microsoft/signalr";
import VChart from "vue-echarts";
import type { EChartsType } from "echarts/core";

const QUOTE_DEBOUNCE_MS = 400;
const LEVELS_LIMIT = 50;

const bids = ref<[number, number][]>([]);
const asks = ref<[number, number][]>([]);
const status = ref<"disconnected" | "connected" | "failed">("disconnected");
const errorMessage = ref<string | null>(null);
const amountBtc = ref<number>(0.1);
const quote = ref<{
  total: number;
  avg: number;
  filled: number;
  ok: boolean;
  ts?: string;
} | null>(null);

let quoteTimer: number | undefined;

// ссылка на компонент vue-echarts
const chartRef = ref<InstanceType<typeof VChart> | null>(null);
let chart: EChartsType | null = null;

function fmtEur(v?: number) {
  return v !== undefined ? `${v.toFixed(2)} EUR` : "0.00 EUR";
}
function fmtBtc(v?: number) {
  return v !== undefined ? `${v.toFixed(4)} BTC` : "0.0000 BTC";
}

async function fetchQuote() {
  try {
    const resp = await fetch(
      `http://localhost:5000/api/quotes?amount=${amountBtc.value}&type=buy`,
      { credentials: "omit" }
    );
    const data = await resp.json();
    quote.value = {
      total: data.totalCost,
      avg: data.averagePrice,
      filled: data.filled,
      ok: data.sufficient,
      ts: data.timestampUtc
    };
  } catch (e) {
    console.error("quote error", e);
  }
}

watch(amountBtc, (newVal, oldVal) => {
  if (newVal === oldVal) return;
  if (quoteTimer) window.clearTimeout(quoteTimer);
  quoteTimer = window.setTimeout(fetchQuote, QUOTE_DEBOUNCE_MS);
});

function updateChart() {
  if (!chart || !bids.value.length || !asks.value.length) return;

  chart.setOption(
    {
      tooltip: { trigger: "axis" },
      grid: { left: 60, right: 30, top: 40, bottom: 50 },
      xAxis: {
        type: "value",
        name: "Price (EUR)",
        nameLocation: "middle",
        nameGap: 35,
        scale: true,
        axisLine: { lineStyle: { color: "#aaa" } },
        axisLabel: { color: "#555" }
      },
      yAxis: {
        type: "value",
        name: "Cumulative BTC",
        nameLocation: "middle",
        nameGap: 45,
        scale: true,
        axisLine: { lineStyle: { color: "#aaa" } },
        axisLabel: { color: "#555" }
      },
      series: [
        {
          name: "Bids",
          type: "line",
          step: "end",
          showSymbol: false,
          lineStyle: { color: "#d62728", width: 2 },
          areaStyle: { color: "rgba(214,39,40,0.4)" },
          data: bids.value,
          animationDurationUpdate: 300,
          animationEasingUpdate: "linear"
        },
        {
          name: "Asks",
          type: "line",
          step: "end",
          showSymbol: false,
          lineStyle: { color: "#2ca02c", width: 2 },
          areaStyle: { color: "rgba(44,160,44,0.4)" },
          data: asks.value,
          animationDurationUpdate: 300,
          animationEasingUpdate: "linear"
        }
      ]
    },
    { notMerge: false, lazyUpdate: true }
  );
}

onMounted(async () => {
  if (chartRef.value?.chart) {
    chart = chartRef.value.chart;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/orderbook", {
      transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build();

  connection.on("orderbook:update", (data: { bids: [string, string][], asks: [string, string][] }) => {
    if (!data?.bids || !data?.asks) return;

    const parsedBids = data.bids
      .slice(0, LEVELS_LIMIT)
      .map(([p, v]): [number, number] => [parseFloat(p), parseFloat(v)])
      .sort((a, b) => b[0] - a[0]);

    const parsedAsks = data.asks
      .slice(0, LEVELS_LIMIT)
      .map(([p, v]): [number, number] => [parseFloat(p), parseFloat(v)])
      .sort((a, b) => a[0] - b[0]);

    // cumulative volume
    let cum = 0;
    bids.value = parsedBids.map(([p, v]) => {
      cum += v;
      return [p, cum];
    });

    cum = 0;
    asks.value = parsedAsks.map(([p, v]) => {
      cum += v;
      return [p, cum];
    });

    updateChart();
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
    <p v-if="errorMessage" class="error">Error: {{ errorMessage }}</p>

    <div class="quote-block">
      <label>Amount (BTC): </label>
      <input type="number" v-model.number="amountBtc" step="0.0001" min="0" />

      <span v-if="quote" class="quote">
        <strong>Quote:</strong>
        <template v-if="quote.ok">
          {{ fmtBtc(quote.filled) }} ≈ {{ fmtEur(quote.total) }}
          (avg {{ fmtEur(quote.avg) }})
        </template>
        <template v-else>
          Not sufficient liquidity (filled {{ fmtBtc(quote.filled) }})
        </template>
        <span v-if="quote.ts" class="timestamp">
          [{{ new Date(quote.ts).toLocaleTimeString() }}]
        </span>
      </span>
    </div>

    <v-chart ref="chartRef" class="chart" :option="{}" autoresize />
  </div>
</template>

<style scoped>
.chart {
  width: 100%;
  height: 500px;
}

.error {
  color: red;
}

.quote-block {
  margin: 12px 0;
}

.quote {
  margin-left: 12px;
}

.timestamp {
  margin-left: 8px;
  color: #666;
  font-size: 0.85em;
}
</style>
