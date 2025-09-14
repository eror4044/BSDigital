<script setup lang="ts">
import { onMounted } from "vue";
import { useOrderBook } from "../hooks/useOrderBook";
import OrderBookChart from "./OrderBookChart.vue";
import OrderBookQuote from "./OrderBookQuote.vue";
import OrderBookSnapshots from "./OrderBookSnapshots.vue";

const { bids, asks, status, errorMessage, amountBtc, quote, connect } = useOrderBook();

onMounted(connect);
</script>

<template>
    <div>
        <h2>BTC/EUR Order Book Depth</h2>
        <p>Status: {{ status }}</p>
        <p v-if="errorMessage" class="error">Error: {{ errorMessage }}</p>

        <order-book-quote v-model:amountBtc="amountBtc" :quote="quote" />
        <order-book-chart :bids="bids" :asks="asks" />
        <order-book-snapshots />
    </div>
</template>
