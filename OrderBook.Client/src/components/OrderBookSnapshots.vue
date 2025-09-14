<template>
    <div class="orderbook">
        <div class="tabs">
            <button :class="{ active: activeTab === 'bids' }" @click="activeTab = 'bids'">Bids</button>
            <button :class="{ active: activeTab === 'asks' }" @click="activeTab = 'asks'">Asks</button>
        </div>

        <div class="orderbook-table-wrapper">
            <table v-if="activeTab === 'bids'">
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>Price (EUR)</th>
                        <th>Amount (BTC)</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(b, i) in bidsSorted" :key="i">
                        <td class="time">{{ new Date(latest?.timestamp).toLocaleTimeString() }}</td>
                        <td class="bid">{{ b.Price.toLocaleString() }}</td>
                        <td class="bid amount">
                            <div class="bar bid-bar" :style="{ width: getBarWidth(b.Amount, maxBidAmount) }"></div>
                            {{ b.Amount.toFixed(6) }}
                        </td>
                    </tr>
                </tbody>
            </table>

            <table v-else>
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>Price (EUR)</th>
                        <th>Amount (BTC)</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(a, i) in asksSorted" :key="i">
                        <td class="time">{{ new Date(latest?.timestamp).toLocaleTimeString() }}</td>
                        <td class="ask">{{ a.Price.toLocaleString() }}</td>
                        <td class="ask amount">
                            <div class="bar ask-bar" :style="{ width: getBarWidth(a.Amount, maxAskAmount) }"></div>
                            {{ a.Amount.toFixed(6) }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useOrderBookSnapshots } from "../hooks/useOrderBookSnapshots";

const { snapshots, fetchSnapshots } = useOrderBookSnapshots();
const activeTab = ref<"bids" | "asks">("bids");

const latest = computed(() =>
    snapshots.value.length > 0 ? snapshots.value[0] : null
);

const bidsSorted = computed(() =>
    latest.value ? JSON.parse(latest.value.bids).sort((a: { Price: number; }, b: { Price: number; }) => b.Price - a.Price) : []
);

const asksSorted = computed(() =>
    latest.value ? JSON.parse(latest.value.asks).sort((a: { Price: number; }, b: { Price: number; }) => a.Price - b.Price) : []
);

const maxBidAmount = computed(() => Math.max(...bidsSorted.value.map((b: { Amount: any; }) => b.Amount), 1));
const maxAskAmount = computed(() => Math.max(...asksSorted.value.map((a: { Amount: any; }) => a.Amount), 1));

function getBarWidth(amount: number, max: number) {
    if (!amount || !max) return "0%";
    return `${(amount / max) * 100}%`;
}

let interval: number | undefined;
onMounted(() => {
    fetchSnapshots(20);
    interval = window.setInterval(() => fetchSnapshots(20), 1000);
});
onUnmounted(() => interval && clearInterval(interval));
</script>

<style scoped>
.orderbook {
    font-family: monospace;
    display: flex;
    flex-direction: column;
    height: 100%;
}

.tabs {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
}

.tabs button {
    flex: 1;
    padding: 6px 10px;
    border: 1px solid #ccc;
    background: #f5f5f5;
    cursor: pointer;
    font-family: monospace;
}

.tabs button.active {
    background: #ddd;
    font-weight: bold;
}

.orderbook-table-wrapper {
    flex: 1;
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
    position: relative;
    font-size: 0.9rem;
}

thead th {
    background: #f8f8f8;
    color: #333;
    font-weight: bold;
}

.time {
    width: 50px;
    text-align: left;
    color: #555;
    font-size: 0.8rem;
}

.bid {
    color: #0a0;
}

.ask {
    color: #c00;
}

.amount {
    position: relative;
}

.bar {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    opacity: 0.25;
    z-index: -1;
}

.bid-bar {
    background-color: #0a0;
}

.ask-bar {
    background-color: #c00;
}
</style>
