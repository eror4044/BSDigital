<template>
  <div class="orderbook-page">
    <h2>BTC/EUR Order Book Depth</h2>
    <p>Status: {{ status }}</p>
    <p v-if="errorMessage" class="error">Error: {{ errorMessage }}</p>

    <order-book-quote v-model:amountBtc="amountBtc" :quote="quote" />

    <div class="orderbook-layout">
      <order-book-chart :bids="bids" :asks="asks" class="chart" />
      <order-book-snapshots class="snapshots" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useOrderBook } from "../hooks/useOrderBook";
import OrderBookChart from "./OrderBookChart.vue";
import OrderBookQuote from "./OrderBookQuote.vue";
import OrderBookSnapshots from "./OrderBookSnapshots.vue";

const { bids, asks, status, errorMessage, amountBtc, quote, connect } = useOrderBook();

onMounted(connect);
</script>

<style scoped>
.orderbook-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.orderbook-layout {
  display: flex;
  flex-direction: row;
  gap: 1rem;
  height: 70vh;
}

.chart,
.snapshots {
  height: 100%;
  min-height: 0;
}

.chart {
  flex: 2;
  min-width: 0;
}

.snapshots {
  flex: 1;
  min-width: 300px;
  overflow-y: auto;
}
</style>
