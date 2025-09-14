<template>
    <tr>
        <td class="time">{{ time }}</td>
        <td class="bid">{{ bid?.Price?.toLocaleString() ?? "" }}</td>
        <OrderBookCell side="bid" :amount="bid?.Amount ?? null" :max="maxBid" />

        <td class="ask">{{ ask?.Price?.toLocaleString() ?? "" }}</td>
        <OrderBookCell side="ask" :amount="ask?.Amount ?? null" :max="maxAsk" />
    </tr>
</template>

<script setup lang="ts">
import OrderBookCell from "./OrderBookCell.vue";

interface Order {
    Price: number;
    Amount: number;
}

interface Props {
    time: string;
    bid?: Order;
    ask?: Order;
    maxBid: number;
    maxAsk: number;
}

defineProps<Props>();
</script>

<style scoped>
.time {
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
</style>
