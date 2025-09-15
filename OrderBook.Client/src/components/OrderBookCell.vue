<template>
  <td :class="[side, 'amount']">
    <div class="bar" :class="side + '-bar'" :style="{ width: barWidth }"></div>
    {{ value }}
  </td>
</template>

<script setup lang="ts">
import type { OrderBookCellProps } from "@/shared/types/props";
import { computed } from "vue";

const props = defineProps<OrderBookCellProps>();

const barWidth = computed(() => {
  if (!props.amount || !props.max) return "0%";
  return `${(props.amount / props.max) * 100}%`;
});

const value = computed(() =>
  props.amount != null ? props.amount.toFixed(6) : ""
);
</script>

<style scoped>
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
