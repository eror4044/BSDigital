<script setup lang="ts">
const props = defineProps<{
    amountBtc: number,
    quote: { total: number; avg: number; filled: number; ok: boolean; ts?: string } | null
}>();
const emits = defineEmits<{
    (e: "update:amountBtc", val: number): void
}>();

function fmtEur(v?: number) { return v !== undefined ? `${v.toFixed(2)} EUR` : "0.00 EUR"; }
function fmtBtc(v?: number) { return v !== undefined ? `${v.toFixed(4)} BTC` : "0.0000 BTC"; }
</script>

<template>
    <div class="quote-block">
        <label>Amount (BTC): </label>
        <input type="number" :value="amountBtc"
            @input="emits('update:amountBtc', ($event.target as HTMLInputElement).valueAsNumber)" step="0.0001"
            min="0" />

        <span v-if="quote" class="quote">
            <strong>Quote:</strong>
            <template v-if="quote.ok">
                {{ fmtBtc(quote.filled) }} ≈ {{ fmtEur(quote.total) }} (avg {{ fmtEur(quote.avg) }})
            </template>
            <template v-else>
                Not sufficient liquidity (filled {{ fmtBtc(quote.filled) }})
            </template>
            <span v-if="quote.ts" class="timestamp">[{{ new Date(quote.ts).toLocaleTimeString() }}]</span>
        </span>
    </div>
</template>
