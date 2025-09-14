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
                    <tr v-for="i in rowCount" :key="i">
                        <td class="time">
                            {{ new Date(latest?.timestamp).toLocaleTimeString() }}
                        </td>
                        <!-- Bid -->
                        <td class="bid">
                            {{ bidsSorted[i - 1]?.Price?.toLocaleString() ?? "" }}
                        </td>
                        <td class="bid amount">
                            <div class="bar bid-bar"
                                :style="{ width: getBarWidth(bidsSorted[i - 1]?.Amount, maxBidAmount) }"></div>
                            {{ bidsSorted[i - 1]?.Amount?.toFixed(6) ?? "" }}
                        </td>

                        <!-- Ask -->
                        <td class="ask">
                            {{ asksSorted[i - 1]?.Price?.toLocaleString() ?? "" }}
                        </td>
                        <td class="ask amount">
                            <div class="bar ask-bar"
                                :style="{ width: getBarWidth(asksSorted[i - 1]?.Amount, maxAskAmount) }"></div>
                            {{ asksSorted[i - 1]?.Amount?.toFixed(6) ?? "" }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from "vue";
import { useOrderBookSnapshots } from "../hooks/useOrderBookSnapshots";

const { snapshots, fetchSnapshots } = useOrderBookSnapshots();

const latest = computed(() =>
    snapshots.value.length > 0 ? snapshots.value[0] : null
);

const asksSorted = computed(() => {
    if (!latest.value) return [];
    return JSON.parse(latest.value.asks).sort((a: { Price: number; }, b: { Price: number; }) => a.Price - b.Price);
});

const bidsSorted = computed(() => {
    if (!latest.value) return [];
    return JSON.parse(latest.value.bids).sort((a: { Price: number; }, b: { Price: number; }) => b.Price - a.Price);
});

const rowCount = computed(() =>
    Math.max(bidsSorted.value.length, asksSorted.value.length)
);

const maxBidAmount = computed(() =>
    Math.max(...bidsSorted.value.map((b: any) => b.Amount), 1)
);
const maxAskAmount = computed(() =>
    Math.max(...asksSorted.value.map((a: any) => a.Amount), 1)
);

function getBarWidth(amount: number, max: number) {
    if (!amount || !max) return "0%";
    return `${(amount / max) * 100}%`;
}
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
    position: relative;
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

.time {
    text-align: left;
    color: #555;
    font-size: 0.8rem;
}

/* Фон для колонок в теле таблицы */
tbody td:nth-child(2),
tbody td:nth-child(3) {
    background: rgba(0, 128, 0, 0.02);
    /* bids */
}

tbody td:nth-child(4),
tbody td:nth-child(5) {
    background: rgba(255, 0, 0, 0.02);
    /* asks */
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
    /* полоски останутся видимыми */
    z-index: -1;
}

.bid-bar {
    background-color: #0a0;
}

.ask-bar {
    background-color: #c00;
}
</style>
