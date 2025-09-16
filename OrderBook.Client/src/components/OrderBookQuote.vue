<template>
  <div class="quote-block">
    <label for="btc-input">Amount (BTC):</label>
    <input id="btc-input" type="number" :value="amountBtc"
      @input="emits('update:amountBtc', ($event.target as HTMLInputElement).valueAsNumber)" step="0.0001" min="0" />

    <div v-if="quote" class="quote" :class="{ ok: quote.sufficient, error: !quote.sufficient }">
      <strong>Quote:</strong>
      <template v-if="quote.sufficient">
        {{ fmtBtc(quote.filled) }} ≈ {{ fmtEur(quote.totalCost) }}
        <span class="avg">(avg {{ fmtEur(quote.averagePrice) }})</span>
      </template>
      <template v-else>
        Not sufficient liquidity (filled {{ fmtBtc(quote.filled) }})
      </template>
      <span v-if="quote.timestampUtc" class="timestamp">
        {{ new Date(quote.timestampUtc).toLocaleTimeString() }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { QuoteResult } from "../shared/types/orderBook";

const props = defineProps<{
  amountBtc: number;
  quote: QuoteResult | null;
}>();

const emits = defineEmits<{
  (e: "update:amountBtc", val: number): void;
}>();

function fmtEur(v?: number) { return v !== undefined ? `${v.toFixed(2)} EUR` : "0.00 EUR"; }
function fmtBtc(v?: number) { return v !== undefined ? `${v.toFixed(4)} BTC` : "0.0000 BTC"; }
</script>

<style scoped>
.quote-block {
    display: flex;
    align-items: center;
    gap: 1rem;
    font-family: monospace;
    margin-bottom: 1rem;
}

label {
    font-weight: bold;
    color: #444;
}

input {
    width: 100px;
    padding: 4px 6px;
    font-family: monospace;
    font-size: 0.9rem;
    border: 1px solid #ccc;
    border-radius: 4px;
}

.quote {
    padding: 6px 10px;
    border-radius: 4px;
    font-size: 0.9rem;
}

.quote.ok {
    background: rgba(0, 160, 0, 0.1);
    color: #0a0;
}

.quote.error {
    background: rgba(200, 0, 0, 0.1);
    color: #c00;
}

.quote .avg {
    color: #555;
    margin-left: 6px;
    font-size: 0.8rem;
}

.timestamp {
    margin-left: 8px;
    font-size: 0.75rem;
    color: #888;
}
</style>
