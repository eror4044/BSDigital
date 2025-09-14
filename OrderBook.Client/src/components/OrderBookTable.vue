<template>
    <div class="orderbook">
        <h3>Order Book</h3>
        <div class="orderbook-table-wrapper">
            <table>
                <thead>
                    <tr class="group-header">
                        <th rowspan="2">Time</th>
                        <th colspan="2" class="bids-block">Bids</th>
                        <th colspan="2" class="asks-block">Asks</th>
                    </tr>
                    <tr>
                        <th class="bids-block">Price (EUR)</th>
                        <th class="bids-block">Amount (BTC)</th>
                        <th class="asks-block">Price (EUR)</th>
                        <th class="asks-block">Amount (BTC)</th>
                    </tr>
                </thead>
                <tbody>
                    <OrderBookRow v-for="i in rowCount" :key="i" :time="time" :bid="bidsSorted[i - 1]"
                        :ask="asksSorted[i - 1]" :maxBid="maxBidAmount" :maxAsk="maxAskAmount" />
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from "vue";
import { useOrderBookSnapshots } from "../hooks/useOrderBookSnapshots";
import OrderBookRow from "./OrderBookRow.vue";

const { snapshots, fetchSnapshots } = useOrderBookSnapshots();

const latest = computed(() =>
    snapshots.value.length > 0 ? snapshots.value[0] : null
);

const asksSorted = computed(() =>
    latest.value ? JSON.parse(latest.value.asks).sort((a: any, b: any) => a.Price - b.Price) : []
);

const bidsSorted = computed(() =>
    latest.value ? JSON.parse(latest.value.bids).sort((a: any, b: any) => b.Price - a.Price) : []
);

const rowCount = computed(() => Math.max(bidsSorted.value.length, asksSorted.value.length));
const time = computed(() => (latest.value ? new Date(latest.value.timestamp).toLocaleTimeString() : ""));

const maxBidAmount = computed(() =>
    Math.max(...bidsSorted.value.map((b: any) => b.Amount), 1)
);
const maxAskAmount = computed(() =>
    Math.max(...asksSorted.value.map((a: any) => a.Amount), 1)
);

let interval: number | undefined;
onMounted(() => {
    fetchSnapshots(20);
    interval = window.setInterval(() => {
        fetchSnapshots(20);
    }, 1000);
});
onUnmounted(() => {
    if (interval) window.clearInterval(interval);
});
</script>

<style scoped>
.orderbook {
    font-family: monospace;
    margin-top: 1rem;
}

.orderbook-table-wrapper {
    max-height: 400px;
    overflow-y: auto;
    border: 1px solid #ddd;
}

table {
    width: 100%;
    border-collapse: collapse;
}

th,
td {
    padding: 4px 6px;
    text-align: right;
    font-size: 0.9rem;
}

thead th {
    background: #f8f8f8;
    color: #333;
    font-weight: bold;
}

thead tr.group-header th {
    text-align: center;
    font-size: 0.95rem;
}

tbody td:nth-child(2),
tbody td:nth-child(3) {
    background: rgba(0, 128, 0, 0.02);
}

tbody td:nth-child(4),
tbody td:nth-child(5) {
    background: rgba(255, 0, 0, 0.02);
}
</style>
